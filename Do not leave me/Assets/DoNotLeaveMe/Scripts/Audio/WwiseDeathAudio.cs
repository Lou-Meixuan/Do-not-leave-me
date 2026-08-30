using UnityEngine;

/// <summary>
/// Owns the formal route's complete death-audio lifecycle. The death screen
/// supplies the gameplay cause; this component translates it to Wwise authoring
/// names and keeps death playback from leaking into restart or title navigation.
/// </summary>
[AddComponentMenu("DoNotLeaveMe/Audio/Wwise Death Audio")]
[RequireComponent(typeof(AkGameObj))]
public sealed class WwiseDeathAudio : MonoBehaviour
{
    [Header("Wwise Events")]
    [SerializeField] private AK.Wwise.Event deathStingerEvent = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Event playDeathCauseMusicEvent = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Event stopDeathCauseMusicEvent = new AK.Wwise.Event();

    [Header("Wwise State Names")]
    [SerializeField] private string deathCauseGroup = "COD";
    [SerializeField] private string anxietyState = "Anxiety";
    [SerializeField] private string caughtState = "Eliminated";

    [Header("Recovery")]
    [Min(0)]
    [SerializeField] private int stingerStopFadeMilliseconds = 100;

    public static WwiseDeathAudio Instance { get; private set; }

    private WwiseMusicController gameplayMusic;
    private uint stingerPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
    private bool warnedMissingStinger;
    private bool warnedMissingPlayMusic;
    private bool warnedMissingStopMusic;
    private bool warnedMissingGameplayMusic;
    private static bool warnedMissingInstance;

    void Awake()
    {
        Instance = this;
        gameplayMusic = GetComponent<WwiseMusicController>();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void PlayDeath(bool anxietyDeath)
    {
        if (Instance != null)
        {
            Instance.Play(anxietyDeath);
            return;
        }

        if (!warnedMissingInstance)
        {
            Debug.LogWarning("[WwiseDeathAudio] No death-audio component is active.");
            warnedMissingInstance = true;
        }
    }

    public static void StopDeathAudio()
    {
        if (Instance != null)
            Instance.StopPlayback();
    }

    void Play(bool anxietyDeath)
    {
        if (gameplayMusic != null)
            gameplayMusic.StopGameplayMusic();
        else if (!warnedMissingGameplayMusic)
        {
            Debug.LogWarning("[WwiseDeathAudio] WwiseMusicController is missing.", this);
            warnedMissingGameplayMusic = true;
        }

        SetDeathCauseState(anxietyDeath ? anxietyState : caughtState);

        if (deathStingerEvent != null && deathStingerEvent.IsValid())
            stingerPlayingId = deathStingerEvent.Post(gameObject);
        else if (!warnedMissingStinger)
        {
            Debug.LogWarning("[WwiseDeathAudio] Play_PlayerDeath_Stinger is not assigned.", this);
            warnedMissingStinger = true;
        }

        if (playDeathCauseMusicEvent != null && playDeathCauseMusicEvent.IsValid())
            playDeathCauseMusicEvent.Post(gameObject);
        else if (!warnedMissingPlayMusic)
        {
            Debug.LogWarning("[WwiseDeathAudio] Play_DeathCause_Music is not assigned.", this);
            warnedMissingPlayMusic = true;
        }
    }

    void StopPlayback()
    {
        if (stopDeathCauseMusicEvent != null && stopDeathCauseMusicEvent.IsValid())
            stopDeathCauseMusicEvent.Post(gameObject);
        else if (!warnedMissingStopMusic)
        {
            Debug.LogWarning("[WwiseDeathAudio] Stop_DeathCause_Music is not assigned.", this);
            warnedMissingStopMusic = true;
        }

        if (stingerPlayingId != AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
        {
            AkUnitySoundEngine.ExecuteActionOnPlayingID(
                AkActionOnEventType.AkActionOnEventType_Stop,
                stingerPlayingId,
                stingerStopFadeMilliseconds);
            stingerPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        }
    }

    void SetDeathCauseState(string stateName)
    {
        if (string.IsNullOrEmpty(deathCauseGroup) || string.IsNullOrEmpty(stateName))
        {
            Debug.LogError("[WwiseDeathAudio] Wwise State Group and State names cannot be empty.", this);
            return;
        }

        AKRESULT result = AkUnitySoundEngine.SetState(deathCauseGroup, stateName);
        if (result != AKRESULT.AK_Success)
        {
            Debug.LogWarning(
                "[WwiseDeathAudio] Failed to set Wwise State " +
                deathCauseGroup + "/" + stateName + ": " + result,
                this);
        }
    }

    void OnValidate()
    {
        stingerStopFadeMilliseconds = Mathf.Max(0, stingerStopFadeMilliseconds);
    }
}
