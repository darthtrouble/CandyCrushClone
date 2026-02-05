using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
        StartCoroutine(SpawnObjectivesSequence(objectives));
        
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
    
    // Polish: Spawn items one by one
    private IEnumerator SpawnObjectivesSequence(List<LevelObjective> objectives) {
        foreach (var objective in objectives) {
            CreateObjectiveUI(objective);
            yield return new WaitForSeconds(0.1f);
        }
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
            }

            // Add Background
            Image img = objectivePanel.GetComponent<Image>();
            if (img == null) img = objectivePanel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); 
            img.raycastTarget = false; 

            // Add Layout Group to PANEL (Vertical: Title -> Timer -> Container)
            VerticalLayoutGroup vlg = objectivePanel.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = objectivePanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 15, 15); 
            vlg.spacing = 8;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Add Content Size Fitter because user wants it to resize
            ContentSizeFitter csf = objectivePanel.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = objectivePanel.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; 
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Set width constraint
            LayoutElement le = objectivePanel.GetComponent<LayoutElement>();
            if (le == null) le = objectivePanel.AddComponent<LayoutElement>();
            le.preferredWidth = 350; 
        } else {
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
            
            timerText.fontSize = 32; 
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.color = Color.white;
            timerText.fontStyle = FontStyles.Bold;
        }
    }

    private void CreateObjectiveUI(LevelObjective objective) {
        if (objectiveContainer == null) return;
        
        GameObject objUI = new GameObject($"Objective_{objective.objectiveType}");
        objUI.transform.SetParent(objectiveContainer, false);
        objUI.transform.localScale = Vector3.zero; // Start hidden for pop
        
        // Compact Design
        Image bg = objUI.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.3f); 
        bg.raycastTarget = false; 
        
        HorizontalLayoutGroup layout = objUI.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 5, 5); 
        layout.spacing = 10;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        
        LayoutElement selfLayout = objUI.AddComponent<LayoutElement>();
        selfLayout.minHeight = 50; 
        selfLayout.flexibleWidth = 1;
        
        // Description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(objUI.transform, false);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = objective.GetDescription();
        descText.fontSize = 40; 
        descText.color = Color.white;
        descText.alignment = TextAlignmentOptions.Left;
        descText.overflowMode = TextOverflowModes.Ellipsis;
        descText.textWrappingMode = TextWrappingModes.NoWrap;
        descText.raycastTarget = false; 
        
        LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
        descLayout.flexibleWidth = 1;
        
        // Progress
        GameObject progObj = new GameObject("Progress");
        progObj.transform.SetParent(objUI.transform, false);
        TextMeshProUGUI progText = progObj.AddComponent<TextMeshProUGUI>();
        
        int targetVal = (objective.objectiveType == LevelObjectiveType.Score) ? objective.targetScore : objective.targetAmount;
        if (objective.objectiveType == LevelObjectiveType.TimedChallenge) targetVal = (int)objective.timeLimit; 

        progText.text = (objective.objectiveType == LevelObjectiveType.TimedChallenge && objective.targetScore == 0) ? "" : $"0/{targetVal}";
        progText.fontSize = 42; 
        progText.fontStyle = FontStyles.Bold;
        progText.color = Color.yellow;
        progText.alignment = TextAlignmentOptions.Right;
        progText.raycastTarget = false; 
        
        LayoutElement progLayout = progObj.AddComponent<LayoutElement>();
        progLayout.minWidth = 70;
        
        // Checkmark
        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(objUI.transform, false);
        Image checkImg = checkObj.AddComponent<Image>();
        
        // FIX: Load a checkmark sprite, or fallback to a green box
        Sprite checkSprite = Resources.Load<Sprite>("Sprites/checkmark");
        if(checkSprite != null) {
            checkImg.sprite = checkSprite;
            checkImg.color = Color.white; 
            checkImg.preserveAspect = true; // Fix distortion
        } else {
            checkImg.color = completedColor; 
        }

        checkImg.raycastTarget = false; 
        checkObj.SetActive(false);
        
        LayoutElement checkLayout = checkObj.AddComponent<LayoutElement>();
        // Smaller size constraint (was min 30 unconstrained)
        checkLayout.minWidth = 25;
        checkLayout.minHeight = 25;
        checkLayout.preferredWidth = 25;
        checkLayout.preferredHeight = 25;
        
        activeObjectives.Add(new ObjectiveItem {
            uiObject = objUI,
            objective = objective,
            descriptionText = descText,
            progressText = progText,
            checkmark = checkImg
        });
        
        // Trigger Pop In
        StartCoroutine(PunchEffect(objUI.transform));
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
                // Optimize: Only update text if value CHANGED
                // We hijack 'lastCurrentVal' (which we need to add to class) or just compare strings?
                // Adding a quick check:
                string newText = $"{current}/{target}";
                if (item.progressText != null && item.progressText.text != newText) {
                     item.progressText.text = newText;
                     // Small pulse on update
                     if(item.uiObject != null && gameObject.activeInHierarchy) StartCoroutine(PunchEffect(item.uiObject.transform, 1.05f));
                }
                
                if (current >= target) MarkObjectiveComplete(item);
            }
        }
    }
    
    private int lastSecondsTimer = -1;

    public void UpdateTimer(float timeRemaining) {
        if (timerText != null) {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            
            if (seconds != lastSecondsTimer) {
                lastSecondsTimer = seconds;
                timerText.text = $"{minutes:00}:{seconds:00}";
                if (timeRemaining < 10f) {
                     timerText.color = timerWarningColor;
                     // Heartbeat effect
                     StartCoroutine(PunchEffect(timerText.transform, 1.2f));
                }
            }
        }
    }
    
    private void MarkObjectiveComplete(ObjectiveItem item) {
        if(item.isCompleted) return;
        
        item.isCompleted = true;
        
        // Show text green instead of hiding it
        if (item.progressText != null) {
            item.progressText.color = completedColor; 
        }

        // Show and animate checkmark
        if (item.checkmark != null) {
            item.checkmark.gameObject.SetActive(true);
            item.checkmark.transform.localScale = Vector3.zero;
            StartCoroutine(PunchEffect(item.checkmark.transform, 1.5f));
        }
    }
    
    // JUICE: Elastic Scale Effect
    private IEnumerator PunchEffect(Transform target, float scale = 1.1f) {
        Vector3 original = Vector3.one;
        if(target == null) yield break;

        float elapsed = 0f;
        float duration = 0.3f;
        
        while(elapsed < duration) {
            if(target == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Elastic Punch
            // Sin wave dampening
            float s = Mathf.Sin(t * Mathf.PI) * (scale - 1f); 
            target.localScale = original + (original * s);
            
            yield return null;
        }
        if(target != null) target.localScale = original;
    }
}
