using UnityEngine;

/// <summary>
/// Generates procedural water splash / whip audio clips at runtime
/// and provides a static Play method for fire-and-forget use.
/// </summary>
public static class WaterSplashAudio
{
    private static AudioClip whipClip;
    private static AudioClip splashClip;

    private const int SampleRate = 44100;
    private const float WhipDuration = 0.18f;
    private const float SplashDuration = 0.22f;
    private const float MasterVolume = 0.35f;

    /// <summary>
    /// Play the water whip sound (horizontal attack).
    /// </summary>
    public static void PlayWhip(Vector3 position)
    {
        if (whipClip == null)
        {
            whipClip = GenerateWhipClip();
        }

        AudioSource.PlayClipAtPoint(whipClip, position, MasterVolume);
    }

    /// <summary>
    /// Play the water splash sound (downward stomp attack).
    /// </summary>
    public static void PlaySplash(Vector3 position)
    {
        if (splashClip == null)
        {
            splashClip = GenerateSplashClip();
        }

        AudioSource.PlayClipAtPoint(splashClip, position, MasterVolume);
    }

    private static AudioClip GenerateWhipClip()
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * WhipDuration);
        float[] samples = new float[sampleCount];

        // Water whip: descending frequency sweep with noise, mimicking a fast lash through water
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount; // 0..1 progress
            float envelope = (1f - t) * (1f - t); // quadratic decay

            // Frequency sweeps downward from ~2200 Hz to ~400 Hz
            float freq = Mathf.Lerp(2200f, 400f, t);
            float phase = 2f * Mathf.PI * freq * i / SampleRate;

            // Mix a sine tone with filtered noise for a watery texture
            float tone = Mathf.Sin(phase) * 0.45f;
            float noise = (Random.value * 2f - 1f) * 0.55f;

            // Simple low-pass effect: blend with previous sample
            float raw = (tone + noise) * envelope;
            if (i > 0)
            {
                raw = samples[i - 1] * 0.3f + raw * 0.7f;
            }

            samples[i] = Mathf.Clamp(raw, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("WaterWhip", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerateSplashClip()
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * SplashDuration);
        float[] samples = new float[sampleCount];

        // Water splash: burst of noise with resonant low thump, like hitting water from above
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;

            // Sharp attack, moderate decay
            float envelope = Mathf.Pow(1f - t, 1.5f);
            // Extra initial punch
            float attackBoost = t < 0.08f ? Mathf.Lerp(1.6f, 1f, t / 0.08f) : 1f;

            // Low resonant thump (~180 Hz decaying)
            float thumpFreq = Mathf.Lerp(180f, 90f, t);
            float thump = Mathf.Sin(2f * Mathf.PI * thumpFreq * i / SampleRate) * 0.4f;

            // Mid splash texture (~600-1400 Hz band noise)
            float noise = (Random.value * 2f - 1f);
            float midFreq = Mathf.Lerp(1400f, 600f, t);
            float midTone = Mathf.Sin(2f * Mathf.PI * midFreq * i / SampleRate);
            float splash = noise * 0.35f + midTone * noise * 0.25f;

            // High sparkle (bubbly overtones)
            float highFreq = Mathf.Lerp(3200f, 1800f, t);
            float sparkle = Mathf.Sin(2f * Mathf.PI * highFreq * i / SampleRate) * (1f - t) * 0.15f;

            float raw = (thump + splash + sparkle) * envelope * attackBoost;

            // Low-pass smoothing
            if (i > 0)
            {
                raw = samples[i - 1] * 0.25f + raw * 0.75f;
            }

            samples[i] = Mathf.Clamp(raw, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("WaterSplash", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
