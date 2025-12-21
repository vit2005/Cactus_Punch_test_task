using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefence.Systems
{
    public class SceneLoaderService : ISceneLoader
    {
        private readonly HashSet<string> _loadingScenes = new HashSet<string>();
        private readonly HashSet<string> _loadedScenes = new HashSet<string>();

        public void Init()
        {
            // Subscribe to scene loaded/unloaded events to keep track of loaded scenes
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            // Track initially loaded scenes
            _loadedScenes.Clear();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    _loadedScenes.Add(scene.name);
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _loadedScenes.Add(scene.name);
            Debug.Log($"[SceneLoader] Scene loaded callback: {scene.name}");
        }

        private void OnSceneUnloaded(Scene scene)
        {
            _loadedScenes.Remove(scene.name);
            Debug.Log($"[SceneLoader] Scene unloaded callback: {scene.name}");
        }

        public async Task LoadSceneAsync(string sceneName, LoadSceneMode mode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));
            }

            if (_loadingScenes.Contains(sceneName))
            {
                Debug.LogWarning($"Scene '{sceneName}' is already being loaded.");
                return;
            }

            // For Single mode, we always reload (Unity will handle unloading current scene)
            // For Additive mode, check if already loaded
            if (mode == LoadSceneMode.Additive && _loadedScenes.Contains(sceneName))
            {
                Debug.LogWarning($"Scene '{sceneName}' is already loaded in additive mode.");
                return;
            }

            _loadingScenes.Add(sceneName);

            try
            {
                // If loading in Single mode, clear our tracked scenes as Unity will unload everything
                if (mode == LoadSceneMode.Single)
                {
                    _loadedScenes.Clear();
                }

                var operation = SceneManager.LoadSceneAsync(sceneName, mode);
                if (operation == null)
                {
                    throw new InvalidOperationException($"Failed to start loading scene '{sceneName}'.");
                }

                operation.allowSceneActivation = false;

                while (operation.progress < 0.9f)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Debug.LogWarning($"Scene loading cancelled: {sceneName}");
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await Task.Yield();
                }

                operation.allowSceneActivation = true;

                while (!operation.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Debug.LogWarning($"Scene loading cancelled: {sceneName}");
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await Task.Yield();
                }

                // The callback will add the scene to _loadedScenes
                Debug.Log($"Scene loaded: {sceneName}");
            }
            finally
            {
                _loadingScenes.Remove(sceneName);
            }
        }

        public async Task UnloadSceneAsync(string sceneName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));
            }

            if (!_loadedScenes.Contains(sceneName))
            {
                Debug.LogWarning($"Scene '{sceneName}' is not loaded.");
                return;
            }

            var operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation == null)
            {
                throw new InvalidOperationException($"Failed to start unloading scene '{sceneName}'.");
            }

            while (!operation.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.LogWarning($"Scene unloading cancelled: {sceneName}");
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await Task.Yield();
            }

            // The callback will remove the scene from _loadedScenes
            Debug.Log($"Scene unloaded: {sceneName}");
        }

        public bool IsSceneLoaded(string sceneName) => _loadedScenes.Contains(sceneName);

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            _loadingScenes.Clear();
            _loadedScenes.Clear();
        }
    }
}