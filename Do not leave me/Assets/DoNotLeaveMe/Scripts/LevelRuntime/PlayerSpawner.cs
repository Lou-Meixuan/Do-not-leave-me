using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private PlayerActors playerActorsPrefab;

    void Awake()
    {
        if (PlayerActors.Instance == null && playerActorsPrefab != null)
            Instantiate(playerActorsPrefab);
    }
}
