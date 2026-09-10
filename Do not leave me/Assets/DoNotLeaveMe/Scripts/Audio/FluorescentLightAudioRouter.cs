using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Only a few nearby physical lamps contribute to the room tone.</summary>
public sealed class FluorescentLightAudioRouter : MonoBehaviour
{
    private readonly List<FluorescentLightAudioEmitter> emitters = new List<FluorescentLightAudioEmitter>();
    private readonly List<FluorescentLightAudioEmitter> nearby = new List<FluorescentLightAudioEmitter>();
    private WwiseUIFeedbackSettings settings;
    private float nextRefresh;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRouter()
    {
        if (FindObjectOfType<FluorescentLightAudioRouter>() != null) return;
        var go = new GameObject("[FluorescentLightAudioRouter]");
        DontDestroyOnLoad(go);
        go.AddComponent<FluorescentLightAudioRouter>();
    }
    private void Awake()
    {
        settings = Resources.Load<WwiseUIFeedbackSettings>(WwiseUIFeedbackSettings.ResourcesPath);
        SceneManager.sceneLoaded += Loaded;
        for (int i = 0; i < SceneManager.sceneCount; i++) InstallOnScene(SceneManager.GetSceneAt(i));
    }
    private void Loaded(Scene scene, LoadSceneMode mode) => InstallOnScene(scene);
    private void InstallOnScene(Scene scene)
    {
        if (!scene.isLoaded || !(scene.name.StartsWith("Level_", StringComparison.Ordinal) || scene.name.StartsWith("SharedArt_", StringComparison.Ordinal))) return;
        if (settings == null || !settings.HasValidFluorescentLightEvent) return;
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (!IsCeilingLampName(t.name)) continue;
            bool nested = false;
            for (Transform p = t.parent; p != null; p = p.parent)
                if (IsCeilingLampName(p.name)) { nested = true; break; }
            if (nested) continue;
            var emitter = t.GetComponent<FluorescentLightAudioEmitter>();
            if (emitter == null) emitter = t.gameObject.AddComponent<FluorescentLightAudioEmitter>();
            emitter.Initialize(settings.FluorescentLightEvent);
            if (!emitters.Contains(emitter)) emitters.Add(emitter);
        }
        nextRefresh = 0;
    }
    private void Update()
    {
        if (!GameplayState.CanSimulate)
        {
            foreach (var emitter in emitters) if (emitter != null) emitter.SetAudible(false);
            return;
        }
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.5f;
        emitters.RemoveAll(e => e == null);
        nearby.Clear();
        var camera = Camera.main;
        if (camera != null)
        {
            Vector3 position = camera.transform.position;
            float radius = FormalSfxMixProfile.Instance.lampRadius;
            foreach (var emitter in emitters)
                if (emitter.isActiveAndEnabled && (emitter.transform.position - position).sqrMagnitude < radius * radius)
                    nearby.Add(emitter);
            nearby.Sort((a, b) => ((a.transform.position - position).sqrMagnitude - (a.IsAudible ? 4f : 0f)).CompareTo(
                (b.transform.position - position).sqrMagnitude - (b.IsAudible ? 4f : 0f)));
        }
        int count = Mathf.Min(nearby.Count, FormalSfxMixProfile.Instance.maximumNearbyLamps);
        foreach (var emitter in emitters)
        {
            bool selected = false;
            for (int i = 0; i < count; i++) if (nearby[i] == emitter) { selected = true; break; }
            emitter.SetAudible(selected);
        }
    }
    public static bool IsCeilingLampName(string name) => !string.IsNullOrEmpty(name) && name.IndexOf("pendant_lamp", StringComparison.OrdinalIgnoreCase) >= 0;
    private void OnDisable() { foreach (var e in emitters) if (e != null) e.SetAudible(false); }
    private void OnDestroy() { SceneManager.sceneLoaded -= Loaded; OnDisable(); }
}
