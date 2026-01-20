using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour {

    [Header("Audio Reference")]
    public AudioMixer mainMixer; 

    [Header("UI Elements")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle shakeToggle;

    // CHANGED: Use OnEnable so it updates every time the panel opens
    void OnEnable() {
        // 1. Load & Update Music Slider
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        if(musicSlider != null) {
            musicSlider.value = savedMusic;
            // Force the event to run so the Mixer updates immediately
            SetMusicVolume(savedMusic); 
        }

        // 2. Load & Update SFX Slider
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        if(sfxSlider != null) {
            sfxSlider.value = savedSFX;
            SetSFXVolume(savedSFX);
        }

        // 3. Load Shake Toggle
        int shakeInt = PlayerPrefs.GetInt("ShakeEnabled", 1);
        if(shakeToggle != null) shakeToggle.isOn = (shakeInt == 1);
    }

    public void SetMusicVolume(float value) {
        if(mainMixer == null) return;
        float dB = (value <= 0.001f) ? -80f : Mathf.Log10(value) * 20;
        mainMixer.SetFloat("MusicVol", dB);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value) {
        if(mainMixer == null) return;
        float dB = (value <= 0.001f) ? -80f : Mathf.Log10(value) * 20;
        mainMixer.SetFloat("SFXVol", dB);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetShake(bool isOn) {
        PlayerPrefs.SetInt("ShakeEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    // Save on close, just to be safe
    public void SaveSettings() {
        PlayerPrefs.Save();
    }
}