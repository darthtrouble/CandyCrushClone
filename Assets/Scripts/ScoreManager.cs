using UnityEngine;
using TMPro; 

public class ScoreManager : MonoBehaviour {

    public TextMeshProUGUI scoreText;
    public int score = 0;
    
    [Header("Combo System")]
    public GameObject comboTextPrefab;
    public int comboMultiplier = 1;

    void Start() {
        // FIXED: Updated to the new Unity 2023 syntax
        if (scoreText == null) {
            scoreText = FindFirstObjectByType<TextMeshProUGUI>();
            if(scoreText == null) Debug.LogError("SCORE MANAGER ERROR: No UI Text found in the scene!");
        }
        
        UpdateScoreText();
    }

    public void IncreaseScore(int amountToIncrease) {
        score += amountToIncrease * comboMultiplier;
        UpdateScoreText();
    }

    private void UpdateScoreText() {
        if(scoreText != null) {
            scoreText.text = "Score: " + score;
        }
    }
    
    public void HandleCombo() {
        comboMultiplier++;
        
        if (comboTextPrefab != null) {
            GameObject comboText = Instantiate(comboTextPrefab, transform.position, Quaternion.identity);
            
            // Assuming the text is on a child of the prefab
            TextMeshProUGUI textMesh = comboText.GetComponentInChildren<TextMeshProUGUI>();
            if(textMesh != null) textMesh.text = "Combo x" + comboMultiplier;
        }
        
        // Cancel previous reset invoke to avoid premature resets
        CancelInvoke("ResetCombo");
        
        // Reset the combo after 2 seconds of inactivity
        Invoke("ResetCombo", 2f);
    }

    private void ResetCombo() {
        comboMultiplier = 1;
    }
}