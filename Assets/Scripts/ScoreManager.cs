using UnityEngine;
using TMPro; 

public class ScoreManager : MonoBehaviour {

    public TextMeshProUGUI scoreText;
    public int score = 0;

    void Start() {
        // FIXED: Updated to the new Unity 2023 syntax
        if (scoreText == null) {
            scoreText = FindFirstObjectByType<TextMeshProUGUI>();
            if(scoreText == null) Debug.LogError("SCORE MANAGER ERROR: No UI Text found in the scene!");
        }
        
        UpdateScoreText();
    }

    public void IncreaseScore(int amountToIncrease) {
        score += amountToIncrease;
        UpdateScoreText();
    }

    private void UpdateScoreText() {
        if(scoreText != null) {
            scoreText.text = "Score: " + score;
        }
    }
}