using UnityEngine;

[AddComponentMenu("DoNotLeaveMe/Level 04B/Parkour Dog Runner")]
public sealed class Level04BParkourDogRunner : MonoBehaviour
{
    [SerializeField] private Transform[] chasePath = new Transform[0];
    [SerializeField] private Transform[] leftBranchPath = new Transform[0];
    [SerializeField] private Transform[] rightBranchPath = new Transform[0];
    [SerializeField] private Transform[] walkoutPath = new Transform[0];
    [SerializeField, Min(0.1f)] private float runSpeed = 8f;
    [SerializeField, Min(0.1f)] private float walkSpeed = 3.5f;

    private PlayerActor dog;
    private Transform[] activePath;
    private int waypointIndex;
    private bool running;

    public bool IsComplete => activePath == null || waypointIndex >= activePath.Length;

    public void Bind(PlayerActor dogActor)
    {
        dog = dogActor;
    }

    public void BeginChase()
    {
        BeginPath(chasePath, true);
    }

    public void BeginBranch(bool left)
    {
        BeginPath(left ? leftBranchPath : rightBranchPath, true);
    }

    public void BeginWalkout()
    {
        BeginPath(walkoutPath, false);
    }

    public void Tick(float deltaTime)
    {
        if (dog == null || IsComplete || deltaTime <= 0f)
            return;

        Transform waypoint = activePath[waypointIndex];
        if (waypoint == null)
        {
            waypointIndex++;
            return;
        }

        Vector3 current = dog.transform.position;
        Vector3 target = waypoint.position;
        target.y = current.y;
        float speed = running ? runSpeed : walkSpeed;
        Vector3 next = Vector3.MoveTowards(current, target, speed * deltaTime);
        Vector3 direction = target - current;
        direction.y = 0f;
        Quaternion rotation = direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(direction, Vector3.up)
            : dog.transform.rotation;
        dog.SetParkourDogPose(next, rotation, running);

        if (FlatDistance(next, target) <= 0.08f)
            waypointIndex++;
    }

    public void ResetRunner()
    {
        activePath = null;
        waypointIndex = 0;
        if (dog != null)
            dog.SetExternalDogMovement(false);
    }

    void BeginPath(Transform[] path, bool useRun)
    {
        activePath = path;
        waypointIndex = 0;
        running = useRun;
    }

    static float FlatDistance(Vector3 first, Vector3 second)
    {
        first.y = second.y = 0f;
        return Vector3.Distance(first, second);
    }

    void OnDrawGizmosSelected()
    {
        DrawPath(chasePath, Color.cyan);
        DrawPath(leftBranchPath, Color.blue);
        DrawPath(rightBranchPath, Color.magenta);
        DrawPath(walkoutPath, Color.green);
    }

    static void DrawPath(Transform[] path, Color color)
    {
        if (path == null)
            return;
        Gizmos.color = color;
        for (int i = 0; i < path.Length; i++)
        {
            if (path[i] == null)
                continue;
            Gizmos.DrawSphere(path[i].position, 0.15f);
            if (i > 0 && path[i - 1] != null)
                Gizmos.DrawLine(path[i - 1].position, path[i].position);
        }
    }
}
