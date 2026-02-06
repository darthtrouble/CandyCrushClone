using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class BackgroundMusic : MonoBehaviour {

    public AudioMixer mainMixer;
    public AudioClip menuMusic;   // Drag your new menu music here (or leave null for procedural)
    public AudioClip gameMusic;   // Drag your existing game music here
    
    private AudioSource audioSource;
    private static BackgroundMusic instance;

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "MainMenu" || scene.name == "LevelSelect") {
            // Fallback to gameMusic (original user music) if no dedicated menu music is set
            AudioClip clipToPlay = menuMusic != null ? menuMusic : gameMusic;
            if(clipToPlay != null) PlayMusic(clipToPlay);
        } else {
            // Assume Game Level
            if (gameMusic != null) PlayMusic(gameMusic);
        }
    }
    
    void PlayMusic(AudioClip clip) {
        if (audioSource.clip == clip) return; // Don't restart if already playing
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    void Start() {
        ApplyVolume();
        // Trigger manually for the first scene
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
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