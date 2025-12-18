using UnityEngine;

public class ExperienceController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Camera mainCamera;

    [Header("Particle Looks")]
    [SerializeField] private ParticleSystem particlesCalm;
    [SerializeField] private ParticleSystem particlesReset;

    [Header("Presets")]
    [SerializeField] private ExperiencePreset deepCalmPreset;
    [SerializeField] private ExperiencePreset mentalResetPreset;

    [Header("Startup")]
    [SerializeField] private ExperiencePreset startPreset;

    [Header("Live Override (optional)")]
    [SerializeField] private bool overrideEnabled = false;
    [Range(60f, 1800f)] public float overrideDurationSeconds = 600f;
    [Range(0f, 1f)] public float overrideOverallVolume = 0.7f;
    [Range(0f, 1f)] public float overrideLowFreqStrength = 0.6f;
    public Color overrideMainColor = new Color(0.2f, 0.4f, 1f, 1f);
    [Range(0f, 1f)] public float overrideMotionSpeed = 0.2f;
    [Range(0f, 1f)] public float overrideBaseIntensity = 0.6f;

    // runtime
    private ExperiencePreset activePreset;
    private float elapsed;
    private bool running;

    private void Start()
    {
        // default start preset fallback
        if (startPreset != null) LoadPreset(startPreset);
        else if (deepCalmPreset != null) LoadPreset(deepCalmPreset);

        StartExperience();
    }

    private void Update()
    {
        // Desktop preview switching

        if (!running) return;

        float duration = GetDurationSeconds();
        elapsed += Time.deltaTime;

        float t = (duration <= 0.001f) ? 1f : Mathf.Clamp01(elapsed / duration);

        // 3-phase envelope:
        // - Arrival: ramp up
        // - Deep: stay high
        // - Return: ramp down
        // Implemented as smooth "0 -> 1 -> 0"
        float envelope = Mathf.Sin(t * Mathf.PI); // 0..1..0
        float shaped = Mathf.SmoothStep(0f, 1f, envelope);

        float baseIntensity = GetBaseIntensity();
        float intensity = baseIntensity * shaped;

        ApplyAudio(intensity);
        ApplyVisuals(intensity);

        if (elapsed >= duration)
        {
            running = false;
            // End gracefully
            if (audioSource != null) audioSource.volume = 0f;
            StopParticles(particlesCalm);
            StopParticles(particlesReset);
        }
    }

    // ---------- Public API (für Buttons später) ----------
    public void StartDeepCalm()
    {
        if (deepCalmPreset == null) return;
        LoadPreset(deepCalmPreset);
        StartExperience();
    }

    public void StartMentalReset()
    {
        if (mentalResetPreset == null) return;
        LoadPreset(mentalResetPreset);
        StartExperience();
    }

    public void ToggleOverride(bool enabled)
    {
        overrideEnabled = enabled;
        // optional: re-apply immediately
        ApplyLookForActivePreset();
    }

    // ---------- Core ----------
    private void LoadPreset(ExperiencePreset preset)
    {
        activePreset = preset;

        // When switching preset, also prime override defaults (handy for tweaking)
        if (activePreset != null)
        {
            overrideDurationSeconds = activePreset.durationSeconds;
            overrideOverallVolume = activePreset.overallVolume;
            overrideLowFreqStrength = activePreset.lowFreqStrength;
            overrideMainColor = activePreset.mainColor;
            overrideMotionSpeed = activePreset.motionSpeed;
            overrideBaseIntensity = activePreset.baseIntensity;
        }

        ApplyLookForActivePreset();
    }

    private void StartExperience()
    {
        elapsed = 0f;
        running = true;

        // ensure correct particles are on for the selected experience
        ApplyLookForActivePreset();

        if (audioSource != null)
        {
            // Start audio if not running
            if (!audioSource.isPlaying)
                audioSource.Play();

            // Start at low volume; will be driven by envelope in Update
            audioSource.volume = 0.01f;
        }
    }

    private void ApplyLookForActivePreset()
    {
        // Decide which particle system should be active based on active preset identity
        bool isMentalReset = (activePreset != null && activePreset == mentalResetPreset);

        if (particlesCalm != null)
        {
            particlesCalm.gameObject.SetActive(!isMentalReset);
            if (!isMentalReset) PlayParticles(particlesCalm);
            else StopParticles(particlesCalm);
        }

        if (particlesReset != null)
        {
            particlesReset.gameObject.SetActive(isMentalReset);
            if (isMentalReset) PlayParticles(particlesReset);
            else StopParticles(particlesReset);
        }

        // Apply initial camera background immediately
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = Color.Lerp(Color.black, GetMainColor(), 0.35f);
        }

    }

    // ---------- Mapping helpers ----------
    private float GetDurationSeconds()
    {
        if (overrideEnabled) return overrideDurationSeconds;
        return activePreset != null ? activePreset.durationSeconds : 600f;
    }

    private float GetBaseIntensity()
    {
        if (overrideEnabled) return overrideBaseIntensity;
        return activePreset != null ? activePreset.baseIntensity : 0.6f;
    }

    private float GetOverallVolume()
    {
        if (overrideEnabled) return overrideOverallVolume;
        return activePreset != null ? activePreset.overallVolume : 0.7f;
    }

    private float GetLowFreqStrength()
    {
        if (overrideEnabled) return overrideLowFreqStrength;
        return activePreset != null ? activePreset.lowFreqStrength : 0.6f;
    }

    private float GetMotionSpeed()
    {
        if (overrideEnabled) return overrideMotionSpeed;
        return activePreset != null ? activePreset.motionSpeed : 0.2f;
    }

    private Color GetMainColor()
    {
        if (overrideEnabled) return overrideMainColor;
        return activePreset != null ? activePreset.mainColor : Color.blue;
    }

    // ---------- Apply audio/visuals ----------
    private void ApplyAudio(float intensity)
    {
        if (audioSource == null) return;

        // Base volume follows intensity envelope
        float targetVolume = Mathf.Lerp(0.12f, GetOverallVolume(), intensity);
        audioSource.volume = targetVolume;

        // Placeholder for later: lowFreqStrength can drive EQ / extra layer
        // float low = GetLowFreqStrength();
        // TODO: AudioMixer.SetFloat("LowFreqGain", Mathf.Lerp(-80f, 0f, low));
    }

    private void ApplyVisuals(float intensity)
    {
        // Camera background (Solid Color)
    if (mainCamera != null)
    {
        mainCamera.backgroundColor =
            Color.Lerp(Color.black, GetMainColor(), 0.25f + 0.75f * intensity);
    }


        // Drive particle behaviour slightly via code (optional)
        float motion = GetMotionSpeed();

        // Calm particles tuning
        if (particlesCalm != null && particlesCalm.gameObject.activeSelf)
        {
            var main = particlesCalm.main;
            main.startSpeed = Mathf.Lerp(0.03f, 0.25f, motion);
            main.startSize = Mathf.Lerp(0.06f, 0.22f, Mathf.Clamp01(intensity + 0.15f));

            var emission = particlesCalm.emission;
            emission.rateOverTime = Mathf.Lerp(10f, 120f, intensity);
        }

        // Reset particles tuning
        if (particlesReset != null && particlesReset.gameObject.activeSelf)
        {
            var main = particlesReset.main;
            main.startSpeed = Mathf.Lerp(0.12f, 0.85f, motion);
            main.startSize = Mathf.Lerp(0.03f, 0.10f, 1f - intensity * 0.5f);

            var emission = particlesReset.emission;
            emission.rateOverTime = Mathf.Lerp(60f, 300f, intensity);
        }
    }

    // ---------- Particle helpers ----------
    private static void PlayParticles(ParticleSystem ps)
    {
        if (ps == null) return;
        if (!ps.isPlaying) ps.Play(true);
    }

    private static void StopParticles(ParticleSystem ps)
    {
        if (ps == null) return;
        if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
