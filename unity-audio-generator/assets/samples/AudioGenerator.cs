using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public static class AudioGenerator
{
    private const int SampleRate = 44100;

    [MenuItem("Tools/Generate All Audio Assets")]
    public static void GenerateAll()
    {
        GenerateSE();
        GenerateBGM();
        AssetDatabase.Refresh();
        Debug.Log("[AudioGenerator] All audio assets generated.");
    }

    private static void GenerateSE()
    {
        // hit - short punchy sound
        SaveWav("Assets/Audio/SE/hit.wav", GenerateTone(0.15f, 440f, 0.8f, decay: 8f));

        // bomb - low explosion
        SaveWav("Assets/Audio/SE/bomb.wav", GenerateNoiseBurst(0.4f, 0.9f, 100f));

        // appear - quick rising tone
        SaveWav("Assets/Audio/SE/appear.wav", GenerateSweep(0.2f, 300f, 600f, 0.5f));

        // combo - bright ascending beep
        SaveWav("Assets/Audio/SE/combo.wav", GenerateTone(0.12f, 880f, 0.6f, decay: 10f));

        // warning - short beep
        SaveWav("Assets/Audio/SE/warning.wav", GenerateTone(0.1f, 1000f, 0.7f, decay: 12f));

        // clear - happy jingle (ascending arpeggio)
        SaveWav("Assets/Audio/SE/clear.wav", GenerateJingle(true));

        // gameover - sad descending tone
        SaveWav("Assets/Audio/SE/gameover.wav", GenerateJingle(false));

        // button - soft click
        SaveWav("Assets/Audio/SE/button.wav", GenerateTone(0.08f, 660f, 0.4f, decay: 15f));
    }

    private static void GenerateBGM()
    {
        SaveWav("Assets/Audio/BGM/bgm.wav", GenerateSimpleBGM(8f));
    }

    private static float[] GenerateTone(float duration, float freq, float volume, float decay = 5f)
    {
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-decay * t);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * envelope;
        }
        return data;
    }

    private static float[] GenerateNoiseBurst(float duration, float volume, float filterFreq)
    {
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];
        System.Random rng = new System.Random(42);
        float prev = 0f;
        float rc = 1f / (2f * Mathf.PI * filterFreq);
        float dt = 1f / SampleRate;
        float alpha = dt / (rc + dt);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-4f * t);
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            prev = prev + alpha * (noise - prev);
            data[i] = prev * volume * envelope * 2f;
        }
        return data;
    }

    private static float[] GenerateSweep(float duration, float startFreq, float endFreq, float volume)
    {
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float progress = t / duration;
            float freq = Mathf.Lerp(startFreq, endFreq, progress);
            float envelope = Mathf.Exp(-3f * t);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * envelope;
        }
        return data;
    }

    private static float[] GenerateJingle(bool ascending)
    {
        float[] notes;
        if (ascending)
            notes = new float[] { 523.25f, 659.25f, 783.99f, 1046.5f }; // C5 E5 G5 C6
        else
            notes = new float[] { 493.88f, 392f, 329.63f, 261.63f }; // B4 G4 E4 C4

        float noteDuration = 0.2f;
        int totalSamples = (int)(SampleRate * noteDuration * notes.Length);
        float[] data = new float[totalSamples];

        for (int n = 0; n < notes.Length; n++)
        {
            int offset = (int)(SampleRate * noteDuration * n);
            int count = (int)(SampleRate * noteDuration);
            for (int i = 0; i < count && (offset + i) < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-5f * t);
                data[offset + i] += Mathf.Sin(2f * Mathf.PI * notes[n] * t) * 0.6f * envelope;
            }
        }
        return data;
    }

    private static float[] GenerateSimpleBGM(float duration)
    {
        int samples = (int)(SampleRate * duration);
        float[] data = new float[samples];

        // Simple loopable melody pattern
        float[] melody = { 261.63f, 329.63f, 392f, 329.63f, 349.23f, 392f, 440f, 392f };
        float beatDuration = duration / melody.Length;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            int beatIndex = Mathf.Min((int)(t / beatDuration), melody.Length - 1);
            float beatT = (t - beatIndex * beatDuration) / beatDuration;

            // Melody (sine)
            float melodyVol = 0.3f * Mathf.Clamp01(1f - beatT * 1.5f);
            float melodyVal = Mathf.Sin(2f * Mathf.PI * melody[beatIndex] * t) * melodyVol;

            // Bass (low octave)
            float bassFreq = melody[beatIndex] * 0.5f;
            float bassVal = Mathf.Sin(2f * Mathf.PI * bassFreq * t) * 0.15f;

            // Simple percussion (noise on beats)
            float percVal = 0f;
            float beatPos = t % beatDuration;
            if (beatPos < 0.05f)
            {
                percVal = Mathf.Sin(beatPos * 200f) * 0.2f * (1f - beatPos / 0.05f);
            }

            data[i] = melodyVal + bassVal + percVal;
        }
        return data;
    }

    private static void SaveWav(string path, float[] samples)
    {
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using (var stream = new FileStream(path, FileMode.Create))
        using (var writer = new BinaryWriter(stream))
        {
            int channels = 1;
            int bitsPerSample = 16;
            int byteRate = SampleRate * channels * bitsPerSample / 8;
            int blockAlign = channels * bitsPerSample / 8;
            int dataSize = samples.Length * blockAlign;

            // RIFF header
            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });

            // fmt chunk
            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write((short)channels);
            writer.Write(SampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)bitsPerSample);

            // data chunk
            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);

            for (int i = 0; i < samples.Length; i++)
            {
                float clamped = Mathf.Clamp(samples[i], -1f, 1f);
                short val = (short)(clamped * 32767f);
                writer.Write(val);
            }
        }
    }
}
