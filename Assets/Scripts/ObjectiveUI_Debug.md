# Objective Prefab - Ultra Simple Debug Guide 🔍

## The Problem

The objective items aren't spawning in the game. Let's debug this step by step.

---

## Debug Step 1: Check if ObjectiveUI is Running

Add these debug logs to see what's happening:

1. Open `ObjectiveUI.cs`
2. Find the `SetupObjectives` method (around line 41)
3. Add this at the start:

```csharp
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
    // ... rest of method
}
```

**Play the game and check the Console.** What do you see?

---

## Possible Issues & Fixes

### Issue 1: SetupObjectives Never Called

**Symptoms:** No debug logs appear at all

**Cause:** Board isn't finding ObjectiveUI or level has no objectives

**Fix:**
1. Check Board's `Awake()` - does it find ObjectiveUI?
2. Check your LevelData - does it have objectives added?
3. Add this to Board.cs in `InitializeObjectives()`:

```csharp
private void InitializeObjectives() {
    Debug.Log($"[Board] InitializeObjectives called");
    Debug.Log($"[Board] currentLevelData is null? {currentLevelData == null}");
    Debug.Log($"[Board] objectiveUI is null? {objectiveUI == null}");
    
    if (currentLevelData == null || currentLevelData.objectives.Count == 0) {
        Debug.LogWarning("[Board] No level data or no objectives!");
        return;
    }
    // ... rest
}
```

### Issue 2: Prefab is Null

**Symptoms:** Log says "Creating UI for..." but nothing appears

**Cause:** Prefab not assigned or broken

**Fix:** Let's use a RUNTIME-CREATED item instead of a prefab!

---

## Solution: Skip the Prefab! (Runtime Creation)

Let me give you code that creates objectives **without needing a prefab**:

### Replace CreateObjectiveUI Method

Open `ObjectiveUI.cs` and replace the `CreateObjectiveUI` method with this:

```csharp
private void CreateObjectiveUI(LevelObjective objective) {
    Debug.Log($"[ObjectiveUI] CreateObjectiveUI called for {objective.objectiveType}");
    
    if (objectiveContainer == null) {
        Debug.LogError("[ObjectiveUI] objectiveContainer is NULL!");
        return;
    }
    
    // Create from scratch - NO PREFAB NEEDED!
    GameObject objUI = new GameObject("ObjectiveItem");
    objUI.transform.SetParent(objectiveContainer, false);
    
    // Add RectTransform
    RectTransform rt = objUI.AddComponent<RectTransform>();
    rt.sizeDelta = new Vector2(0, 60); // Height only, width from layout
    
    // Add background (optional)
    Image bg = objUI.AddComponent<Image>();
    bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark semi-transparent
    
    // Add Horizontal Layout
    HorizontalLayoutGroup layout = objUI.AddComponent<HorizontalLayoutGroup>();
    layout.padding = new RectOffset(15, 15, 10, 10);
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
    descText.fontSize = 28;
    descText.color = Color.white;
    descText.alignment = TextAlignmentOptions.Left;
    
    LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
    descLayout.flexibleWidth = 1;
    
    // Create Progress Text
    GameObject progObj = new GameObject("Progress");
    progObj.transform.SetParent(objUI.transform, false);
    TextMeshProUGUI progText = progObj.AddComponent<TextMeshProUGUI>();
    progText.text = "0/?";
    progText.fontSize = 32;
    progText.fontStyle = FontStyles.Bold;
    progText.color = Color.yellow;
    progText.alignment = TextAlignmentOptions.Right;
    
    LayoutElement progLayout = progObj.AddComponent<LayoutElement>();
    progLayout.minWidth = 100;
    
    // Create Checkmark (hidden initially)
    GameObject checkObj = new GameObject("Checkmark");
    checkObj.transform.SetParent(objUI.transform, false);
    Image checkImg = checkObj.AddComponent<Image>();
    checkImg.color = Color.green;
    RectTransform checkRT = checkObj.GetComponent<RectTransform>();
    checkRT.sizeDelta = new Vector2(30, 30);
    checkObj.SetActive(false);
    
    // Store reference
    ObjectiveItem item = new ObjectiveItem {
        uiObject = objUI,
        objective = objective,
        descriptionText = descText,
        progressText = progText,
        checkmark = checkImg
    };
    
    activeObjectives.Add(item);
    
    Debug.Log($"[ObjectiveUI] Successfully created UI item for {objective.objectiveType}");
}
```

**Benefits:**
- ✅ No prefab required!
- ✅ Creates everything at runtime
- ✅ Works immediately
- ✅ Easy to debug

---

## Test It

1. **Apply the above code**
2. **Play the game**
3. Check Console for debug logs
4. Objectives should appear!

---

## If Still Not Working

Tell me what you see in Console:
- [ ] "SetupObjectives called! Count: X" - what's the count?
- [ ] "Creating UI for: ..." - does it print?
- [ ] "objectiveContainer is NULL!" - error?
- [ ] "Successfully created UI item" - does it reach here?

Also send me a screenshot of:
1. Your ObjectivePanel in Inspector (show ObjectiveUI component)
2. Your LevelData in Inspector (show objectives list)
3. Console output when you play

This will help me pinpoint exactly what's breaking! 🔍
