var scenes = string.Join("; ", System.Linq.Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount).Select(i => { var s=UnityEngine.SceneManagement.SceneManager.GetSceneAt(i); return s.path+" dirty="+s.isDirty; }));
var controllers = UnityEngine.Object.FindObjectsOfType<Level04BParkourController>(true);
var info = string.Join("; ", controllers.Select(c => c.gameObject.scene.name+" active="+c.isActiveAndEnabled+" phase="+c.CurrentPhase+" valid="+c.ValidateRoute(out var e)+" error="+e));
var console = UnityTcp.Editor.Tools.ReadConsole.HandleCommand(Codely.Newtonsoft.Json.Linq.JObject.Parse("{\"action\":\"get\",\"count\":30,\"include_stacktrace\":true}"));
return Codely.Newtonsoft.Json.JsonConvert.SerializeObject(new { playing=UnityEditor.EditorApplication.isPlaying, scenes, info, console });
