using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CrateDoorTrigger : MonoBehaviour
{
    [SerializeField] private Door door;
    private bool preloadRouteSuccessor;
    private bool completed;

    public Door Door => door;

    /// <summary>由场景自有的实体出口绑定在运行时设置，不改动 Prefab 资源。</summary>
    public void SetPreloadRouteSuccessor(bool enabled)
    {
        preloadRouteSuccessor = enabled;
    }

    /// <summary>供关卡恢复流程重开同一出口的箱子机关。</summary>
    public void ResetForLevelRecovery()
    {
        completed = false;
    }

    void Awake()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        TryOpenFromCollider(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (!completed)
            TryOpenFromCollider(other);
    }

    void TryOpenFromCollider(Collider other)
    {
        if (completed)
            return;

        PushableCrate crate = other.GetComponentInParent<PushableCrate>();
        if (crate == null)
            return;

        completed = true;
        if (door != null)
            door.OpenPermanently();
        else
            Debug.LogError("[CrateDoorTrigger] Door reference is missing on " + name);

        GameFlowController flow = FindObjectOfType<GameFlowController>();
        if (flow != null)
        {
            if (preloadRouteSuccessor)
            {
                Debug.Log(
                    $"[PhysicalDoorTransition] crate-exit trigger='{name}' scene='{gameObject.scene.name}' mode=preload.",
                    this);
                flow.PreloadRouteSuccessor(this, openTransitionDoor: true);
            }
            else
            {
                flow.RequestRouteAdvance(this);
            }
        }
        else
            Debug.LogError("[CrateDoorTrigger] GameFlowController not found.");
    }
}
