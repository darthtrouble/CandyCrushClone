using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio; // REQUIRED for Mixers

public class SettingsManager : MonoBehaviour {

    [Header("Audio Reference")]
    public AudioMixer mainMixer; // Drag MainMixer here

    [Header("UI Elements")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle shakeToggle;

    void Start() {
        // 1. Load Music (Default 0.5)
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        if(musicSlider != null) musicSlider.value = savedMusic;
        SetMusicVolume(savedMusic); // Apply immediately

        // 2. Load SFX (Default 0.5)
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        if(sfxSlider != null) sfxSlider.value = savedSFX;
        SetSFXVolume(savedSFX); // Apply immediately

        // 3. Load Shake
        int shakeInt = PlayerPrefs.GetInt("ShakeEnabled", 1);
        if(shakeToggle != null) shakeToggle.isOn = (shakeInt == 1);
    }

    public void SetMusicVolume(float value) {
        // Convert 0-1 slider to Logarithmic Decibels (-80 to 0)
        float dB = (value <= 0.001f) ? -80f : Mathf.Log10(value) * 20;
        
        mainMixer.SetFloat("MusicVol", dB);
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value) {
        float dB = (value <= 0.001f) ? -80f : Mathf.Log10(value) * 20;

        mainMixer.SetFloat("SFXVol", dB);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    public void SetShake(bool isOn) {
        PlayerPrefs.SetInt("ShakeEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void BackToMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}