using UnityEngine;

public enum TriggerRequirement
{
    EitherPlayer,
    HumanOnly,
    DogOnly,
    BothPlayers,
    ResettablePhysicsOccupant
}

public static class TriggerEligibility
{
    public static bool Accepts(Collider other, TriggerRequirement requirement)
    {
        if (other == null)
            return false;

        if (requirement == TriggerRequirement.HumanOnly)
            return LevelActors.IsHuman(other);

        if (requirement == TriggerRequirement.DogOnly)
            return LevelActors.IsDog(other);

        if (requirement == TriggerRequirement.EitherPlayer || requirement == TriggerRequirement.BothPlayers)
            return LevelActors.IsPlayer(other);

        return other.GetComponentInParent<ResettablePhysicsOccupant>() != null;
    }

    public static Object ResolveOccupant(Collider other, TriggerRequirement requirement)
    {
        if (!Accepts(other, requirement))
            return null;

        PlayerActor player = LevelActors.ResolvePlayer(other);
        if (player != null)
            return player;

        return other.GetComponentInParent<ResettablePhysicsOccupant>();
    }
}
