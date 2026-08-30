using System;
using System.Collections;
using UnityEngine;

[AddComponentMenu("DoNotLeaveMe/Level 04B/Three Lane Parkour")]
public sealed class Level04BParkourController : MonoBehaviour, ILevelTemporaryState
{
    public enum Phase { Inactive, Chase, Branch, Walkout, Tutorial, Released, Failed }
    public enum TurnDirection { None, Left, Right, Either }

    [Serializable]
    public sealed class RouteSegment
    {
        public string name = "Segment";
        public Transform start;
        public Transform end;
        [Min(0.25f)] public float laneWidth = 1.8f;
        [Min(0.1f)] public float runSpeed = 8f;
        [Min(0.1f)] public float decisionWindow = 4f;
        public TurnDirection requiredTurn = TurnDirection.None;
        public bool finalJunction;

        public float Length => start != null && end != null ? Vector3.Distance(start.position, end.position) : 0f;
        public Vector3 Forward => start != null && end != null
            ? (end.position - start.position).normalized
            : Vector3.forward;
        public Vector3 Right => Vector3.Cross(Vector3.up, Forward).normalized;
    }

    [Header("Actors and ownership")]
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Level04BParkourDogRunner dogRunner;
    [SerializeField] private Transform cameraPose;
    [SerializeField] private Transform monsterPresentation;

    [Header("Route")]
    [SerializeField] private RouteSegment[] segments = new RouteSegment[0];
    [SerializeField] private Transform[] leftBranchPath = new Transform[0];
    [SerializeField] private Transform[] rightBranchPath = new Transform[0];
    [SerializeField] private Transform mergeAnchor;
    [SerializeField] private Transform[] walkoutPath = new Transform[0];
    [SerializeField] private bool beginOnStart = true;
    [SerializeField, Min(0.05f)] private float laneChangeSeconds = 0.22f;
    [SerializeField, Min(0.1f)] private float branchSpeed = 7f;
    [SerializeField, Min(0.1f)] private float walkSpeed = 3.2f;

    [Header("Camera")]
    [SerializeField] private Vector3 chaseCameraOffset = new Vector3(0f, 3.2f, -6f);
    [SerializeField] private Vector3 walkCameraOffset = new Vector3(2.5f, 2.4f, -5f);
    [SerializeField, Min(0.1f)] private float cameraBlendSpeed = 8f;

    [Header("Chase pressure")]
    [SerializeField, Range(0f, 1f)] private float baselineProximity = 0.3f;
    [SerializeField, Range(0f, 1f)] private float nearMissPressure = 0.24f;
    [SerializeField, Min(0f)] private float recoveryDelay = 1.8f;
    [SerializeField, Min(0f)] private float recoveryPerSecond = 0.08f;
    [SerializeField, Range(0f, 1f)] private float barkThreshold = 0.55f;
    [SerializeField, Min(0.1f)] private float barkCooldown = 2.5f;
    [SerializeField, Min(1f)] private float monsterFarDistance = 12f;
    [SerializeField, Min(0.5f)] private float monsterNearDistance = 3f;
    [SerializeField] private AK.Wwise.Event dogWarningBark = new AK.Wwise.Event();

    [Header("Completion")]
    [SerializeField] private string completionMessage = "怪物似乎追丢了";
    [SerializeField, Min(0.5f)] private float messageSeconds = 2.8f;

    private PlayerActor human;
    private PlayerActor dog;
    private DogOrbitFollower dogOrbit;
    private Phase phase = Phase.Inactive;
    private int segmentIndex;
    private float segmentDistance;
    private int lane;
    private int laneFrom;
    private int laneTarget;
    private float laneBlend = 1f;
    private TurnDirection queuedTurn;
    private Transform[] activePath;
    private int pathIndex;
    private float pathStuckSeconds;
    private float previousPathDistance = float.PositiveInfinity;
    private float proximity;
    private float lastPressureTime;
    private float nextBarkTime;
    private bool warnedMissingBark;
    private bool ownershipHeld;
    private bool dogOrbitWasFollowing;
    private float tutorialEndsAt;

    public Phase CurrentPhase => phase;
    public bool IsChaseActive => phase == Phase.Chase || phase == Phase.Branch;
    public float Proximity => proximity;
    public int Lane => laneTarget;
    public event Action BarkRequested;

