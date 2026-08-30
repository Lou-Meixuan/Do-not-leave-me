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

        // 人和狗互相不碰撞，避免切换角色时把对方挤开。
        if (human != null && dog != null)
        {
            Collider humanCollider = human.GetComponentInChildren<Collider>();
            Collider dogCollider = dog.GetComponentInChildren<Collider>();
            if (humanCollider != null && dogCollider != null)
                Physics.IgnoreCollision(humanCollider, dogCollider, true);
        }
    }
}
