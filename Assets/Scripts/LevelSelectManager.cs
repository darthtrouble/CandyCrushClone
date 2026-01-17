using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelectManager : MonoBehaviour {

    [Header("Configuration")]
    public string gameSceneName = "GameLevel";
    public Button[] levelButtons; // Drag your buttons (Level 1, Level 2, etc.) here
    public Sprite lockedSprite;   // Optional: Sprite for a padlock
    public Sprite unlockedSprite; // Optional: Sprite for the number/star

    void Start() {
        // 1. Get progress (Default to Level 1 if no save exists)
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // 2. Loop through all buttons
        for (int i = 0; i < levelButtons.Length; i++) {
            // Level numbers start at 1, but array starts at 0
            int levelNum = i + 1;

            if (levelNum <= unlockedLevel) {
                // --- UNLOCKED ---
                levelButtons[i].interactable = true;
                
                // Visuals (Optional)
                if(unlockedSprite != null) levelButtons[i].image.sprite = unlockedSprite;
                
                // Set the text to the number (e.g. "1")
                TextMeshProUGUI btnText = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if(btnText) btnText.text = levelNum.ToString();
                
                // Add Click Listener via Code (so you don't have to drag dropping manually)
                int indexToLoad = i; // Local copy for the closure
                levelButtons[i].onClick.AddListener(() => LoadLevel(indexToLoad));
            } 
            else {
                // --- LOCKED ---
                levelButtons[i].interactable = false;
                
                // Visuals (Optional)
                if(lockedSprite != null) levelButtons[i].image.sprite = lockedSprite;
                
                // Hide text or show lock
                TextMeshProUGUI btnText = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if(btnText) btnText.text = ""; 
            }
        }
    }

    public void LoadLevel(int levelIndex) {
        // Save which level we want to play
        PlayerPrefs.SetInt("CurrentLevel", levelIndex);
        
        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }
    
    // Add this to a "Back" button
    public void GoBackToMenu() {
        SceneManager.LoadScene("MainMenu");
    }
    
    // Dev Tool: Call this to reset progress
    public void ResetProgress() {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}