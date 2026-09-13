var p=UnityEngine.Object.FindObjectOfType<Level04BParkourController>();
var actors=PlayerActors.Instance;
var flow=UnityEngine.Object.FindObjectOfType<GameFlowController>();
return Codely.Newtonsoft.Json.JsonConvert.SerializeObject(new {playing=UnityEditor.EditorApplication.isPlaying, level=flow?.CurrentLevelScene, phase=p?.CurrentPhase.ToString(),human=actors?.Human?.transform.position.ToString("F3"),dog=actors?.Dog?.transform.position.ToString("F3"),timeScale=UnityEngine.Time.timeScale, errors=UnityTcp.Editor.Tools.ReadConsole.HandleCommand(Codely.Newtonsoft.Json.Linq.JObject.Parse("{\"action\":\"get\",\"count\":10,\"types\":[\"error\"],\"include_stacktrace\":false}"))});
