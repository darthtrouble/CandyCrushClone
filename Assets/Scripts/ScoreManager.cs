using UnityEngine;
using TMPro; 

public class ScoreManager : MonoBehaviour {

    public TextMeshProUGUI scoreText;
    public int score = 0;
    
    [Header("Combo System")]
    public GameObject comboTextPrefab;
    public int comboMultiplier = 1;

    void Start() {
        // FIXED: Updated lookup logic to be more specific
        if (scoreText == null) {
            // First try to find by name "ScoreText"
            GameObject scoreObj = GameObject.Find("ScoreText");
            if (scoreObj != null) {
                scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
            }
            
            // Fallback: Find first type but warn
            if (scoreText == null) {
                scoreText = FindFirstObjectByType<TextMeshProUGUI>();
                if (scoreText != null) Debug.LogWarning("ScoreManager: Found a TextMeshProUGUI but unsure if it's the score. Please assign 'ScoreText' in Inspector.");
            }

            if(scoreText == null) Debug.LogError("SCORE MANAGER ERROR: No UI Text found in the scene! Create a TextMeshPro object named 'ScoreText'.");
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
    
    private Coroutine comboResetCoroutine;
    
    public void HandleCombo() {
        comboMultiplier++;
        
        if (comboTextPrefab != null) {
            GameObject comboText = Instantiate(comboTextPrefab, transform.position, Quaternion.identity);
            
            // Assuming the text is on a child of the prefab
            TextMeshProUGUI textMesh = comboText.GetComponentInChildren<TextMeshProUGUI>();
            if(textMesh != null) textMesh.text = "Combo x" + comboMultiplier;
        }
        
        // Optimized: Use coroutine instead of Invoke for better performance
        if (comboResetCoroutine != null) StopCoroutine(comboResetCoroutine);
        comboResetCoroutine = StartCoroutine(ResetComboAfterDelay(2f));
    }
    
    private System.Collections.IEnumerator ResetComboAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        ResetCombo();
    }

    private void ResetCombo() {
        comboMultiplier = 1;
    }
}