using UnityEngine;

[AddComponentMenu("DoNotLeaveMe/Level 04B/Parkour Near Miss Trigger")]
public sealed class Level04BParkourNearMissTrigger : MonoBehaviour
{
    [SerializeField] private Level04BParkourObstacle obstacle;

    void Awake()
    {
        if (obstacle == null)
            obstacle = GetComponentInParent<Level04BParkourObstacle>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (obstacle != null)
            obstacle.EnterEvaluation(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (obstacle != null)
            obstacle.ExitEvaluation(other);
    }
}
