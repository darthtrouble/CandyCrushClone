using UnityEngine;
using TMPro; 

public class ScoreManager : MonoBehaviour {

    public TextMeshProUGUI scoreText;
    public int score = 0;
    
    [Header("Combo System")]
    public GameObject comboTextPrefab;
    public int comboMultiplier = 1;

    // Polish: Tracking for counting animation
    private int currentDisplayedScore = 0;
    private Coroutine scoreCoroutine;

    private int visualTargetScore = 0; // The goal for the UI animation

    void Start() {
        if (scoreText == null) {
            GameObject scoreObj = GameObject.Find("ScoreText");
            if (scoreObj != null) {
                scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
            }
            
            if (scoreText == null) {
                scoreText = FindFirstObjectByType<TextMeshProUGUI>();
                if (scoreText != null) Debug.LogWarning("ScoreManager: Found a TextMeshProUGUI but unsure if it's the score. Please assign 'ScoreText' in Inspector.");
            }

            if(scoreText == null) Debug.LogError("SCORE MANAGER ERROR: No UI Text found in the scene! Create a TextMeshPro object named 'ScoreText'.");
        }
        
        UpdateScoreText(false); 
        visualTargetScore = score; // Sync start
    }

    // Returns the actual points added (including multiplier)
    public int IncreaseScore(int amountToIncrease) {
        int pointsToAdd = amountToIncrease * comboMultiplier;
        score += pointsToAdd;
        // Do NOT update UI here. The floating text will trigger it when it arrives.
        return pointsToAdd;
    }

    public void AddVisibleScore(int amount) {
        visualTargetScore += amount;
        UpdateScoreText(true);
    }

    private void UpdateScoreText(bool animate) {
        if(scoreText != null) {
            if(animate) {
                if(scoreCoroutine != null) StopCoroutine(scoreCoroutine);
                scoreCoroutine = StartCoroutine(CountUpScore());
            } else {
                currentDisplayedScore = score;
                scoreText.text = "Score: " + score;
            }
        }
    }
    
    private System.Collections.IEnumerator CountUpScore() {
        float duration = 0.5f;
        float elapsed = 0f;
        int startScore = currentDisplayedScore;
        
        // Punch effect on text
        StartCoroutine(PunchEffect(scoreText.transform));

        while(elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // EaseOutQuad
            t = t * (2 - t);
            
            // Animate towards the VISUAL target, not the total score (to allow sequential updates)
            currentDisplayedScore = (int)Mathf.Lerp(startScore, visualTargetScore, t);
            scoreText.text = "Score: " + currentDisplayedScore;
            yield return null;
        }
        currentDisplayedScore = visualTargetScore;
        scoreText.text = "Score: " + currentDisplayedScore;
    }

    private System.Collections.IEnumerator PunchEffect(Transform target) {
        Vector3 original = Vector3.one;
        float elapsed = 0f;
        float duration = 0.2f;
        
        while(elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Simple ping-pong scale
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
            target.localScale = original * scale;
            yield return null;
        }
        target.localScale = original;
    }
    
    private Coroutine comboResetCoroutine;
    
    public void HandleCombo() {
        comboMultiplier++;
        
        if (comboTextPrefab != null) {
            // Determine flavor text based on combo
            string flavorText = "Combo " + comboMultiplier;
            Color flavorColor = Color.white;
            float scaleMult = 1f;

            if (comboMultiplier >= 8) {
                flavorText = "Divine!";
                flavorColor = new Color(0.2f, 1f, 1f); // Cyan
                scaleMult = 2.0f;
            } else if (comboMultiplier >= 5) {
                flavorText = "Tasty!";
                flavorColor = new Color(1f, 0.4f, 0.8f); // Pink/Purple
                scaleMult = 1.5f;
            } else if (comboMultiplier >= 3) {
                flavorText = "Sweet!";
                flavorColor = new Color(1f, 0.8f, 0.2f); // Golden/Orange
                scaleMult = 1.2f;
            }

            GameObject comboText = Instantiate(comboTextPrefab, transform.position, Quaternion.identity);
            
            // Assuming the text is on a child of the prefab
            TextMeshProUGUI textMesh = comboText.GetComponentInChildren<TextMeshProUGUI>();
            if(textMesh != null) {
                textMesh.text = flavorText;
                textMesh.color = flavorColor;
            }
            // Scale up slightly if possible
            comboText.transform.localScale *= scaleMult;
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