using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper component for individual objective items.
/// Attach this to your ObjectivePrefab to make it easier to assign references.
/// </summary>
public class ObjectiveItem : MonoBehaviour {
    
    [Header("UI References")]
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public GameObject checkmark;
    
    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color completedColor = Color.green;
    public float completionScalePunch = 1.2f;
    
    private LevelObjectiveType objectiveType;
    private bool isCompleted = false;
    
    public void Setup(LevelObjective objective) {
        objectiveType = objective.objectiveType;
        
        // Set description based on objective type
        switch (objective.objectiveType) {
            case LevelObjectiveType.Score:
                descriptionText.text = "Reach Score";
                UpdateProgress(0, objective.targetScore);
                break;
                
            case LevelObjectiveType.CollectAnimals:
                descriptionText.text = $"Collect {objective.animalTag}";
                UpdateProgress(0, objective.targetAmount);
                break;
                
            case LevelObjectiveType.ClearIce:
                descriptionText.text = "Clear All Ice";
                int totalIce = objective.targetAmount > 0 ? objective.targetAmount : 10;
                UpdateProgress(0, totalIce);
                break;
                
            case LevelObjectiveType.TimedChallenge:
                descriptionText.text = "Reach Score (Timed)";
                UpdateProgress(0, objective.targetScore);
                break;
        }
        
        if (checkmark != null) checkmark.SetActive(false);
    }
    
    public void UpdateProgress(int current, int target) {
        if (progressText != null) {
            progressText.text = $"{current}/{target}";
            
            // Update color based on progress
            float progress = (float)current / target;
            if (progress >= 1f) {
                MarkComplete();
            } else if (progress >= 0.75f) {
                progressText.color = Color.Lerp(normalColor, completedColor, (progress - 0.75f) * 4f);
            } else {
                progressText.color = normalColor;
            }
        }
    }
    
    public void MarkComplete() {
        if (isCompleted) return;
        isCompleted = true;
        
        // Show checkmark
        if (checkmark != null) {
            checkmark.SetActive(true);
            StartCoroutine(PunchScale(checkmark.transform));
        }
        
        // Change text colors
        if (descriptionText != null) descriptionText.color = completedColor;
        if (progressText != null) {
            progressText.color = completedColor;
            progressText.text = "✓ Complete";
        }
    }
    
    private System.Collections.IEnumerator PunchScale(Transform target) {
        Vector3 originalScale = target.localScale;
        float time = 0;
        float duration = 0.3f;
        
        while (time < duration) {
            time += Time.deltaTime;
            float t = time / duration;
            float scale = Mathf.Lerp(0, completionScalePunch, t);
            target.localScale = originalScale * scale;
            yield return null;
        }
        
        time = 0;
        while (time < duration) {
            time += Time.deltaTime;
            float t = time / duration;
            float scale = Mathf.Lerp(completionScalePunch, 1f, t);
            target.localScale = originalScale * scale;
            yield return null;
        }
        
        target.localScale = originalScale;
    }
}