    void Start()
    {
        if (beginOnStart)
            StartCoroutine(BeginWhenReady());
    }

    void Update()
    {
        if (phase == Phase.Chase)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
                TryJump();

            if (Input.GetKeyDown(KeyCode.A))
                HandleHorizontalInput(TurnDirection.Left);
            else if (Input.GetKeyDown(KeyCode.D))
                HandleHorizontalInput(TurnDirection.Right);
        }

        if (phase == Phase.Tutorial && Time.unscaledTime >= tutorialEndsAt)
            ReleaseParkour();
    }

    void FixedUpdate()
    {
        if (phase == Phase.Chase)
            TickChase(Time.fixedDeltaTime);
        else if (phase == Phase.Branch)
            TickPath(Time.fixedDeltaTime, true);
        else if (phase == Phase.Walkout)
            TickPath(Time.fixedDeltaTime, false);

        if (phase == Phase.Chase || phase == Phase.Branch || phase == Phase.Walkout)
        {
            if (dogRunner != null)
                dogRunner.Tick(Time.fixedDeltaTime);
            UpdateCameraPose();
            UpdatePressure(Time.fixedDeltaTime);
        }
    }

    public void BeginParkour()
    {
        ResolveDependencies();
        if (human == null || playerControl == null || segments == null || segments.Length == 0 || !SegmentIsValid(segments[0]))
        {
            Debug.LogError("[Level04BParkour] Cannot begin: actors, control, or route is incomplete.", this);
            return;
        }

        AcquireOwnership();
        segmentIndex = 0;
        segmentDistance = 0f;
        lane = laneFrom = laneTarget = 0;
        laneBlend = 1f;
        queuedTurn = TurnDirection.None;
        proximity = baselineProximity;
        lastPressureTime = Time.time;
        nextBarkTime = 0f;
        phase = Phase.Chase;

        RouteSegment first = segments[0];
        human.SetPositionAndRotation(first.start.position, Quaternion.LookRotation(first.Forward, Vector3.up));
        if (dogRunner != null)
        {
            dogRunner.Bind(dog);
            dogRunner.BeginChase();
        }
        BindObstacles();
        UpdateCameraPose(true);
    }

    void TickChase(float deltaTime)
    {
        if (segmentIndex < 0 || segmentIndex >= segments.Length)
        {
            BeginWalkout();
            return;
        }

        RouteSegment segment = segments[segmentIndex];
        if (!SegmentIsValid(segment))
        {
            FailParkour("invalid-segment:" + segmentIndex);
            return;
        }

        segmentDistance += segment.runSpeed * deltaTime;
        laneBlend = Mathf.Min(1f, laneBlend + deltaTime / Mathf.Max(0.01f, laneChangeSeconds));
        float laneValue = Mathf.Lerp(laneFrom, laneTarget, Smooth01(laneBlend));
        float t = Mathf.Clamp01(segmentDistance / Mathf.Max(0.01f, segment.Length));
        Vector3 center = Vector3.Lerp(segment.start.position, segment.end.position, t);
        Vector3 position = center + segment.Right * (laneValue * segment.laneWidth);
        human.SetParkourHorizontalPose(position, Quaternion.LookRotation(segment.Forward, Vector3.up), true);

        if (segmentDistance < segment.Length)
            return;

        bool needsTurn = segment.requiredTurn != TurnDirection.None;
        if (needsTurn && queuedTurn == TurnDirection.None)
        {
            FailParkour("missed-turn:" + segment.name);
            return;
        }

        if (needsTurn && segment.requiredTurn != TurnDirection.Either && queuedTurn != segment.requiredTurn)
        {
            FailParkour("wrong-turn:" + segment.name);
            return;
        }

        if (segment.finalJunction)
        {
            BeginBranch(queuedTurn == TurnDirection.Left);
            return;
        }

        segmentIndex++;
        segmentDistance = 0f;
        queuedTurn = TurnDirection.None;
    }

    void HandleHorizontalInput(TurnDirection direction)
    {
        RouteSegment segment = segmentIndex >= 0 && segmentIndex < segments.Length ? segments[segmentIndex] : null;
        if (segment == null)
            return;

        float remaining = segment.Length - segmentDistance;
        if (segment.requiredTurn != TurnDirection.None && remaining <= segment.decisionWindow)
        {
            queuedTurn = direction;
            return;
        }

        int delta = direction == TurnDirection.Left ? -1 : 1;
        int next = Mathf.Clamp(laneTarget + delta, -1, 1);
        if (next == laneTarget)
            return;
        laneFrom = Mathf.RoundToInt(Mathf.Lerp(laneFrom, laneTarget, Smooth01(laneBlend)));
        laneTarget = next;
        laneBlend = 0f;
        lane = laneTarget;
    }

    void TryJump()
    {
        if (human != null && human.IsGroundedForParkour)
            human.Jump();
    }

    void BeginBranch(bool choseLeft)
    {
        phase = Phase.Branch;
        activePath = choseLeft ? leftBranchPath : rightBranchPath;
        pathIndex = 0;
        ResetPathWatchdog();
        if (dogRunner != null)
            dogRunner.BeginBranch(choseLeft);

        Transform[] monsterPath = choseLeft ? rightBranchPath : leftBranchPath;
        if (monsterPresentation != null && monsterPath != null && monsterPath.Length > 0 && monsterPath[0] != null)
            monsterPresentation.position = monsterPath[0].position;

        if (activePath == null || activePath.Length == 0)
            CompleteBranch();
    }

    void TickPath(float deltaTime, bool running)
    {
        if (activePath == null || pathIndex >= activePath.Length)
        {
            if (phase == Phase.Branch)
                CompleteBranch();
            else
                CompleteWalkout();
            return;
        }

        Transform waypoint = activePath[pathIndex];
        if (waypoint == null)
        {
            pathIndex++;
            ResetPathWatchdog();
            return;
        }

        Vector3 current = human.transform.position;
        Vector3 target = waypoint.position;
        target.y = current.y;
        float speed = running ? branchSpeed : walkSpeed;
        Vector3 next = Vector3.MoveTowards(current, target, speed * deltaTime);
        Vector3 direction = target - current;
        direction.y = 0f;
        Quaternion rotation = direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(direction, Vector3.up)
            : human.transform.rotation;
        human.SetParkourHorizontalPose(next, rotation, running);
        float remaining = FlatDistance(human.transform.position, target);
        if (remaining <= 0.08f)
        {
            pathIndex++;
            ResetPathWatchdog();
            return;
        }

        if (remaining < previousPathDistance - 0.005f)
            pathStuckSeconds = 0f;
        else
            pathStuckSeconds += deltaTime;
        previousPathDistance = remaining;

        if (pathStuckSeconds >= 0.8f)
        {
            Debug.LogWarning("[Level04BParkour] Path movement was blocked; advancing to the authored waypoint.", this);
            human.SetPositionAndRotation(waypoint.position, rotation);
            pathIndex++;
            ResetPathWatchdog();
        }
    }

    void CompleteBranch()
    {
        if (mergeAnchor != null)
        {
            human.SetPositionAndRotation(mergeAnchor.position, mergeAnchor.rotation);
            if (dog != null)
                dog.SetPositionAndRotation(mergeAnchor.position - mergeAnchor.right * 1.4f, mergeAnchor.rotation);
        }
        BeginWalkout();
    }

    void BeginWalkout()
    {
        phase = Phase.Walkout;
        activePath = walkoutPath;
        pathIndex = 0;
        ResetPathWatchdog();
        proximity = 0f;
        if (dogRunner != null)
            dogRunner.BeginWalkout();
        if (activePath == null || activePath.Length == 0)
            CompleteWalkout();
    }

    void CompleteWalkout()
    {
        phase = Phase.Tutorial;
        tutorialEndsAt = Time.unscaledTime + messageSeconds;
        if (human != null)
            human.Stop();
        if (dog != null)
            dog.Stop();
    }

    public void ReportNearMiss(Level04BParkourObstacle source)
    {
        if (!IsChaseActive)
            return;
        proximity = Mathf.Clamp01(proximity + nearMissPressure);
        lastPressureTime = Time.time;
    }

    public void FailParkour(string reason)
    {
        if (phase == Phase.Failed || phase == Phase.Released)
            return;
        phase = Phase.Failed;
        if (human != null)
            human.Stop();
        if (dog != null)
            dog.Stop();
        ReleaseOwnership();
        Debug.Log("[Level04BParkour] Failed: " + reason, this);
        DeathScreen.Trigger(DeathScreen.DeathCause.Caught);
    }

    public void ReleaseParkour()
    {
        if (phase == Phase.Released)
            return;
        phase = Phase.Released;
        ReleaseOwnership();
    }

    public void ResetTemporaryState()
    {
        ReleaseOwnership();
        if (dogRunner != null)
            dogRunner.ResetRunner();
        foreach (Level04BParkourObstacle obstacle in GetComponentsInChildren<Level04BParkourObstacle>(true))
            obstacle.ResetTemporaryState();
        phase = Phase.Inactive;
        StartCoroutine(RestartAfterRecoveryPlacement());
    }

    IEnumerator BeginWhenReady()
    {
        float deadline = Time.realtimeSinceStartup + 10f;
        while (Time.realtimeSinceStartup < deadline)
        {
            ResolveDependencies();
            if (playerControl != null && human != null && dog != null)
            {
                BeginParkour();
                yield break;
            }
            yield return null;
        }
        Debug.LogError("[Level04BParkour] Timed out waiting for the formal actor pair.", this);
    }

    IEnumerator RestartAfterRecoveryPlacement()
    {
        yield return null;
        if (isActiveAndEnabled)
            yield return BeginWhenReady();
    }

    void ResolveDependencies()
    {
        if (playerControl == null)
            playerControl = FindObjectOfType<PlayerControl>();
        PlayerActors actors = PlayerActors.Instance;
        human = playerControl != null && playerControl.Human != null ? playerControl.Human : actors != null ? actors.Human : null;
        dog = playerControl != null && playerControl.Dog != null ? playerControl.Dog : actors != null ? actors.Dog : null;
        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<CameraFollow>();
        if (dogRunner == null)
            dogRunner = GetComponent<Level04BParkourDogRunner>();
        dogOrbit = dog != null ? dog.GetComponent<DogOrbitFollower>() : null;
    }

    void AcquireOwnership()
    {
        if (ownershipHeld)
            return;
        ownershipHeld = true;
        playerControl.AcquireParkourControl();
        dogOrbitWasFollowing = dogOrbit != null && dogOrbit.IsFollowing;
        if (dogOrbit != null)
            dogOrbit.StopOrbit();
        if (cameraFollow != null && cameraPose != null)
            cameraFollow.AcquireScriptedControl(cameraPose, cameraBlendSpeed);
    }

    void ReleaseOwnership()
    {
        if (!ownershipHeld)
            return;
        ownershipHeld = false;
        if (playerControl != null)
            playerControl.ReleaseParkourControl();
        if (cameraFollow != null)
            cameraFollow.ReleaseScriptedControl();
        if (dogOrbitWasFollowing && dogOrbit != null && human != null && dog != null)
            dogOrbit.BeginOrbit(human, dog);
    }

    void BindObstacles()
    {
        foreach (Level04BParkourObstacle obstacle in GetComponentsInChildren<Level04BParkourObstacle>(true))
            obstacle.Bind(this);
    }

    void UpdatePressure(float deltaTime)
    {
        if (Time.time - lastPressureTime >= recoveryDelay)
            proximity = Mathf.MoveTowards(proximity, baselineProximity, recoveryPerSecond * deltaTime);

        if (monsterPresentation != null && human != null && phase != Phase.Walkout)
        {
            float distance = Mathf.Lerp(monsterFarDistance, monsterNearDistance, proximity);
            monsterPresentation.position = human.transform.position - human.transform.forward * distance;
            monsterPresentation.rotation = Quaternion.LookRotation(human.transform.position - monsterPresentation.position, Vector3.up);
        }

        if (proximity < barkThreshold || dog == null || Time.time < nextBarkTime)
            return;
        nextBarkTime = Time.time + barkCooldown;
        if (BarkRequested != null)
            BarkRequested();
        if (dogWarningBark != null && dogWarningBark.IsValid())
            dogWarningBark.Post(dog.gameObject);
        else if (!warnedMissingBark)
        {
            warnedMissingBark = true;
            Debug.LogWarning("[Level04BParkour] Dog warning bark Wwise Event is not assigned.", this);
        }
    }

    void UpdateCameraPose(bool snap = false)
    {
        if (cameraPose == null || human == null)
            return;
        bool walking = phase == Phase.Walkout || phase == Phase.Tutorial;
        Vector3 localOffset = walking ? walkCameraOffset : chaseCameraOffset;
        Quaternion facing = human.transform.rotation;
        Vector3 wanted = human.transform.position + facing * localOffset;
        cameraPose.position = snap ? wanted : Vector3.Lerp(cameraPose.position, wanted, 0.35f);
        Vector3 focus = human.transform.position + Vector3.up * 1.3f;
        Vector3 look = focus - cameraPose.position;
        if (look.sqrMagnitude > 0.001f)
            cameraPose.rotation = Quaternion.LookRotation(look, Vector3.up);
        if (cameraFollow != null)
            cameraFollow.SetScriptedPose(cameraPose);
    }

    public bool ValidateRoute(out string error)
    {
        if (segments == null || segments.Length == 0)
        {
            error = "No route segments are assigned.";
            return false;
        }
        for (int i = 0; i < segments.Length; i++)
        {
            if (!SegmentIsValid(segments[i]))
            {
                error = "Segment " + i + " is missing endpoints or has zero length.";
                return false;
            }
            if (i > 0 && Vector3.Distance(segments[i - 1].end.position, segments[i].start.position) > 0.25f)
            {
                error = "Segments " + (i - 1) + " and " + i + " are not contiguous.";
                return false;
            }
        }
        error = null;
        return true;
    }

    static bool SegmentIsValid(RouteSegment segment)
    {
        return segment != null && segment.start != null && segment.end != null && segment.Length > 0.05f;
    }

    static float Smooth01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }

    static float FlatDistance(Vector3 first, Vector3 second)
    {
        first.y = second.y = 0f;
        return Vector3.Distance(first, second);
    }

    void ResetPathWatchdog()
    {
        pathStuckSeconds = 0f;
        previousPathDistance = float.PositiveInfinity;
    }

    void OnGUI()
    {
        if (phase == Phase.Chase)
        {
            GUIStyle hintStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.026f),
                wordWrap = true
            };
            string hint = CurrentInputHint();
            Rect hintRect = new Rect(Screen.width * 0.3f, Screen.height * 0.82f,
                Screen.width * 0.4f, Screen.height * 0.075f);
            GUI.Box(hintRect, hint, hintStyle);
            return;
        }

        if (phase != Phase.Tutorial)
            return;
        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(Screen.height * 0.035f),
            wordWrap = true
        };
        Rect rect = new Rect(Screen.width * 0.2f, Screen.height * 0.72f, Screen.width * 0.6f, Screen.height * 0.1f);
        GUI.Box(rect, completionMessage, style);
    }

    string CurrentInputHint()
    {
        RouteSegment segment = segmentIndex >= 0 && segmentIndex < segments.Length ? segments[segmentIndex] : null;
        if (segment == null || segment.requiredTurn == TurnDirection.None ||
            segment.Length - segmentDistance > segment.decisionWindow)
            return "A / D 换道    W / Space 跳跃";

        if (segment.requiredTurn == TurnDirection.Left)
            return "转角：按 A 左转";
        if (segment.requiredTurn == TurnDirection.Right)
            return "转角：按 D 右转";
        return "最后路口：A 左转或 D 右转";
    }

    void OnDrawGizmosSelected()
    {
        if (segments == null)
            return;
        foreach (RouteSegment segment in segments)
        {
            if (!SegmentIsValid(segment))
                continue;
            Gizmos.color = segment.finalJunction ? Color.yellow : Color.white;
            Gizmos.DrawLine(segment.start.position, segment.end.position);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(segment.start.position - segment.Right * segment.laneWidth, segment.end.position - segment.Right * segment.laneWidth);
            Gizmos.DrawLine(segment.start.position + segment.Right * segment.laneWidth, segment.end.position + segment.Right * segment.laneWidth);
        }
    }

    void OnDisable()
    {
        ReleaseOwnership();
    }
}
