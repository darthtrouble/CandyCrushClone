using UnityEngine;
using UnityEngine.Audio; // Required for Mixer

public class BackgroundMusic : MonoBehaviour {

    public AudioMixer mainMixer; // Drag your MainMixer here!

    private static BackgroundMusic instance;

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        // Apply the saved volume immediately when the game starts
        ApplyVolume();
    }

    public void ApplyVolume() {
        if (mainMixer == null) return;

        // 1. Load & Set Music
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        mainMixer.SetFloat("MusicVol", ConvertToDecibels(savedMusic));

        // 2. Load & Set SFX
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        mainMixer.SetFloat("SFXVol", ConvertToDecibels(savedSFX));
    }
    
    // Helper method to convert linear volume (0-1) to decibels
    private float ConvertToDecibels(float linearVolume) {
        return (linearVolume <= 0.001f) ? -80f : Mathf.Log10(linearVolume) * 20;
    }
}