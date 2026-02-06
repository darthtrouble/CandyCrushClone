using UnityEngine;

public static class ProceduralMusic {

    public static AudioClip CreateMenuMusic() {
        int sampleRate = 44100;
        float duration = 8.0f; // 8 second loop
        int sampleCount = (int)(sampleRate * duration);
        float[] data = new float[sampleCount];

        // Simple C Major Chord Progression: C -> G -> Am -> F
        // Frequencies: C4(261.63), G3(196.00), A3(220.00), F3(174.61)
        float[] rootNotes = { 261.63f, 196.00f, 220.00f, 174.61f };
        float chordDuration = duration / 4;

        float phase = 0;

        for (int i = 0; i < sampleCount; i++) {
            float t = (float)i / sampleRate;
            int currentChord = (int)(t / chordDuration);
            float freq = rootNotes[currentChord];
            
            phase += freq * 2 * Mathf.PI / sampleRate;

            // Simple Organ-like tone (fundamental + harmonics)
            float val = Mathf.Sin(phase) * 0.5f + 
                        Mathf.Sin(phase * 2) * 0.2f + 
                        Mathf.Sin(phase * 4) * 0.1f;
            
            // Envelope per chord (gentle attack, sustain, release)
            float chordT = (t % chordDuration) / chordDuration;
            float envelope = 1f;
            if(chordT < 0.1f) envelope = chordT / 0.1f; // Attack
            else if(chordT > 0.9f) envelope = 1f - ((chordT - 0.9f) / 0.1f); // Release

            data[i] = val * 0.2f * envelope;
        }

        AudioClip clip = AudioClip.Create("MenuMusicProcedural", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
