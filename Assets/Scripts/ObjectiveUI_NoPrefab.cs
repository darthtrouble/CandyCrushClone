using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays current level objectives on the game screen
/// Updates in real-time as player progresses
/// VERSION 2: No prefab required - creates UI at runtime
/// </summary>
public class ObjectiveUI_NoPrefab : MonoBehaviour {
    
    [Header("UI References")]
    public GameObject objectivePanel;
    public Transform objectiveContainer; // Parent for objective items (needs Vertical Layout Group)
    
    [Header("Optional Specific Elements")]
    public TextMeshProUGUI timerText; // For timed challenges
    
    [Header("Colors")]
    public Color completedColor = Color.green;
    public Color timerWarningColor = Color.red;
    
    private List<ObjectiveItem> activeObjectives = new List<ObjectiveItem>();
    private Board board;
    
    // Helper class to track individual objective UI
    private class ObjectiveItem {
        public GameObject uiObject;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI progressText;
        public Image checkmark;
        public LevelObjective objective;
        public bool isCompleted = false;
    }
    
    void Start() {
        board = FindFirstObjectByType<Board>();
        if (objectivePanel != null) objectivePanel.SetActive(true);
        
        Debug.Log("[ObjectiveUI] Started. Panel active: " + (objectivePanel != null));
    }
    
    /// <summary>
    /// Initialize objectives from level data
    /// </summary>
    public void SetupObjectives(List<LevelObjective> objectives) {
        Debug.Log($"[ObjectiveUI] SetupObjectives called! Count: {objectives.Count}");
        
        // Clear existing
        foreach (var item in activeObjectives) {
            if (item.uiObject != null) Destroy(item.uiObject);
        }
        activeObjectives.Clear();
        
        // Create UI for each objective
        foreach (var objective in objectives) {
            Debug.Log($"[ObjectiveUI] Creating UI for: {objective.objectiveType}");
            CreateObjectiveUI(objective);
        }
        
        // Show/hide timer based on objectives
        if (timerText != null) {
            bool hasTimed = objectives.Exists(o => o.objectiveType == LevelObjectiveType.TimedChallenge);
            timerText.gameObject.SetActive(hasTimed);
            Debug.Log($"[ObjectiveUI] Timer visibility: {hasTimed}");
        }
    }
    
    private void CreateObjectiveUI(LevelObjective objective) {
        Debug.Log($"[ObjectiveUI] CreateObjectiveUI called for {objective.objectiveType}");
        
        if (objectiveContainer == null) {
            Debug.LogError("[ObjectiveUI] objectiveContainer is NULL! Assign it in Inspector!");
            return;
        }
        
        // Create from scratch - NO PREFAB NEEDED!
        GameObject objUI = new GameObject($"Objective_{objective.objectiveType}");
        objUI.transform.SetParent(objectiveContainer, false);
        
        // Add RectTransform
        RectTransform rt = objUI.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 70); // Height only, width from parent's layout
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        
        // Add background
        Image bg = objUI.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Dark semi-transparent
        
        // Add Horizontal Layout
        HorizontalLayoutGroup layout = objUI.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(15, 15, 12, 12);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        
        // Create Description Text
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(objUI.transform, false);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = objective.GetDescription();
        descText.fontSize = 32;
        descText.color = Color.white;
        descText.alignment = TextAlignmentOptions.MidlineLeft;
        descText.textWrappingMode = TextWrappingModes.NoWrap; // FIXED: Updated from obsolete enableWordWrapping
        descText.overflowMode = TextOverflowModes.Overflow;
        
        LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
        descLayout.flexibleWidth = 1;
        descLayout.preferredHeight = 50;
        
        // Create Progress Text
        GameObject progObj = new GameObject("Progress");
        progObj.transform.SetParent(objUI.transform, false);
        TextMeshProUGUI progText = progObj.AddComponent<TextMeshProUGUI>();
        progText.text = "0/?";
        progText.fontSize = 36;
        progText.fontStyle = FontStyles.Bold;
        progText.color = new Color(1f, 0.92f, 0.016f); // Gold/Yellow
        progText.alignment = TextAlignmentOptions.MidlineRight;
        
        LayoutElement progLayout = progObj.AddComponent<LayoutElement>();
        progLayout.minWidth = 120;
        progLayout.preferredHeight = 50;
        
        // Create Checkmark (hidden initially)
        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(objUI.transform, false);
        Image checkImg = checkObj.AddComponent<Image>();
        checkImg.color = completedColor;
        RectTransform checkRT = checkObj.GetComponent<RectTransform>();
        checkRT.sizeDelta = new Vector2(40, 40);
        checkObj.SetActive(false);
        
        LayoutElement checkLayout = checkObj.AddComponent<LayoutElement>();
        checkLayout.minWidth = 40;
        checkLayout.preferredWidth = 40;
        
        // Store reference
        ObjectiveItem item = new ObjectiveItem {
            uiObject = objUI,
            objective = objective,
            descriptionText = descText,
            progressText = progText,
            checkmark = checkImg
        };
        
        activeObjectives.Add(item);
        
        Debug.Log($"[ObjectiveUI] ✓ Successfully created UI item for {objective.objectiveType}");
    }
    
    /// <summary>
    /// Update objective progress (call from Board.cs)
    /// </summary>
    public void UpdateObjectiveProgress(LevelObjectiveType type, int current, int target) {
        Debug.Log($"[ObjectiveUI] UpdateProgress: {type}, {current}/{target}");
        
        foreach (var item in activeObjectives) {
            if (item.objective.objectiveType == type && !item.isCompleted) {
                if (item.progressText != null) {
                    item.progressText.text = $"{current}/{target}";
                }
                
                // Check if completed
                if (current >= target) {
                    MarkObjectiveComplete(item);
                }
            }
        }
    }
    
    /// <summary>
    /// Update timer display (call from Board.cs)
    /// </summary>
    public void UpdateTimer(float timeRemaining) {
        if (timerText != null) {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            // Warning color when low
            if (timeRemaining < 10f) {
                timerText.color = timerWarningColor;
            }
        }
    }
    
    private void MarkObjectiveComplete(ObjectiveItem item) {
        item.isCompleted = true;
        
        Debug.Log($"[ObjectiveUI] Objective complete: {item.objective.objectiveType}");
        
        if (item.checkmark != null) {
            item.checkmark.gameObject.SetActive(true);
        }
        
        if (item.progressText != null) {
            item.progressText.text = "✓";
            item.progressText.color = completedColor;
        }
        
        if (item.descriptionText != null) {
            item.descriptionText.color = completedColor;
        }
    }
    
    /// <summary>
    /// Check if all objectives are complete
    /// </summary>
    public bool AreAllObjectivesComplete() {
        foreach (var item in activeObjectives) {
            if (!item.isCompleted) return false;
        }
        return activeObjectives.Count > 0; // At least one objective exists
    }
}
