using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ArtOverviewTools
{
    private static readonly string[] ArtScenes =
    {
        "Assets/DoNotLeaveMe/Levels/Level_01.unity",
        "Assets/DoNotLeaveMe/Levels/Level_02.unity",
        "Assets/DoNotLeaveMe/Levels/Level_03.unity",
        "Assets/DoNotLeaveMe/Levels/Level_04.unity",
        "Assets/DoNotLeaveMe/Levels/Level_04B.unity",
        "Assets/DoNotLeaveMe/Levels/Level_05.unity",
        "Assets/DoNotLeaveMe/Levels/SharedArt_L01_L02.unity",
        "Assets/DoNotLeaveMe/Levels/SharedArt_L02_L03.unity",
        "Assets/DoNotLeaveMe/Levels/SharedArt_L03_L04.unity",
        "Assets/DoNotLeaveMe/Levels/SharedArt_L04_L04B.unity",
        "Assets/DoNotLeaveMe/Levels/SharedArt_L04B_L05.unity"
    };

    [MenuItem("Tools/DoNotLeaveMe/Art/Open All Art Scenes Additively")]
    public static void OpenAllArtScenesAdditively()
    {
        EditorSceneManager.OpenScene(ArtScenes[0], OpenSceneMode.Single);
        for (int i = 1; i < ArtScenes.Length; i++)
            EditorSceneManager.OpenScene(ArtScenes[i], OpenSceneMode.Additive);
    }

    [MenuItem("Tools/DoNotLeaveMe/Art/Close All Art Scenes Except Active Scene")]
    public static void CloseAllArtScenesExceptActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene != activeScene && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }
    }
}
