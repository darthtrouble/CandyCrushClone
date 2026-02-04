using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays current level objectives on the game screen
/// Updates in real-time as player progresses
/// </summary>
public class ObjectiveUI : MonoBehaviour {
    
    [Header("UI References")]
    public GameObject objectivePanel;
    public Transform objectiveContainer; // Parent for objective items (needs Vertical Layout Group)
    
    [Header("Optional Specific Elements")]
    public TextMeshProUGUI timerText; // For timed challenges
    public GameObject objectivePrefab; // Legacy support (not used if null)
    
    [Header("Layout Settings")]
    public float verticalOffset = -50f; // Move down by default
    
    [Header("Colors")]
    public Color completedColor = Color.green;
    public Color timerWarningColor = Color.red;
    
    private List<ObjectiveItem> activeObjectives = new List<ObjectiveItem>();
    
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
        if (objectivePanel != null) {
            objectivePanel.SetActive(true);
            
            // Apply vertical offset
            RectTransform rt = objectivePanel.GetComponent<RectTransform>();
            if (rt != null) {
                rt.anchoredPosition += new Vector2(0, verticalOffset);
            }
        }
        Debug.Log($"[ObjectiveUI] Started on {gameObject.name}. Panel active: {(objectivePanel != null)}");
    }
    
    /// <summary>
    /// Initialize objectives from level data
    /// </summary>
    public void SetupObjectives(List<LevelObjective> objectives) {
        // AUTO-FIX: Create UI if missing
        if (objectivePanel == null || objectiveContainer == null) {
            Debug.LogWarning("[ObjectiveUI] UI References missing! Attempting auto-creation...");
            AutoCreateUI();
        }

        Debug.Log($"[ObjectiveUI] SetupObjectives called! Count: {objectives.Count}");

        // Clear existing
        foreach (var item in activeObjectives) {
            if (item.uiObject != null) Destroy(item.uiObject);
        }
        activeObjectives.Clear();
        
        // Create UI for each objective
        foreach (var objective in objectives) {
            CreateObjectiveUI(objective);
        }
        
        // Show/hide timer based on objectives
        if (timerText != null) {
            bool hasTimed = objectives.Exists(o => o.objectiveType == LevelObjectiveType.TimedChallenge);
            timerText.gameObject.SetActive(true); // Always active in layout, just change text if needed
            if (!hasTimed) timerText.text = ""; // Hide text if not needed
            timerText.transform.SetAsFirstSibling(); // Ensure at top
        }

        // Ensure Title is at top (if it exists)
        Transform titleTr = objectivePanel.transform.Find("Title");
        if (titleTr != null) titleTr.SetAsFirstSibling();
    }

    private void AutoCreateUI() {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // 0. Setup Safe Area
        Transform safeAreaTr = canvas.transform.Find("SafeAreaContainer");
        if (safeAreaTr == null) {
             GameObject safeAreaObj = new GameObject("SafeAreaContainer");
             safeAreaObj.transform.SetParent(canvas.transform, false);
             safeAreaTr = safeAreaObj.transform;
             
             // Full stretch
             RectTransform rt = safeAreaObj.AddComponent<RectTransform>();
             rt.anchorMin = Vector2.zero;
             rt.anchorMax = Vector2.one;
             rt.sizeDelta = Vector2.zero;
             
             // Add SafeArea script if it exists in project
             // We use reflection/component check to be safe
             // (Assuming user has a SafeArea script based on file list)
             safeAreaObj.AddComponent<SafeArea>();
        }

        // 1. Setup Panel
        if (objectivePanel == null) {
            Transform existingPanel = safeAreaTr.Find("ObjectivePanel");
            if (existingPanel != null) {
                objectivePanel = existingPanel.gameObject;
            } else {
                GameObject panelObj = new GameObject("ObjectivePanel");
                panelObj.transform.SetParent(safeAreaTr, false); // Parent to SafeArea
                objectivePanel = panelObj;
                
                RectTransform rt = panelObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1); // Top Left
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(20, -20);
                // Size handled by Fitter
            }

            // Add Background
            Image img = objectivePanel.GetComponent<Image>();
            if (img == null) img = objectivePanel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent
            img.raycastTarget = false; // OPTIMIZATION: Panel background needs no input

            // Add Layout Group to PANEL (Vertical: Title -> Timer -> Container)
            VerticalLayoutGroup vlg = objectivePanel.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = objectivePanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 15, 15); // Increased padding
            vlg.spacing = 8;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Add Content Size Fitter because user wants it to resize
            ContentSizeFitter csf = objectivePanel.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = objectivePanel.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; // Fixed width set by LayoutElement or rect
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Set width constraint
            LayoutElement le = objectivePanel.GetComponent<LayoutElement>();
            if (le == null) le = objectivePanel.AddComponent<LayoutElement>();
            le.preferredWidth = 350; // Wider for larger text
        } else {
            // Reparent if needed
             if (objectivePanel.transform.parent != safeAreaTr) {
                 objectivePanel.transform.SetParent(safeAreaTr, true);
             }
        }

        // 2. Setup Container (for the list items)
        if (objectiveContainer == null) {
             Transform existingContainer = objectivePanel.transform.Find("ObjectiveContainer");
             if (existingContainer != null) objectiveContainer = existingContainer;
             else {
                 GameObject containerObj = new GameObject("ObjectiveContainer");
                 containerObj.transform.SetParent(objectivePanel.transform, false);
                 objectiveContainer = containerObj.transform;
                 
                 VerticalLayoutGroup cvlg = containerObj.AddComponent<VerticalLayoutGroup>();
                 cvlg.spacing = 5;
                 cvlg.childControlWidth = true;
                 cvlg.childControlHeight = true;
                 cvlg.childForceExpandHeight = false;
             }
        }

        // 3. Setup Timer Text
        if (timerText == null) {
            Transform t = objectivePanel.transform.Find("TimerText");
            if (t == null) {
                 GameObject tObj = new GameObject("TimerText");
                 tObj.transform.SetParent(objectivePanel.transform, false);
                 t = tObj.transform;
            }
            timerText = t.GetComponent<TextMeshProUGUI>();
            if (timerText == null) timerText = t.gameObject.AddComponent<TextMeshProUGUI>();
            
            timerText.fontSize = 32; // Larger
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.color = Color.white;
            timerText.fontStyle = FontStyles.Bold;
        }

        // 4. FIX OTHER UI POSITIONS (Score & Moves)
        // Manual setup required via Scene now.
    }

    // Auto-creation and positioning removed per user request to use Scene/Inspector.
    // Ensure you set up the UI in the scene:
    // 1. ObjectivePanel under Canvas
    // 2. ScoreText and MovesText in Top-Right
    // 3. Fonts set to 40+


    
    private void CreateObjectiveUI(LevelObjective objective) {
        if (objectiveContainer == null) return;
        
        GameObject objUI = new GameObject($"Objective_{objective.objectiveType}");
        objUI.transform.SetParent(objectiveContainer, false);
        
        // Compact Design
        Image bg = objUI.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.3f); // Weaker background
        bg.raycastTarget = false; // OPTIMIZATION: Background needs no input
        
        HorizontalLayoutGroup layout = objUI.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 5, 5); // Compact padding
        layout.spacing = 10;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        
        // Layout Element for height control
        LayoutElement selfLayout = objUI.AddComponent<LayoutElement>();
        selfLayout.minHeight = 50; // Much smaller (was 80)
        selfLayout.flexibleWidth = 1;
        
        // Description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(objUI.transform, false);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = objective.GetDescription();
        descText.fontSize = 40; // Larger font for mobile
        descText.color = Color.white;
        descText.alignment = TextAlignmentOptions.Left;
        descText.overflowMode = TextOverflowModes.Ellipsis;
        descText.textWrappingMode = TextWrappingModes.NoWrap;
        descText.raycastTarget = false; // OPTIMIZATION: Text needs no input
        
        LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
        descLayout.flexibleWidth = 1;
        
        // Progress
        GameObject progObj = new GameObject("Progress");
        progObj.transform.SetParent(objUI.transform, false);
        TextMeshProUGUI progText = progObj.AddComponent<TextMeshProUGUI>();
        
        int targetVal = (objective.objectiveType == LevelObjectiveType.Score) ? objective.targetScore : objective.targetAmount;
        if (objective.objectiveType == LevelObjectiveType.TimedChallenge) targetVal = (int)objective.timeLimit; // Just visual placeholder really

        progText.text = (objective.objectiveType == LevelObjectiveType.TimedChallenge && objective.targetScore == 0) ? "" : $"0/{targetVal}";
        progText.fontSize = 42; // Even larger for numbers
        progText.fontStyle = FontStyles.Bold;
        progText.color = Color.yellow;
        progText.alignment = TextAlignmentOptions.Right;
        progText.raycastTarget = false; // OPTIMIZATION
        
        LayoutElement progLayout = progObj.AddComponent<LayoutElement>();
        progLayout.minWidth = 70;
        
        // Checkmark
        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(objUI.transform, false);
        Image checkImg = checkObj.AddComponent<Image>();
        checkImg.color = completedColor;
        checkImg.raycastTarget = false; // OPTIMIZATION
        checkObj.SetActive(false);
        
        LayoutElement checkLayout = checkObj.AddComponent<LayoutElement>();
        checkLayout.minWidth = 30;
        checkLayout.minHeight = 30;
        
        activeObjectives.Add(new ObjectiveItem {
            uiObject = objUI,
            objective = objective,
            descriptionText = descText,
            progressText = progText,
            checkmark = checkImg
        });
    }
    
    public void UpdateObjectiveProgress(LevelObjectiveType type, int current, int target, string animalTag = null) {
        foreach (var item in activeObjectives) {
            bool typeMatch = item.objective.objectiveType == type;
            bool tagMatch = true;

            // If it is a collection objective, we MUST check the tag
            if (type == LevelObjectiveType.CollectAnimals && !string.IsNullOrEmpty(animalTag)) {
                 tagMatch = (item.objective.animalTag == animalTag);
            }

            if (typeMatch && tagMatch && !item.isCompleted) {
                if (item.progressText != null) item.progressText.text = $"{current}/{target}";
                if (current >= target) MarkObjectiveComplete(item);
            }
        }
    }
    
    private int lastSecondsTimer = -1;

    public void UpdateTimer(float timeRemaining) {
        if (timerText != null) {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            
            // OPTIMIZATION: Only update text if the value actually changed
            if (seconds != lastSecondsTimer) {
                lastSecondsTimer = seconds;
                timerText.text = $"{minutes:00}:{seconds:00}";
                if (timeRemaining < 10f) timerText.color = timerWarningColor;
            }
        }
    }
    
    private void MarkObjectiveComplete(ObjectiveItem item) {
        item.isCompleted = true;
        if (item.checkmark != null) item.checkmark.gameObject.SetActive(true);
        if (item.progressText != null) {
            item.progressText.text = "";
            item.progressText.gameObject.SetActive(false);
        }
    }
}
