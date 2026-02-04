# Troubleshooting Prefab & Timer Issues 🔧

## Problem Summary
You're seeing `SerializedObjectNotCreatableException` errors and having issues with:
- Objective prefab not working
- Timer not displaying

---

## Fix 1: Clear the Prefab Errors

These errors happen when Unity Inspector tries to display a null/deleted object. Here's how to fix:

### Step 1: Clean Up
1. **Close Unity completely** and reopen it
2. In Hierarchy, delete the `ObjectivePanel` if it exists
3. In Project, delete any prefabs you created (we'll remake them)

### Step 2: Verify Scripts Compile
1. Open Console (Window → General → Console)
2. Check for any script errors
3. If you see "ObjectiveItem not found", that's OK - we'll add it next

---

## Fix 2: Create Objective Prefab (Step-by-Step)

Let's create a simple, working prefab:

### Create the Prefab Item

1. **In Hierarchy**, right-click on Canvas → UI → **Panel**
2. Rename to `ObjectiveItemPrefab`
3. Set **Width: 250**, **Height: 40**

### Add Text Components

**Inside ObjectiveItemPrefab:**

**A) Description Text**
1. Right-click `ObjectiveItemPrefab` → UI → **Text - TextMeshPro**
2. Rename to **`Description`** (exact name!)
3. Settings:
   - Text: "Collect Red Animals"
   - Font Size: 16
   - Color: White
   - Alignment: Left, Middle
   - **RectTransform**:
     - Anchor: Left-Middle
     - Position: (10, 0)
     - Width: 150, Height: 30

**B) Progress Text**
1. Right-click `ObjectiveItemPrefab` → UI → **Text - TextMeshPro**
2. Rename to **`Progress`** (exact name!)
3. Settings:
   - Text: "0/20"
   - Font Size: 16, Bold
   - Color: Yellow (#FFEB3B)
   - Alignment: Right, Middle
   - **RectTransform**:
     - Anchor: Right-Middle
     - Position: (-10, 0)
     - Width: 70, Height: 30

**C) Checkmark (Optional)**
1. Right-click `ObjectiveItemPrefab` → UI → **Image**
2. Rename to **`Checkmark`** (exact name!)
3. Settings:
   - Color: Green
   - Width: 20, Height: 20
   - **Disable it** (uncheck in Inspector)
   - Position: Right side near Progress

### Save as Prefab

1. **In Project panel**, create folder: `Assets/Prefabs` (if it doesn't exist)
2. **Drag** `ObjectiveItemPrefab` from Hierarchy → into `Assets/Prefabs` folder
3. You should see a blue cube icon appear
4. **Delete** `ObjectiveItemPrefab` from Hierarchy (important!)

---

## Fix 3: Setup ObjectivePanel Properly

Now create the container:

### Create Panel
1. Right-click Canvas → UI → **Panel**
2. Rename to `ObjectivePanel`
3. **RectTransform**:
   - Anchor Preset: **Top-Left**
   - Position X: 150, Y: -50
   - Width: 280, Height: 200

### Add ObjectiveUI Script
1. Select `ObjectivePanel`
2. Add Component → **ObjectiveUI**
3. **Don't assign anything yet**

### Create Container
1. Right-click `ObjectivePanel` → **Create Empty**
2. Rename to `ObjectiveContainer`
3. Add Component → **Vertical Layout Group**:
   - Padding: Top=10, Left=10, Right=10, Bottom=10
   - Spacing: 8
   - Child Alignment: Upper Left
   - ✓ Child Controls Size (Width & Height)
   - ✓ Child Force Expand (Width only)
4. Add Component → **Content Size Fitter**:
   - Vertical Fit: **Preferred Size**

### Create Timer (Optional)
1. Right-click `ObjectivePanel` → UI → **Text - TextMeshPro**
2. Rename to `TimerText`
3. Settings:
   - Text: "1:00"
   - Font Size: 28, Bold
   - Color: White
   - Alignment: Center
   - Position at top of panel

### Link Everything
Select `ObjectivePanel`, in ObjectiveUI component:
- **Objective Panel**: Drag `ObjectivePanel` itself
- **Objective Container**: Drag `ObjectiveContainer`
- **Objective Prefab**: Drag prefab from **Assets/Prefabs** folder
- **Timer Text**: Drag `TimerText`

---

## Fix 4: Test with Simple Level

Create a test level to verify it works:

### Create Level Data
1. Project → Right-click → Create → **Level Data**
2. Name: `Level_Test`
3. Settings:
   - Width: 6, Height: 8
   - Moves: 15
   - One Star: 500
   - Two Star: 800
   - Three Star: 1200

### Add Objective
1. Under **Objectives**, click **+**
2. Set:
   - **Objective Type**: Score
   - **Target Score**: 500

### Assign to Board
1. Find `Board` object in scene
2. Under **Levels** array, set Size: 1
3. Drag `Level_Test` into Element 0
4. **Save scene**

---

## Verify It Works

### Quick Test
1. **Play the scene**
2. You should see:
   - ✅ Board positioned lower (not overlapping UI)
   - ✅ ObjectivePanel at top-left
   - ✅ One objective showing: "Reach Score: 0/500"
   - ✅ Progress updates as you match tiles

### If Timer Doesn't Show
That's normal! Timer only shows for **Timed Challenge** objectives:
1. Change objective type to `TimedChallenge`
2. Set Time Limit: 60
3. Play again - timer appears!

---

## Common Issues & Solutions

**"Prefab is null"**
- Make sure you dragged the **blue prefab icon** from Project, not from Hierarchy
- Verify child names are exactly: `Description`, `Progress`, `Checkmark`

**"Nothing shows up"**
- Check ObjectivePanel is **Active** (checkbox in Inspector)
- Verify Board's level data has objectives added
- Check Console for errors

**"Text is cut off"**
- Increase ObjectivePanel width (try 300)
- Check Vertical Layout Group spacing

**"Board still overlaps"**
- Increase `verticalOffset` in Board.cs (try -3.0f or -4.0f)
- Or move Camera up (increase Y position)

---

## Quick Checklist ✓

- [ ] Prefab has children named: Description, Progress, Checkmark
- [ ] Prefab is in Assets/Prefabs and is BLUE icon
- [ ] ObjectiveUI component has all 4 fields assigned
- [ ] Level data has at least 1 objective
- [ ] Board references the level data
- [ ] Unity Console shows no errors

**Follow these steps exactly and it should work!** If you still have issues, let me know which step fails.
