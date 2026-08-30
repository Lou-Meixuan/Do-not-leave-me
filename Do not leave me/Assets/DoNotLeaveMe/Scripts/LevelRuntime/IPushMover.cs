using UnityEngine;

public interface IPushMover
{
    bool IsEngaged { get; }
    bool IsAttached(PlayerActor actor);
    bool TryEngage(PlayerActor actor);
    void Cancel();
    void Move(Vector3 worldDirection, bool pushingAnimation);
    void SetAttachedPushAnimation();
    void SetAttachedPullAnimation();
    void SetAttachedIdleAnimation();
}