using System;
using System.Threading;
using TowerDefence.Core;
using TowerDefence.Systems;
using TowerDefence.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefence.Game
{
    public class GameOverState : IState
    {
        private IEventBus _eventBus;
        private IEventToken _restartToken;
        private IEventToken _returnToMenuToken;
        private CancellationTokenSource _cts;
        private bool _isTransitioning;

        public async void OnEnter()
        {
            _cts = new CancellationTokenSource();

            // Wait a frame for scene to stabilize
            await System.Threading.Tasks.Task.Yield();
            if (_cts.Token.IsCancellationRequested) return;

            _eventBus = Services.Get<IEventBus>();
            _restartToken = _eventBus?.Subscribe<StartGameRequestedEvent>(OnRestartRequested);
            _returnToMenuToken = _eventBus?.Subscribe<ReturnToMenuRequestedEvent>(OnReturnToMenuRequested);

            // Show GameOver screen
            var uiRegistry = Services.Get<IUIRegistry>();
            if (uiRegistry.TryGetScreen<IScreen>("GameOver", out var gameOverScreen))
            {
                var screenRouter = Services.Get<IScreenRouter>();

                try
                {
                    await screenRouter.ShowModalAsync(gameOverScreen, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("GameOver screen show was cancelled");
                }
            }
            else
            {
                Debug.LogWarning("GameOver screen not found in UIRegistry. Make sure it exists in Gameplay scene with ScreenId='GameOver'");
            }
        }

        public void OnExit()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            if (_eventBus != null)
            {
                if (_restartToken != null)
                {
                    _eventBus.Unsubscribe(_restartToken);
                }

                if (_returnToMenuToken != null)
                {
                    _eventBus.Unsubscribe(_returnToMenuToken);
                }
            }

            Time.timeScale = 1f;
        }

        public void Tick(float deltaTime) { }

        private async void OnRestartRequested(StartGameRequestedEvent evt)
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

                // Hide GameOver modal first
                var screenRouter = Services.Get<IScreenRouter>();
                await screenRouter.HideModalAsync();

                using var loadCts = new CancellationTokenSource();

                // Reload the gameplay scene (Single mode will unload current and load fresh)
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

                // Return to GameplayState to start fresh
                var stateMachine = Services.Get<IStateMachine>();
                stateMachine?.SetState(new GameplayState());
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Game restart was cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to restart game: {ex}");
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private async void OnReturnToMenuRequested(ReturnToMenuRequestedEvent evt)
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

                // Hide GameOver modal first
                var screenRouter = Services.Get<IScreenRouter>();
                await screenRouter.HideModalAsync();

                using var loadCts = new CancellationTokenSource();

                // Load menu scene
                await sceneLoader.LoadSceneAsync(
                    config.MenuSceneName,
                    LoadSceneMode.Single,
                    loadCts.Token
                );

                // Wait for scene to initialize
                for (int i = 0; i < 3; i++)
                {
                    await System.Threading.Tasks.Task.Yield();
                }

                // Switch to MenuState
                var stateMachine = Services.Get<IStateMachine>();
                stateMachine?.SetState(new MenuState());
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Return to menu was cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to return to menu: {ex}");
            }
            finally
            {
                _isTransitioning = false;
            }
        }
    }
}