using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour {

    [Header("UI Elements")]
    public Slider volumeSlider;
    public Toggle shakeToggle;

    void Start() {
        // 1. Load Volume (Default to 1.0 / Max)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if(volumeSlider != null) volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume; // Sets global game volume

        // 2. Load Shake (Default to 1 / On)
        int shakeInt = PlayerPrefs.GetInt("ShakeEnabled", 1);
        if(shakeToggle != null) shakeToggle.isOn = (shakeInt == 1);
    }

    public void SetVolume(float value) {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
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