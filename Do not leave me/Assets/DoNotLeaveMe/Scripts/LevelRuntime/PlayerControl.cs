using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private PlayerActor human;
    [SerializeField] private PlayerActor dog;
    private PlayerActor activeActor;
    private CameraFollow cameraFollow;
    private bool humanOnly;

    public bool IsDogActive => activeActor != null && activeActor.Role == PlayerActor.ActorRole.Dog;

    public event System.Action<bool> ActiveRoleChanged;

    void Start()
    {
        activeActor = human;
        cameraFollow = FindObjectOfType<CameraFollow>();
        if (cameraFollow == null)
        {
            GameObject cameraObject = new GameObject("FormalMainCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraFollow = cameraObject.AddComponent<CameraFollow>();
            cameraFollow.enableMouseRotation = true;
        }

        SetCameraTarget();
    }

    void Update()
    {
        if (ReadingArchiveController.IsShowing)
            return;

        if (human == null)
            return;

        if (!humanOnly && dog != null && Input.GetKeyDown(KeyCode.Tab) && !IsMoverEngaged())
        {
            SwitchActor(activeActor == human ? dog : human);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            // 空手站在公告牌前，F 是阅读；已经挂着箱子时 F 仍然是松手。
            if (IsMoverEngaged() || !NoticeBoard.TryRead(activeActor))
                ToggleMoverEngagement();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !IsMoverEngaged())
        {
            if (!activeActor.IsExecutionLocked)
                activeActor.Jump();
        }
    }

    /// <summary>
    /// 切换操控对象。必须把上一个角色的速度清掉——刚体用的是无摩擦材质
    /// (PlayerNoFriction)，不主动停它就会一直按切换前的速度滑走，
    /// 而且动画还停在 Walk 上。
    /// </summary>
    void SwitchActor(PlayerActor next)
    {
        if (next == null || next == activeActor)
            return;

        if (activeActor != null)
            activeActor.Stop();

        activeActor = next;
        SetCameraTarget();

        if (ActiveRoleChanged != null)
            ActiveRoleChanged(IsDogActive);
    }

    void FixedUpdate()
    {
        if (ReadingArchiveController.IsShowing)
        {
            if (activeActor != null)
                activeActor.Stop();
            return;
        }

        if (activeActor == null)
            return;

        if (activeActor.IsExecutionLocked)
        {
            activeActor.Stop();
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 direction = CameraRelativeDirection(input);
        CooperativeRailMover railMover = FindEngagedRailMover();
        IPushMover engagedMover = railMover != null ? (IPushMover)railMover : FindEngagedPushable();
        if (engagedMover != null)
        {
            if (railMover != null)
            {
                // 轨道机关只沿轨道走，动画仍按前后输入判断。
                if (vertical > 0.01f)
                    railMover.SetAttachedPushAnimation();
                else if (vertical < -0.01f)
                    railMover.SetAttachedPullAnimation();
                else
                    railMover.SetAttachedIdleAnimation();

                Vector3 moverDirection = activeActor.transform.forward * vertical;
                railMover.Move(moverDirection, vertical > 0.01f);
            }
            // 物理推箱的移动和推箱动画都由 PushableCrate 自己的 FixedUpdate 驱动：
            // 它才知道当前这次 WASD 输入到底有没有真的推动箱子（Free 模式下 A/D 也算数）。
            return;
        }

        bool sprint = activeActor.Role == PlayerActor.ActorRole.Human && Input.GetKey(KeyCode.LeftShift);
        activeActor.Move(direction, sprint);
    }

    void ToggleMoverEngagement()
    {
        if (activeActor == null || activeActor.IsExecutionLocked)
            return;

        foreach (CooperativeRailMover mover in FindObjectsOfType<CooperativeRailMover>())
        {
            if (mover.IsAttached(activeActor))
            {
                mover.Cancel();
                return;
            }

            if (mover.TryEngage(activeActor))
                return;

            // A stale single-actor engagement should never block a fresh F interaction.
            if (!mover.IsEngaged)
                mover.Cancel();
        }

        foreach (PushableCrate crate in FindObjectsOfType<PushableCrate>())
        {
            if (crate.IsAttached(activeActor))
            {
                crate.Cancel();
                return;
            }

            if (crate.TryEngage(activeActor))
                return;
        }
    }

    Vector3 CameraRelativeDirection(Vector3 input)
    {
        if (cameraFollow == null || input.sqrMagnitude < 0.01f)
            return input;

        Vector3 forward = cameraFollow.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = cameraFollow.transform.right;
        right.y = 0f;
        right.Normalize();
        return (forward * input.z + right * input.x).normalized;
    }

    void SetCameraTarget()
    {
        if (cameraFollow == null || human == null || dog == null)
            return;

        cameraFollow.SetTarget(activeActor.FocusAnchor);
    }

    bool IsMoverEngaged()
    {
        return FindEngagedRailMover() != null || FindEngagedPushable() != null;
    }

    public void ForceHumanOnly(bool forced)
    {
        humanOnly = forced;
        if (!forced)
        {
            DogOrbitFollower follower = dog != null ? dog.GetComponent<DogOrbitFollower>() : null;
            if (follower != null)
                follower.StopOrbit();
            return;
        }

        if (human == null)
            return;

        if (activeActor != null && activeActor != human)
            activeActor.Stop();

        activeActor = human;
        human.gameObject.SetActive(true);
        if (dog != null)
        {
            dog.gameObject.SetActive(true);
            dog.Stop();
        }
        SetCameraTarget();
        if (ActiveRoleChanged != null)
            ActiveRoleChanged(false);
    }

    static CooperativeRailMover FindEngagedRailMover()
    {
        foreach (CooperativeRailMover mover in FindObjectsOfType<CooperativeRailMover>())
            if (mover.IsEngaged)
                return mover;
        return null;
    }

    static PushableCrate FindEngagedPushable()
    {
        foreach (PushableCrate crate in FindObjectsOfType<PushableCrate>())
            if (crate.IsEngaged)
                return crate;
        return null;
    }
}
