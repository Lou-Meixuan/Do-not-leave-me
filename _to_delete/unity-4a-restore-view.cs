if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return "Waiting for play mode to stop.";
for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
    if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) return "Kept unsaved scene open.";
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/DoNotLeaveMe/Levels/Start.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
return "Restored the original editor scene; no scenes saved.";
