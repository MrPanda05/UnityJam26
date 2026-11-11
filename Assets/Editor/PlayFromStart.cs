using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromStart
{
    static PlayFromStart()
    {
        // Reset after every play session so the normal Play button
        // goes back to "play current scene".
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                EditorSceneManager.playModeStartScene = null;
        };
    }

    [MenuItem("Tools/Play From Start _F5")]
    private static void Play()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes in Build Settings.");
            return;
        }

        var startScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[0].path);
        if (startScene == null)
        {
            Debug.LogError($"Could not load start scene at {scenes[0].path}");
            return;
        }

        // Prompt to save unsaved changes in the open scenes first.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorSceneManager.playModeStartScene = startScene;
        EditorApplication.EnterPlaymode();
    }
}