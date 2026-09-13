System.Collections.IEnumerator Run4A()
{
    float deadline = UnityEngine.Time.realtimeSinceStartup + 55f;
    var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
    Level04BParkourController p = null;
    while (p == null && UnityEngine.Time.realtimeSinceStartup < deadline)
    {
        p = UnityEngine.Object.FindObjectOfType<Level04BParkourController>();
        yield return null;
    }
    if (p == null) throw new System.Exception("4A controller was not loaded.");
    if (DeathScreen.Instance != null) DeathScreen.Instance.Restart();
    yield return null;
    yield return null;
    var input = p.GetType().GetMethod("HandleHorizontalInput", flags);
    var jump = p.GetType().GetMethod("TryJump", flags);
    var indexField = p.GetType().GetField("segmentIndex", flags);
    var distanceField = p.GetType().GetField("segmentDistance", flags);
    var routes = (Level04BParkourController.RouteSegment[])p.GetType().GetField("segments", flags).GetValue(p);
    int maxSegment = 0;
    float maxY = -1000, minDogY = 1000;
    bool jumped = false;
    var phases = new System.Collections.Generic.HashSet<string>();
    while (UnityEngine.Time.realtimeSinceStartup < deadline)
    {
        phases.Add(p.CurrentPhase.ToString());
        var actors = PlayerActors.Instance;
        if (actors != null && actors.Human != null && actors.Dog != null)
        {
            maxY = UnityEngine.Mathf.Max(maxY, actors.Human.transform.position.y);
            minDogY = UnityEngine.Mathf.Min(minDogY, actors.Dog.transform.position.y);
        }
        if (p.CurrentPhase == Level04BParkourController.Phase.Chase)
        {
            int index = (int)indexField.GetValue(p);
            maxSegment = System.Math.Max(maxSegment, index);
            float distance = (float)distanceField.GetValue(p);
            var route = routes[index];
            if (route.Length - distance > route.decisionWindow)
            {
                if (p.Lane != -1) input.Invoke(p, new object[] { Level04BParkourController.TurnDirection.Left });
                if (index == 0 && distance > 6 && !jumped)
                {
                    jump.Invoke(p, null);
                    jumped = true;
                }
            }
            else
            {
                var turn = route.requiredTurn == Level04BParkourController.TurnDirection.Either ? Level04BParkourController.TurnDirection.Left : route.requiredTurn;
                if (turn != Level04BParkourController.TurnDirection.None) input.Invoke(p, new object[] { turn });
            }
        }
        if (p.CurrentPhase == Level04BParkourController.Phase.Released || p.CurrentPhase == Level04BParkourController.Phase.Failed) break;
        yield return null;
    }
    var flow = UnityEngine.Object.FindObjectOfType<GameFlowController>();
    string result = Codely.Newtonsoft.Json.JsonConvert.SerializeObject(new { level=flow.CurrentLevelScene, phase=p.CurrentPhase.ToString(), maxSegment, maxY, minDogY, jumped, phases, human=PlayerActors.Instance.Human.transform.position.ToString("F3"),dog=PlayerActors.Instance.Dog.transform.position.ToString("F3") });
    System.IO.File.WriteAllText("../_to_delete/4A-runtime-test.json", result);
    UnityEngine.Debug.Log("4A RUN TEST: " + result);
}
return Run4A();
