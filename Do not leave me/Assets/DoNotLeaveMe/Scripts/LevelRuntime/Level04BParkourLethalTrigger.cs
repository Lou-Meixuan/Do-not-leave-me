using UnityEngine;

[AddComponentMenu("DoNotLeaveMe/Level 04B/Parkour Lethal Trigger")]
public sealed class Level04BParkourLethalTrigger : MonoBehaviour
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
            obstacle.EnterLethal(other);
    }
}
