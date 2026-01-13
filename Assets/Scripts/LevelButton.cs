using UnityEngine;
using TMPro;
using UnityEngine.UI; // Required for Button
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour {

    [Header("UI References")]
    public TextMeshProUGUI levelText;
    public Button myButton;
    public GameObject lockedOverlay; // Drag your 'LockedOverlay' (The X) here!

    private int levelIndex;

    public void Setup(int level, bool isUnlocked) {
        levelIndex = level;
        levelText.text = (level + 1).ToString(); // Display "1" for Level 0

        if (isUnlocked) {
            // UNLOCKED STATE
            myButton.interactable = true;       // Make it clickable
            lockedOverlay.SetActive(false);     // Hide the X
            
            // Clear old listeners to prevent double-clicks if recycled
            myButton.onClick.RemoveAllListeners();
            
            // Add click logic
            myButton.onClick.AddListener(() => {
                PlayerPrefs.SetInt("CurrentLevel", levelIndex);
                SceneManager.LoadScene("GameLevel");
            });
        } 
        else {
            // LOCKED STATE
            myButton.interactable = false;      // Make it unclickable
            lockedOverlay.SetActive(true);      // Show the X
        }
    }
}