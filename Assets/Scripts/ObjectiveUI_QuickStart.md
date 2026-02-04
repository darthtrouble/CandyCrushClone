# Quick Start: Objective Display in 5 Minutes ⚡

## What You'll Get
A clean, animated objective display showing:
- Current goals (e.g., "Collect 20 Red Animals")
- Live progress updates (e.g., "15/20")
- Green checkmarks when complete ✓
- Timer for timed challenges ⏱️

---

## Fast Setup (Minimal Steps)

### 1. Create ObjectivePanel (30 seconds)
1. In your **GameLevel scene**, find the Canvas
2. Right-click Canvas → **UI → Panel**
3. Rename to `ObjectivePanel`
4. Move to **top-left corner** of screen
5. Add Component → `ObjectiveUI` script

### 2. Create ObjectiveContainer (30 seconds)
1. Right-click `ObjectivePanel` → **Create Empty**
2. Rename to `ObjectiveContainer`
3. Add Component → **Vertical Layout Group**
4. Add Component → **Content Size Fitter**
   - Set Vertical Fit: **Preferred Size**

### 3. Create ObjectivePrefab (2 minutes)

**Option A: Quick & Dirty**
1. Right-click `ObjectiveContainer` → UI → **Panel**
2. Name it `ObjectiveItem`
3. Add 2 text objects inside:
   - `Description` (left side) - "Collect Red: "
   - `Progress` (right side) - "0/20"
4. Add `ObjectiveItem` component to it
5. **Drag to Project folder** to make prefab
6. **Delete from hierarchy**

**Option B: Polished (recommended)**
Follow the detailed guide in `ObjectiveUI_SetupGuide.md` for styled version.

### 4. Create Timer (30 seconds) - Optional
1. Right-click `ObjectivePanel` → UI → **Text - TextMeshPro**
2. Rename to `TimerText`
3. Text: "1:00"
4. Font Size: 32
5. Position at top of panel

### 5. Link Everything (30 seconds)
Select `ObjectivePanel`, in the ObjectiveUI component:
- **Objective Panel**: Drag `ObjectivePanel`
- **Objective Container**: Drag `ObjectiveContainer`
- **Objective Prefab**: Drag your prefab from Project
- **Timer Text**: Drag `TimerText`

---

## Test It!

### Create a Simple Test Level
1. Project → Right-click → Create → **Level Data**
2. Name it `TestLevel`
3. In Inspector:
   - **Width**: 6, **Height**: 8
   - **Moves**: 20
   - **One Star Score**: 500
   - **Two Star Score**: 800
   - **Three Star Score**: 1200
4. Click **+** under Objectives
5. Set:
   - **Objective Type**: Score
   - **Target Score**: 500

### Assign to Board
1. Find your `Board` object in scene
2. Drag `TestLevel` into the **Levels** array
3. **Play!**

You should see:
```
┌─────────────────┐
│  OBJECTIVES     │
├─────────────────┤
│ Reach Score     │
│ 0/500          │
└─────────────────┘
```

As you play, the progress updates automatically!

---

## Common Issues

**"Nothing shows up"**
- Check Board has ObjectiveUI assigned (should auto-find)
- Make sure ObjectivePanel is **Active** (checked in Inspector)
- Verify prefab is assigned in ObjectiveUI component

**"Progress doesn't update"**
- Make sure your LevelData has objectives added
- Check Board's `currentLevelIndex` matches your test level

**"Text is cut off"**
- Increase ObjectivePanel width (recommended: 280-300)
- Enable horizontal overflow on text components

---

## Styling Ideas 🎨

Quick copy-paste values for a polished look:

**ObjectivePanel**:
- Background Color: Black, Alpha = 0.8
- Add Component → **Outline**: Width 2, Color White

**Text Colors**:
- Description: White #FFFFFF
- Progress (incomplete): Yellow #FFEB3B
- Progress (complete): Green #00FF00

**Animations** (already built into ObjectiveItem.cs):
- Checkmark punch animation ✓
- Color transitions
- Progress bar (coming soon)

---

## What's Next?

Once this is working, you can:
1. Add **icons** for each animal type
2. Create **fancy prefabs** with backgrounds and borders
3. Add **sound effects** when objectives complete
4. Implement **progress bars** instead of text

Check out `ObjectiveUI_SetupGuide.md` for detailed customization options!

---

**You're all set!** The system works automatically - just create LevelData with different objectives and watch them display beautifully! 🎉
