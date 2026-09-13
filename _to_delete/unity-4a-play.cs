if (UnityEditor.EditorApplication.isCompiling || UnityEditor.EditorApplication.isUpdating) return "Compiling; retry after reload.";
if (UnityEditor.EditorApplication.isPlaying) return "Already playing.";
for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
    if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) return "Not switching: unsaved editor scene.";
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/DoNotLeaveMe/Levels/Persistent.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
UnityEditor.EditorApplication.isPlaying = true;
return "Testing saved Persistent startup without saving or changing any scene.";
