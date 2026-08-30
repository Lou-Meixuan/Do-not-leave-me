using UnityEngine;

public class PlayerVisualLoader : MonoBehaviour
{
    [SerializeField] private PlayerActor human;
    [SerializeField] private PlayerActor dog;
    [SerializeField] private GameObject humanVisualPrefab;
    [SerializeField] private GameObject dogVisualPrefab;
    [SerializeField] private Vector3 humanVisualOffset;
    [SerializeField] private Vector3 dogVisualOffset;

    // 注意：这两个字符串是【场景里子物体的名字】，不是预制体资产名。
    // PlayerActors.prefab 里嵌的 Visual 实例就叫 FormalHumanVisual / FormalDogVisual，
    // 改这里必须同时改 PlayerActors.prefab 里的 m_Name 覆盖，否则会重复生成一份。
    void Awake()
    {
        LoadVisual(human, humanVisualPrefab, "FormalHumanVisual", humanVisualOffset);
        LoadVisual(dog, dogVisualPrefab, "FormalDogVisual", dogVisualOffset);
    }

    static void LoadVisual(PlayerActor player, GameObject prefab, string instanceName, Vector3 offset)
    {
        if (player == null || prefab == null)
            return;

        // 视觉挂到 Body 下，与胶囊共享同一缩放旋钮；无 Body 时回退到根节点。
        Transform parent = player.transform.Find("Body");
        if (parent == null)
            parent = player.transform;
        if (parent.Find(instanceName) != null)
            return;

        GameObject visual = Instantiate(prefab, parent);
        visual.name = instanceName;
        visual.transform.localPosition += offset;
    }
}
