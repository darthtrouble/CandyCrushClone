using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour {

    public TextMeshProUGUI levelText;
    public Button myButton;
    public GameObject lockIcon; // Optional: drag a lock image here if you have one

    private int levelIndex;

    public void Setup(int level, bool isUnlocked) {
        levelIndex = level;
        levelText.text = (level + 1).ToString(); // Display "1" for Level 0

        if (isUnlocked) {
            myButton.interactable = true;
            if(lockIcon != null) lockIcon.SetActive(false);
            
            // On Click, save which level we want and load the game
            myButton.onClick.AddListener(() => {
                PlayerPrefs.SetInt("CurrentLevel", levelIndex);
                SceneManager.LoadScene("GameLevel");
            });
        } else {
            myButton.interactable = false; // Greyed out
            myButton.image.color = Color.grey; 
            if(lockIcon != null) lockIcon.SetActive(true);
        }
    }
}