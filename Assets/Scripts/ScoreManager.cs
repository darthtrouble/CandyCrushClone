using UnityEngine;
using TMPro; 

public class ScoreManager : MonoBehaviour {

    public TextMeshProUGUI scoreText;
    public int score = 0;

    void Start() {
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