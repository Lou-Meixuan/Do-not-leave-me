using UnityEngine;

public interface ILevelTemporaryState
{
    void ResetTemporaryState();
}

public interface ILevelPermanentState
{
    bool IsComplete { get; }
}

public interface ILevelActuator
{
    void Open();
    void Close();
}

public static class LevelActors
{
    public static LevelController FindLevelController(UnityEngine.SceneManagement.Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            LevelController controller = root.GetComponentInChildren<LevelController>(true);
            if (controller != null)
                return controller;
        }

        return null;
    }

    public static bool IsHuman(Collider other)
    {
        PlayerActor player = ResolvePlayer(other);
        return player != null && player.Role == PlayerActor.ActorRole.Human;
    }

    public static bool IsDog(Collider other)
    {
        PlayerActor player = ResolvePlayer(other);
        return player != null && player.Role == PlayerActor.ActorRole.Dog;
    }

    public static bool IsPlayer(Collider other)
    {
        return ResolvePlayer(other) != null;
    }

    public static PlayerActor ResolvePlayer(Collider other)
    {
        return other != null ? other.GetComponentInParent<PlayerActor>() : null;
    }
}
