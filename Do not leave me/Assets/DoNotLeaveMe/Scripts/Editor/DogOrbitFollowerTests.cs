#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
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
}
#endif
