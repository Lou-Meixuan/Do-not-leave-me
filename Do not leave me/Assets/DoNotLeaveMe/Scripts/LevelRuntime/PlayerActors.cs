using UnityEngine;

public class PlayerActors : MonoBehaviour
{
    [SerializeField] private PlayerActor human;
    [SerializeField] private PlayerActor dog;

    public static PlayerActors Instance { get; private set; }
    public PlayerActor Human => human;
    public PlayerActor Dog => dog;

    void Awake()
    {
        Instance = this;

        // 两名角色都使用实体胶囊；切换控制权不应关闭它们之间的物理解算。
        // 显式恢复碰撞，避免此前的 IgnoreCollision 状态在禁用/启用对象后继续生效。
        if (human != null && dog != null)
        {
            Collider humanCollider = human.GetComponentInChildren<Collider>();
            Collider dogCollider = dog.GetComponentInChildren<Collider>();
            if (humanCollider != null && dogCollider != null)
                Physics.IgnoreCollision(humanCollider, dogCollider, false);
        }
    }
}
