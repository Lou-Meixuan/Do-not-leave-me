using UnityEngine;

/// <summary>
/// Central read-only gate for runtime systems that must only operate while
/// formal gameplay simulation is active. UI audio remains independent.
/// </summary>
public static class GameplayState
{
    public static bool CanSimulate
    {
        get
        {
            if (Time.timeScale <= 0f)
                return false;

            return !PauseMenu.IsPaused
                && !TutorialPopup.IsShowing
                && !DeathScreen.IsShowing;
        }
    }
}

