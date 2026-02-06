using UnityEngine;

public static class ProceduralAudio {

    public static AudioClip CreateWinClip() {
        int sampleRate = 44100;
        float duration = 1.0f;
        int sampleCount = (int)(sampleRate * duration);
        float[] data = new float[sampleCount];

        // Major Arpeggio: C5 (523.25), E5 (659.25), G5 (783.99), C6 (1046.50)
        float[] frequencies = { 523.25f, 659.25f, 783.99f, 1046.50f };
        float noteDuration = duration / frequencies.Length;

        int currentNote = 0;
        float phase = 0;

        for (int i = 0; i < sampleCount; i++) {
            float t = (float)i / sampleRate;
            
            // Switch notes
            if (t > (currentNote + 1) * noteDuration && currentNote < frequencies.Length - 1) {
                currentNote++;
            }

            float freq = frequencies[currentNote];
            phase += freq * 2 * Mathf.PI / sampleRate;
            
            // Simple Sine Wave with Decay per note
            float noteT = (t % noteDuration) / noteDuration; // 0 to 1 for this note
            float envelope = 1f - noteT; // Linear decay

            data[i] = Mathf.Sin(phase) * 0.5f * envelope;
        }

        AudioClip clip = AudioClip.Create("WinProcedural", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip CreateLoseClip() {
        int sampleRate = 44100;
        float duration = 1.0f;
        int sampleCount = (int)(sampleRate * duration);
        float[] data = new float[sampleCount];

        float startFreq = 400f;
        float endFreq = 100f;
        float phase = 0;

        for (int i = 0; i < sampleCount; i++) {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(startFreq, endFreq, t); // Linear slide down
            
            phase += freq * 2 * Mathf.PI / sampleRate;

            // Sawtooth-ish wave for "buzzer" feel (Sine + harmonics)
            float val = Mathf.Sin(phase) + 0.5f * Mathf.Sin(phase * 2) + 0.25f * Mathf.Sin(phase * 4);
            
            // Envelope: Sustain then fade out at end
            float envelope = i > sampleCount * 0.8f ? 1f - ((t - 0.8f) / 0.2f) : 1f;

            data[i] = val * 0.3f * envelope;
        }

        AudioClip clip = AudioClip.Create("LoseProcedural", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
