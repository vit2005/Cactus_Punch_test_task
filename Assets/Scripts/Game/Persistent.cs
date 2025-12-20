using TowerDefence.Core;
using TowerDefence.Systems;
using UnityEngine;

namespace TowerDefence.Game
{
    /// <summary>
    /// Persistent GameObject that survives scene changes.
    /// Holds TickDispatcher and keeps ServiceLocator alive.
    /// </summary>
    public sealed class Persistent : MonoBehaviour
    {
        private static Persistent _instance;
        private ITickDispatcher _tickDispatcher;
        private bool _isQuitting;

        public void Initialize()
        {
            // Singleton pattern to prevent duplicates
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Duplicate Persistent instance detected. Destroying...");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeTickDispatcher();
            InitializeStateMachine();
        }

        private void InitializeTickDispatcher()
        {
            _tickDispatcher = gameObject.AddComponent<TickDispatcher>();
            _tickDispatcher.Init();

            var locator = Services.Get<IServiceLocator>();
            if (locator != null)
            {
                locator.Register(_tickDispatcher);
            }
        }

        private void InitializeStateMachine()
        {
            var stateMachine = Services.Get<IStateMachine>();
            if (stateMachine != null)
            {
                _tickDispatcher.Subscribe(deltaTime => stateMachine?.Tick(deltaTime));
            }
        }

        private void OnDestroy()
        {
            if (_isQuitting || _instance != this)
            {
                return;
            }

            _instance = null;
            Cleanup();
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
            Cleanup();
        }

        private void Cleanup()
        {
            // Clear tick dispatcher
            if (_tickDispatcher != null)
            {
                _tickDispatcher.Clear();
            }

            // Clear event bus
            if (Services.TryGet<IEventBus>(out var eventBus))
            {
                eventBus.Clear();
            }

            // Dispose scene loader
            if (Services.TryGet<ISceneLoader>(out var sceneLoader))
            {
                sceneLoader.Dispose();
            }

            // Clear service locator last
            if (Services.TryGet<IServiceLocator>(out var locator))
            {
                locator.Clear();
            }
        }
    }
}