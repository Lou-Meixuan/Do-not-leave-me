using Pathfinding;
using UnityEngine;

public class DogOrbitFollower : MonoBehaviour
{
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float orbitSpeed = 1.6f;
    [Tooltip("强制跟随时，狗的速度相对于角色 walkSpeed 的倍率。")]
    [SerializeField] private float followSpeedMultiplier = 1.3f;
    [Tooltip("狗与人之间的水平距离小于等于该值时，停止强制跟随。")]
    [SerializeField] private float stopDistance = 2.25f;
    [Tooltip("等待中的狗与人之间的水平距离大于该值时，重新开始强制跟随。")]
    [SerializeField] private float resumeDistance = 3.5f;
    [SerializeField] private float movementThreshold = 0.01f;

    private PlayerActor human;
    private PlayerActor dog;
    private Seeker seeker;
    private Path currentPath;
    private int waypointIndex;
    private float nextRepathTime;
    private float orbitAngle;
    private bool active;
    private Vector3 moveTarget;
    private bool hasMoveTarget;
    private bool waitingNearHuman;

    public bool IsFollowing => active && human != null && dog != null;
    public bool IsWaitingNearHuman => IsFollowing && waitingNearHuman;

    public void BeginOrbit(PlayerActor humanActor, PlayerActor dogActor)
    {
        human = humanActor;
        dog = dogActor;
        seeker = dog != null ? dog.GetComponent<Seeker>() : null;
        if (seeker == null && dog != null)
            seeker = dog.gameObject.AddComponent<Seeker>();
        orbitAngle = 0f;
        active = human != null && dog != null;
        currentPath = null;
        waypointIndex = 0;
        nextRepathTime = 0f;
        hasMoveTarget = false;
        waitingNearHuman = active && FlatDistance(human.transform.position, dog.transform.position) <= StopDistance;
        if (dog != null)
        {
            dog.SetRuntimeMovementSpeedMultiplier(followSpeedMultiplier);
            if (waitingNearHuman)
                EnterWaitingState();
        }
    }

    public void StopOrbit()
    {
        if (dog != null)
        {
            dog.SetExternalDogMovement(false);
            dog.SetRuntimeMovementSpeedMultiplier(1f);
            dog.Stop();
        }

        active = false;
        human = null;
        dog = null;
        currentPath = null;
        hasMoveTarget = false;
        waitingNearHuman = false;
    }

    void Update()
    {
        if (!active || human == null || dog == null)
            return;

        float distanceToHuman = FlatDistance(human.transform.position, dog.transform.position);
        bool shouldWait = EvaluateWaitingState(waitingNearHuman, distanceToHuman, StopDistance, ResumeDistance);
        if (shouldWait)
        {
            if (!waitingNearHuman || hasMoveTarget || currentPath != null)
                EnterWaitingState();
            else
                dog.SetExternalDogMovement(false);
            return;
        }

        if (waitingNearHuman)
        {
            waitingNearHuman = false;
            currentPath = null;
            waypointIndex = 0;
            nextRepathTime = 0f;
            hasMoveTarget = false;
        }

        orbitAngle += orbitSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Cos(orbitAngle), 0f, Mathf.Sin(orbitAngle)) * orbitRadius;
        Vector3 target = human.transform.position + offset;
        target.y = dog.transform.position.y;

        if (seeker != null && HasUsableGraph() && Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + 0.25f;
            seeker.StartPath(dog.transform.position, target, OnPathComplete);
        }

        moveTarget = target;
        if (currentPath != null && !currentPath.error && currentPath.vectorPath != null && currentPath.vectorPath.Count > 0)
        {
            while (waypointIndex < currentPath.vectorPath.Count - 1 &&
                   FlatDistance(dog.transform.position, currentPath.vectorPath[waypointIndex]) < 0.35f)
                waypointIndex++;

            moveTarget = currentPath.vectorPath[waypointIndex];
            moveTarget.y = dog.transform.position.y;
        }

        hasMoveTarget = true;

        Vector3 look = human.transform.position - dog.transform.position;
        look.y = 0f;
        if (look.sqrMagnitude > 0.01f)
            dog.transform.rotation = Quaternion.Slerp(dog.transform.rotation, Quaternion.LookRotation(look), 8f * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (!active || dog == null || waitingNearHuman || !hasMoveTarget)
            return;

        Vector3 direction = moveTarget - dog.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= movementThreshold * movementThreshold)
        {
            dog.Stop();
            return;
        }

        // 通过刚体速度移动，让角色、墙体与场景碰撞都由 Unity 在物理步中解算。
        dog.Move(direction.normalized, false);
    }

    void OnPathComplete(Path path)
    {
        if (!active || waitingNearHuman)
            return;

        currentPath = path;
        waypointIndex = 0;
    }

    void OnValidate()
    {
        stopDistance = Mathf.Max(0f, stopDistance);
        resumeDistance = Mathf.Max(stopDistance + 0.01f, resumeDistance);
    }

    void EnterWaitingState()
    {
        waitingNearHuman = true;
        currentPath = null;
        waypointIndex = 0;
        nextRepathTime = 0f;
        hasMoveTarget = false;
        if (dog != null)
        {
            dog.SetExternalDogMovement(false);
            dog.Stop();
        }
    }

    float StopDistance => Mathf.Max(0f, stopDistance);
    float ResumeDistance => Mathf.Max(StopDistance + 0.01f, resumeDistance);

    public static bool EvaluateWaitingState(bool currentlyWaiting, float distance, float stop, float resume)
    {
        float safeStop = Mathf.Max(0f, stop);
        float safeResume = Mathf.Max(safeStop + 0.01f, resume);
        return currentlyWaiting ? distance <= safeResume : distance <= safeStop;
    }

    static float FlatDistance(Vector3 first, Vector3 second)
    {
        first.y = 0f;
        second.y = 0f;
        return Vector3.Distance(first, second);
    }

    static bool HasUsableGraph()
    {
        return AstarPath.active != null &&
            AstarPath.active.data != null &&
            AstarPath.active.data.graphs != null &&
            AstarPath.active.data.graphs.Length > 0;
    }
}
