#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DogOrbitFollowerTests
{
    [Test]
    public void ForcedFollow_StopsAtNearDistance()
    {
        Assert.That(DogOrbitFollower.EvaluateWaitingState(false, 2.25f, 2.25f, 3.5f), Is.True);
    }

    [Test]
    public void ForcedFollow_RemainsWaitingInsideHysteresisBand()
    {
        Assert.That(DogOrbitFollower.EvaluateWaitingState(true, 3f, 2.25f, 3.5f), Is.True);
    }

    [Test]
    public void ForcedFollow_ResumesBeyondResumeDistance()
    {
        Assert.That(DogOrbitFollower.EvaluateWaitingState(true, 3.51f, 2.25f, 3.5f), Is.False);
    }

    [Test]
    public void PlayerControlCancellation_StopsFollowWithoutMovingActors()
    {
        GameObject humanObject = new GameObject("TestHuman");
        GameObject dogObject = new GameObject("TestDog");
        try
        {
            PlayerActor human = humanObject.AddComponent<PlayerActor>();
            PlayerActor dog = dogObject.AddComponent<PlayerActor>();
            DogOrbitFollower follower = dogObject.AddComponent<DogOrbitFollower>();
            PlayerControl control = humanObject.AddComponent<PlayerControl>();
            typeof(PlayerControl).GetField("dog", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(control, dog);
            humanObject.transform.position = new Vector3(10f, 2f, 20f);
            dogObject.transform.position = new Vector3(14f, 2f, 20f);
            Vector3 humanPosition = humanObject.transform.position;
            Vector3 dogPosition = dogObject.transform.position;

            follower.BeginOrbit(human, dog);
            Assert.That(follower.IsFollowing, Is.True);

            control.CancelForcedDogFollow();
            control.CancelForcedDogFollow();

            Assert.That(follower.IsFollowing, Is.False);
            Assert.That(humanObject.transform.position, Is.EqualTo(humanPosition));
            Assert.That(dogObject.transform.position, Is.EqualTo(dogPosition));
            Assert.That(dog.RuntimeMovementSpeedMultiplier, Is.EqualTo(1f));
        }
        finally
        {
            Object.DestroyImmediate(dogObject);
            Object.DestroyImmediate(humanObject);
        }
    }

    [Test]
    public void Level05Checkpoint_DoesNotCommitPendingPhysicalTransition()
    {
        GameObject flowObject = new GameObject("TestGameFlow");
        try
        {
            GameFlowController flow = flowObject.AddComponent<GameFlowController>();
            SetPrivateField(flow, "currentLevelScene", "Level_04B");
            SetPrivateField(flow, "pendingPhysicalTransitionFromScene", "Level_04B");
            SetPrivateField(flow, "pendingPhysicalTransitionToScene", "Level_05");
            SetPrivateField(flow, "retainedPhysicalPredecessorScene", "Level_04");

            flow.NotifyCheckpointActivated("Level_05");

            Assert.That(flow.CurrentLevelScene, Is.EqualTo("Level_04B"));
            Assert.That(flow.HasPendingPhysicalTransition, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(flowObject);
        }
    }

    [Test]
    public void CheckpointCarpet_RequiresBothRolesAndLosesReadinessOnExit()
    {
        GameObject humanObject = new GameObject("TestHuman");
        GameObject dogObject = new GameObject("TestDog");
        try
        {
            PlayerActor human = humanObject.AddComponent<PlayerActor>();
            PlayerActor dog = dogObject.AddComponent<PlayerActor>();
            SetPrivateField(dog, "role", PlayerActor.ActorRole.Dog);
            List<PlayerActor> occupants = new List<PlayerActor> { human };

            Assert.That(LevelCheckpoint.HasBothActorRoles(occupants), Is.False);

            occupants.Add(dog);
            Assert.That(LevelCheckpoint.HasBothActorRoles(occupants), Is.True);

            occupants.Remove(human);
            Assert.That(LevelCheckpoint.HasBothActorRoles(occupants), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(dogObject);
            Object.DestroyImmediate(humanObject);
        }
    }

    [Test]
    public void Level05Handoff_ReleasesParkourAndDoesNotRestartFollow()
    {
        GameObject flowObject = new GameObject("TestGameFlow");
        GameObject humanObject = new GameObject("TestHuman");
        GameObject dogObject = new GameObject("TestDog");
        GameObject parkourObject = new GameObject("TestParkour");
        try
        {
            GameFlowController flow = flowObject.AddComponent<GameFlowController>();
            PlayerActor human = humanObject.AddComponent<PlayerActor>();
            PlayerActor dog = dogObject.AddComponent<PlayerActor>();
            PlayerControl control = humanObject.AddComponent<PlayerControl>();
            DogOrbitFollower follower = dogObject.AddComponent<DogOrbitFollower>();
            Level04BParkourController parkour = parkourObject.AddComponent<Level04BParkourController>();
            SetPrivateField(control, "human", human);
            SetPrivateField(control, "dog", dog);
            SetPrivateField(parkour, "playerControl", control);
            SetPrivateField(parkour, "human", human);
            SetPrivateField(parkour, "dog", dog);
            SetPrivateField(parkour, "dogOrbit", follower);
            SetPrivateField(parkour, "ownershipHeld", true);
            SetPrivateField(parkour, "dogOrbitWasFollowing", true);
            control.AcquireParkourControl();
            follower.BeginOrbit(human, dog);
            follower.StopOrbit();
            Vector3 humanPosition = humanObject.transform.position;
            Vector3 dogPosition = dogObject.transform.position;

            typeof(GameFlowController).GetMethod(
                "ReleaseLevel05TemporaryControl",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(flow, null);

            Assert.That(control.IsParkourControlled, Is.False);
            Assert.That(follower.IsFollowing, Is.False);
            Assert.That(humanObject.transform.position, Is.EqualTo(humanPosition));
            Assert.That(dogObject.transform.position, Is.EqualTo(dogPosition));
        }
        finally
        {
            Object.DestroyImmediate(parkourObject);
            Object.DestroyImmediate(dogObject);
            Object.DestroyImmediate(humanObject);
            Object.DestroyImmediate(flowObject);
        }
    }

    [Test]
    public void ParkourRelease_StartsQueuedDogFollow()
    {
        GameObject humanObject = new GameObject("TestHuman");
        GameObject dogObject = new GameObject("TestDog");
        GameObject controlObject = new GameObject("TestControl");
        GameObject parkourObject = new GameObject("TestParkour");
        try
        {
            PlayerActor human = humanObject.AddComponent<PlayerActor>();
            PlayerActor dog = dogObject.AddComponent<PlayerActor>();
            PlayerControl control = controlObject.AddComponent<PlayerControl>();
            Level04BParkourController parkour = parkourObject.AddComponent<Level04BParkourController>();
            SetPrivateField(control, "human", human);
            SetPrivateField(control, "dog", dog);
            SetPrivateField(parkour, "playerControl", control);
            SetPrivateField(parkour, "human", human);
            SetPrivateField(parkour, "dog", dog);
            SetPrivateField(parkour, "ownershipHeld", true);
            control.AcquireParkourControl();

            parkour.StartDogFollowWhenReleased();
            parkour.ReleaseParkour();

            DogOrbitFollower follower = dog.GetComponent<DogOrbitFollower>();
            Assert.That(follower, Is.Not.Null);
            Assert.That(follower.IsFollowing, Is.True);
            Assert.That(control.IsParkourControlled, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(parkourObject);
            Object.DestroyImmediate(controlObject);
            Object.DestroyImmediate(dogObject);
            Object.DestroyImmediate(humanObject);
        }
    }

    static void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }
}
#endif
