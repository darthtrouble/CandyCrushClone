using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; // Needed for Text

public class MainMenu : MonoBehaviour {

    public TextMeshProUGUI highScoreText;

    void Start() {
        // 1. Ask Unity: "Do we have a saved 'HighScore' number?"
        // If yes, get it. If no, give me 0.
        int bestScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // 2. Update the text
        if(highScoreText != null) {
            highScoreText.text = "Best: " + bestScore;
        }
    }

    public void PlayGame() {
        SceneManager.LoadScene("GameLevel");
    }

    public void QuitGame() {
        Application.Quit();
    }
}