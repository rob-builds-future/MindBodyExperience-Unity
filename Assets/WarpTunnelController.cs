using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class WarpTunnelCurvedGlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private Camera cam;

    [Header("Tunnel Geometry")]
    public float innerRadius = 1.2f;
    public float outerRadius = 6.0f;

    public float nearDist = 0.4f;
    public float farDist = 60f;

    [Header("Curve")]
    public float bendStrength = 4.0f;
    public float bendExponent = 2.2f;

    [Header("Motion")]
    [Range(0.1f, 20f)] public float speed = 4f; // ruhig!

    [Header("Glow / Size")]
    public float farSize = 0.05f;
    public float nearSize = 0.22f;

    [Header("Population")]
    [Range(500, 12000)] public int particleCount = 5000;

    [Header("Color")]
    public Color particleColor = new Color(0.75f, 0.9f, 1f, 0.35f);

    private ParticleSystem.Particle[] particles;

    // 👇 per-particle state (DAS war der fehlende Teil)
    private float[] tValues;     // 0..1 position along tunnel
    private float[] angles;      // ring angle
    private float[] radii;       // ring radius

    private void OnEnable()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();
        if (cam == null) cam = Camera.main;
        if (ps == null || cam == null) return;

        var main = ps.main;
        main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0f;
        main.maxParticles = Mathf.Max(main.maxParticles, particleCount);

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        ps.Clear(true);

        particles = new ParticleSystem.Particle[particleCount];
        tValues  = new float[particleCount];
        angles   = new float[particleCount];
        radii    = new float[particleCount];

        // initialize stable per-particle state
        for (int i = 0; i < particleCount; i++)
        {
            tValues[i] = Random.value;                       // depth
            angles[i]  = Random.value * Mathf.PI * 2f;      // fixed angle
            radii[i]   = Mathf.Lerp(innerRadius, outerRadius, Mathf.Sqrt(Random.value));
            UpdateParticle(i);
        }

        ps.SetParticles(particles, particles.Length);
        ps.Play(true);
    }

    private void UpdateParticle(int i)
    {
        float t = tValues[i];

        float d = Mathf.Lerp(nearDist, farDist, t);

        float curve = Mathf.Pow(t, bendExponent);
        float bendY = -bendStrength * curve;

        var ct = cam.transform;

        float x = Mathf.Cos(angles[i]) * radii[i];
        float y = Mathf.Sin(angles[i]) * radii[i];

        Vector3 pos =
            ct.position
            + ct.forward * d
            + ct.right * x
            + ct.up * (y + bendY);

        particles[i].position = pos;
        particles[i].startColor = particleColor;
        particles[i].startSize = Mathf.Lerp(nearSize, farSize, t);
        particles[i].startLifetime = 999f;
        particles[i].remainingLifetime = 999f;
    }

    private void LateUpdate()
    {
        if (particles == null) return;

        float deltaT = (speed / farDist) * Time.deltaTime;

        for (int i = 0; i < particles.Length; i++)
        {
            tValues[i] -= deltaT;

            if (tValues[i] <= 0f)
                tValues[i] = 1f;

            UpdateParticle(i);
        }

        ps.SetParticles(particles, particles.Length);
    }
}
