using UnityEngine;

[AddComponentMenu("DoNotLeaveMe/Level 04B/Parkour Obstacle")]
public sealed class Level04BParkourObstacle : MonoBehaviour, ILevelTemporaryState
{
    [SerializeField, Min(0.05f)] private float lateWindowSeconds = 0.65f;

    private Level04BParkourController controller;
    private bool humanInsideEvaluation;
    private bool lethalHit;
    private float evaluationEnteredAt;

    public void Bind(Level04BParkourController owner)
    {
        controller = owner;
    }

    public void EnterEvaluation(Collider other)
    {
        if (!LevelActors.IsHuman(other) || controller == null || !controller.IsChaseActive)
            return;
        humanInsideEvaluation = true;
        lethalHit = false;
        evaluationEnteredAt = Time.time;
    }

    public void ExitEvaluation(Collider other)
    {
        if (!LevelActors.IsHuman(other) || !humanInsideEvaluation)
            return;

        humanInsideEvaluation = false;
        if (!lethalHit && controller != null && Time.time - evaluationEnteredAt <= lateWindowSeconds)
            controller.ReportNearMiss(this);
    }

    public void EnterLethal(Collider other)
    {
        if (!LevelActors.IsHuman(other) || controller == null || !controller.IsChaseActive)
            return;
        lethalHit = true;
        controller.FailParkour("obstacle:" + name);
    }

    public void ResetTemporaryState()
    {
        humanInsideEvaluation = false;
        lethalHit = false;
        evaluationEnteredAt = 0f;
    }
}
