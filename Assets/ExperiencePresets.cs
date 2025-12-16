using UnityEngine;

[CreateAssetMenu(fileName = "ExperiencePreset", menuName = "MindBody/Experience Preset")]
public class ExperiencePreset : ScriptableObject
{
    [Header("Meta")]
    public string presetName = "Deep Calm";
    [Range(60f, 1800f)] public float durationSeconds = 600f;

    [Header("Audio")]
    [Range(0f, 1f)] public float overallVolume = 0.7f;
    [Range(0f, 1f)] public float lowFreqStrength = 0.6f; // später Bass-Layer, Filter etc.

    [Header("Visuals")]
    public Color mainColor = new Color(0.2f, 0.4f, 1f, 1f);
    [Range(0f, 1f)] public float motionSpeed = 0.2f;
    [Range(0f, 1f)] public float baseIntensity = 0.6f;
}
