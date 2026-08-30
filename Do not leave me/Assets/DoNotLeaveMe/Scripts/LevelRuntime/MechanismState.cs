using UnityEngine;
using UnityEngine.Events;

public enum MechanismResetPolicy
{
    Permanent,
    Resettable
}

public class MechanismState : MonoBehaviour, ILevelTemporaryState, ILevelPermanentState
{
    [SerializeField] private MechanismResetPolicy resetPolicy = MechanismResetPolicy.Resettable;
    [SerializeField] private UnityEvent onCompleted;
    [SerializeField] private UnityEvent onReset;

    public bool IsComplete { get; private set; }
    public bool IsPermanent => resetPolicy == MechanismResetPolicy.Permanent;

    void Awake()
    {
        LevelController level = LevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    public void Complete()
    {
        if (IsComplete)
            return;

        IsComplete = true;
        onCompleted?.Invoke();
    }

    public void ResetTemporaryState()
    {
        if (IsPermanent || !IsComplete)
            return;

        IsComplete = false;
        onReset?.Invoke();
    }
}
