using System;
using System.Threading;
using TowerDefence.Core;
using TowerDefence.Systems;
using TowerDefence.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefence.Game
{
    public class MenuState : IState
    {
        private IEventBus _eventBus;
        private IEventToken _startGameToken;
        private CancellationTokenSource _cts;
        private bool _isTransitioning;

        public async void OnEnter()
        {
            _cts = new CancellationTokenSource();

            // Wait for scene to fully initialize (3 frames is usually enough)
            for (int i = 0; i < 3; i++)
            {
                await System.Threading.Tasks.Task.Yield();
                if (_cts.Token.IsCancellationRequested) return;
            }

            var screenRouter = Services.Get<IScreenRouter>();
            screenRouter?.Clear();

            _eventBus = Services.Get<IEventBus>();
            _startGameToken = _eventBus?.Subscribe<StartGameRequestedEvent>(OnStartGameRequested);

            // Try to find MainMenuScreen with retries
            var uiRegistry = Services.Get<IUIRegistry>();
            MainMenuScreen mainMenu = null;

            // Retry up to 10 times with small delays
            for (int attempt = 0; attempt < 10; attempt++)
            {
                if (uiRegistry.TryGetScreen<MainMenuScreen>("MainMenu", out mainMenu))
                {
                    Debug.Log($"MainMenuScreen found on attempt {attempt + 1}");
                    break;
                }

                Debug.LogWarning($"MainMenuScreen not found, retrying... (attempt {attempt + 1}/10)");
                await System.Threading.Tasks.Task.Delay(100, _cts.Token);

                if (_cts.Token.IsCancellationRequested) return;
            }

            if (mainMenu != null)
            {
                try
                {
                    await screenRouter.PushAsync(mainMenu, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Menu screen push was cancelled");
                }
            }
            else
            {
                Debug.LogError("MainMenuScreen not found in UIRegistry after 10 attempts! " +
                              "Make sure GameObject with MainMenuScreen component exists in Menu scene with ScreenId='MainMenu' " +
                              "and is ACTIVE in hierarchy.");
            }
        }

        public void OnExit()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            if (_eventBus != null && _startGameToken != null)
            {
                _eventBus.Unsubscribe(_startGameToken);
            }
        }

        public void Tick(float deltaTime) { }

        private async void OnStartGameRequested(StartGameRequestedEvent evt)
        {
            if (_isTransitioning)
            {
                return;
            }

            _isTransitioning = true;

            try
            {
                var sceneLoader = Services.Get<ISceneLoader>();
                var config = Services.Get<GameConfig>();

                if (sceneLoader == null || config == null)
                {
                    Debug.LogError("SceneLoader or GameConfig is null!");
                    return;
                }

                using var loadCts = new CancellationTokenSource();

                await sceneLoader.LoadSceneAsync(
                    config.GameSceneName,
                    LoadSceneMode.Single,
                    loadCts.Token
                );

                // Wait for scene to initialize
                for (int i = 0; i < 3; i++)
                {
                    await System.Threading.Tasks.Task.Yield();
                }

                var stateMachine = Services.Get<IStateMachine>();
                stateMachine?.SetState(new GameplayState());
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Game start was cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to start game: {ex}");
            }
            finally
            {
                _isTransitioning = false;
            }
        }
    }
}