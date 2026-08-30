using UnityEngine;

public class PrerequisiteActuator : MonoBehaviour, ILevelTemporaryState
{
    [SerializeField] private MechanismState[] prerequisites;
    [SerializeField] private MonoBehaviour[] actuators;
    [SerializeField] private bool permanentResult = true;

    private bool applied;

    void Awake()
    {
        LevelController level = LevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    void Update()
    {
        if (!applied && ArePrerequisitesComplete())
            ApplyOpenState();
    }

    public bool ArePrerequisitesComplete()
    {
        if (prerequisites == null || prerequisites.Length == 0)
            return false;

        foreach (MechanismState prerequisite in prerequisites)
        {
            if (prerequisite == null || !prerequisite.IsComplete)
                return false;
        }

        return true;
    }

    public void ResetTemporaryState()
    {
        if (permanentResult || !applied)
            return;

        applied = false;
        foreach (MonoBehaviour behaviour in actuators)
        {
            ILevelActuator actuator = behaviour as ILevelActuator;
            if (actuator != null)
                actuator.Close();
        }
    }

    void ApplyOpenState()
    {
        applied = true;
        foreach (MonoBehaviour behaviour in actuators)
        {
            ILevelActuator actuator = behaviour as ILevelActuator;
            if (actuator != null)
                actuator.Open();
        }
    }
}
