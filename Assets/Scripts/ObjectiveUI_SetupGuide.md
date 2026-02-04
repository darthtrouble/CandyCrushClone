# Objective UI Setup Guide 🎯

## Quick Overview
The `ObjectiveUI` system displays your level goals in real-time during gameplay. It shows:
- 📋 Each objective with a description
- 📊 Current progress (e.g., "15/20 Red Animals")
- ✅ Checkmark when completed
- ⏱️ Timer for timed challenges

---

## Unity Hierarchy Structure

Create this UI structure in your **GameLevel scene**:

```
Canvas
└── ObjectivePanel (GameObject)
    ├── ObjectiveUI (Script Component)
    ├── Background (Image - optional)
    ├── Title (TextMeshProUGUI - "Objectives")
    ├── ObjectiveContainer (Vertical Layout Group)
    │   └── (Objective items spawn here dynamically)
    └── TimerText (TextMeshProUGUI - optional)
```

---

## Step-by-Step Setup

### 1. Create ObjectivePanel
1. Right-click on your **Canvas** → UI → Panel
2. Rename to `ObjectivePanel`
3. Position it (recommended: top-left corner)
4. **RectTransform settings**:
   - Anchor: Top-Left
   - Position: X=150, Y=-50
   - Width: 280, Height: auto (use vertical layout)

### 2. Add ObjectiveUI Component
1. Select `ObjectivePanel`
2. Add Component → `ObjectiveUI`
3. You'll see these fields in Inspector (we'll fill them next)

### 3. Create Title (Optional)
1. Right-click `ObjectivePanel` → UI → Text - TextMeshPro
2. Rename to `Title`
3. Text: "OBJECTIVES"
4. Font Size: 24, Bold, Color: White

### 4. Create ObjectiveContainer
1. Right-click `ObjectivePanel` → Create Empty
2. Rename to `ObjectiveContainer`
3. Add Component → **Vertical Layout Group**:
   - Child Alignment: Upper Left
   - Child Controls Size: Width ✓, Height ✓
   - Child Force Expand: Width ✓, Height ✗
   - Spacing: 8
4. Add Component → **Content Size Fitter**:
   - Vertical Fit: Preferred Size

### 5. Create ObjectivePrefab
This is the template for each objective item:

1. Right-click `ObjectiveContainer` → UI → Panel
2. Rename to `ObjectivePrefab`
3. Add **Horizontal Layout Group**:
   - Padding: 10 (all sides)
   - Spacing: 10
   - Child Alignment: Middle Left
   - Child Controls Size: Width ✓, Height ✓

**Inside ObjectivePrefab, create:**

#### a) Checkmark (Image)
- Right-click `ObjectivePrefab` → UI → Image
- Name: `Checkmark`
- Width: 24, Height: 24
- Source Image: Use a checkmark sprite
- Color: Green (#00FF00)
- **Initial State**: Disabled (unchecked in Inspector)

#### b) Description (TextMeshProUGUI)
- Right-click `ObjectivePrefab` → UI → Text - TextMeshPro
- Name: `DescriptionText`
- Text: "Collect 20 Red Animals" (example)
- Font Size: 18
- Color: White
- Horizontal Overflow: Wrap
- **Layout Element** (Add Component):
  - Flexible Width: 1

#### c) Progress (TextMeshProUGUI)
- Right-click `ObjectivePrefab` → UI → Text - TextMeshPro
- Name: `ProgressText`
- Text: "0/20"
- Font Size: 18, Bold
- Color: Yellow (#FFEB3B)
- Alignment: Right

**Now drag ObjectivePrefab to your Project folder to save it as a prefab, then DELETE it from the hierarchy** (it will be spawned dynamically).

### 6. Create TimerText (For Timed Challenges)
1. Right-click `ObjectivePanel` → UI → Text - TextMeshPro
2. Rename to `TimerText`
3. Text: "1:00"
4. Font Size: 32, Bold
5. Color: White (turns red when < 10s)
6. Alignment: Center
7. Position at top of panel

### 7. Assign References in ObjectiveUI Component
Select `ObjectivePanel` and fill in the Inspector:

- **Objective Panel**: Drag `ObjectivePanel` itself
- **Objective Container**: Drag `ObjectiveContainer`
- **Objective Prefab**: Drag the prefab from your Project folder
- **Timer Text**: Drag `TimerText`
- **Completed Color**: Green (#00FF00)
- **Timer Warning Color**: Red (#FF0000)

---

## Visual Example

```
┌─────────────────────────────────┐
│         OBJECTIVES              │
├─────────────────────────────────┤
│ ✓ Reach 1000 Points    ✓        │
│ ○ Collect Red: 15/20            │
│ ○ Clear Ice: 3/5                │
├─────────────────────────────────┤
│         Timer: 0:45             │
└─────────────────────────────────┘
```

---

## Styling Tips (Optional)

### Panel Background
- Add a semi-transparent dark background
- Color: Black with Alpha = 0.7
- Add subtle border using Outline component

### Animations
Add these to ObjectiveUI.cs for polish:
- Fade in on level start
- Scale checkmark when objective completes
- Flash timer when < 10 seconds

### Icons
Instead of text, use animal sprites:
- Replace "Collect Red" with a red animal icon
- Replace "Clear Ice" with an ice cube icon

---

## Testing

1. **Create a Test Level**:
   - Create a `LevelData` ScriptableObject
   - Add objectives (Score, Collect Animals, etc.)
   - Assign to Board's `levels` array

2. **Play Mode**:
   - Objectives should appear on ObjectivePanel
   - Progress should update as you play
   - Checkmarks appear when complete

3. **Common Issues**:
   - **Nothing appears**: Check Board finds ObjectiveUI via `FindFirstObjectByType`
   - **Text overlaps**: Adjust Horizontal Layout Group spacing
   - **Prefab doesn't spawn**: Ensure prefab is assigned in Inspector

---

## Quick Setup (Copy-Paste Values)

If you want to speed through setup, use these exact values:

**ObjectivePanel**:
- Anchor: Top-Left
- Pos: (150, -50, 0)
- Size: (280, 200)

**ObjectivePrefab** (before making prefab):
- Height: 40
- Horizontal Layout: Padding 10, Spacing 10

**Checkmark**:
- Size: 24x24, Green

**DescriptionText**:
- Font: 18, White
- Flexible Width: 1

**ProgressText**:
- Font: 18 Bold, Yellow

---

## Done! 🎉

Once set up, the system works automatically:
- Board initializes objectives
- ObjectiveUI displays them
- Progress updates in real-time
- Checkmarks appear when complete

No code changes needed - it's all data-driven through your LevelData ScriptableObjects!
