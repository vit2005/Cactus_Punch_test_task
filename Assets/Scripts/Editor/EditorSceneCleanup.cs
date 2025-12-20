#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TowerDefence.Editor
{
    /// <summary>
    /// Cleans up Editor references when switching scenes to prevent null reference exceptions
    /// Place this file in an Editor folder
    /// </summary>
    [InitializeOnLoad]
    public static class EditorSceneCleanup
    {
        static EditorSceneCleanup()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosing += OnSceneClosing;
        }

        private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            // Force editor to release references
            EditorUtility.ClearProgressBar();

            // Clear selection to prevent inspector errors
            Selection.activeObject = null;

            // Force repaint
            EditorApplication.delayCall += () =>
            {
                if (EditorWindow.focusedWindow != null)
                {
                    EditorWindow.focusedWindow.Repaint();
                }
            };
        }

        private static void OnSceneClosing(UnityEngine.SceneManagement.Scene scene, bool removingScene)
        {
            // Clear selection before scene unloads
            Selection.activeObject = null;

            // Clear inspector
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }
    }
}
#endif