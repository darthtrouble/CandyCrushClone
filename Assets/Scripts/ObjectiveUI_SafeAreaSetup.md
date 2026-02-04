# Objective UI Setup - Optimized for Your Project 📱

## Understanding Your Setup

Your game uses:
- ✅ **SafeArea system** - Adapts to notches/cutouts on different devices
- ✅ **Canvas-based UI** - All UI is in screen space
- ✅ **World space board** - Game board exists in world coordinates

**The Solution**: Put ObjectiveUI inside the SafeArea so it automatically adjusts for all devices!

---

## Quick Setup (Works with SafeArea)

### 1. Find Your Canvas & SafeArea

Your Canvas probably has a child GameObject with the `SafeArea` component. This is where ALL your UI should live.

**Hierarchy should look like:**
```
Canvas
└── SafeAreaPanel (has SafeArea.cs component)
    ├── ScoreText (existing)
    ├── MovesText (existing)
    ├── PauseButton (existing?)
    └── ObjectivePanel (NEW - we'll create this)
```

### 2. Create ObjectivePanel

1. Select your **SafeAreaPanel** (the one with SafeArea component)
2. Right-click → UI → **Panel**
3. Rename to `ObjectivePanel`

**RectTransform Settings:**
- **Anchor Preset**: Top-Left (hold Alt+Shift, click top-left)
- **Pivot**: (0, 1) - top-left
- **Position**: X=20, Y=-20 (small offset from corner)
- **Width**: 400-500 (adjust to your preference)
- **Height**: Leave auto (will expand with content)

**Visual Settings:**
- Background Image: Keep or set to transparent
- Color: Black with Alpha ~0.7 for semi-transparent

### 3. Add ObjectiveUI Component

1. Select `ObjectivePanel`
2. Add Component → **ObjectiveUI** script
3. Don't assign fields yet

### 4. Create Container for Objectives

1. Right-click `ObjectivePanel` → **Create Empty**
2. Rename to `ObjectiveContainer`
3. **Add Component → Vertical Layout Group:**
   - Padding: 15 (all sides)
   - Spacing: 12
   - Child Alignment: Upper Left
   - ✓ Control Child Size: Width & Height
   - ✓ Child Force Expand: Width only
4. **Add Component → Content Size Fitter:**
   - Vertical Fit: **Preferred Size**

### 5. Create Title (Optional but Recommended)

1. Right-click `ObjectivePanel` → UI → **Text - TextMeshPro**
2. Rename to `ObjectivesTitle`
3. **Settings:**
   - Text: "OBJECTIVES"
   - Font Size: 36-42
   - Font Style: Bold
   - Color: White or Yellow
   - Alignment: Center
   - **RectTransform**:
     - Anchor: Top-Stretch
     - Height: 60
     - Position Y: -10

### 6. Create Objective Item Prefab

**In Hierarchy** (we'll make this then convert to prefab):

1. Right-click `ObjectiveContainer` → UI → **Panel**
2. Rename to `ObjectiveItem`
3. **RectTransform:**
   - Width: Stretch (use anchor stretch)
   - Height: 60-80 (larger for mobile!)
4. **Add Horizontal Layout Group:**
   - Padding: 15
   - Spacing: 15
   - Child Alignment: Middle Left
   - ✓ Control Child Size: Width & Height
   - ✓ Child Force Expand: Width only

**Inside ObjectiveItem, create 3 children:**

#### A) Icon (Optional - for visual polish)
1. Right-click `ObjectiveItem` → UI → **Image**
2. Rename to `Icon`
3. Size: 50x50
4. Leave sprite empty for now (can add animal icons later)

#### B) Description
1. Right-click `ObjectiveItem` → UI → **Text - TextMeshPro**
2. Rename to **`Description`** (exact name!)
3. **Settings:**
   - Text: "Collect Red Animals"
   - Font Size: 28-32 (mobile-friendly!)
   - Color: White
   - Alignment: Left, Middle
   - Wrapping: Enabled
   - **Add Component → Layout Element:**
     - Flexible Width: 1 (allows it to expand)

#### C) Progress
1. Right-click `ObjectiveItem` → UI → **Text - TextMeshPro**
2. Rename to **`Progress`** (exact name!)
3. **Settings:**
   - Text: "0/20"
   - Font Size: 32-36 (slightly larger, bold)
   - Font Style: Bold
   - Color: Yellow (#FFD700)
   - Alignment: Right, Middle
   - **Layout Element:**
     - Min Width: 100

#### D) Checkmark (Optional)
1. Right-click `ObjectiveItem` → UI → **Image**
2. Rename to **`Checkmark`** (exact name!)
3. Size: 40x40
4. Color: Green (#00FF00)
5. Source Image: Use a checkmark sprite or Unity's default
6. **Disable it** (uncheck active in Inspector)

### 7. Make it a Prefab

1. Create folder: **Assets/Prefabs** (if doesn't exist)
2. **Drag** `ObjectiveItem` from Hierarchy → into Prefabs folder
3. You should see a **blue cube icon**
4. **DELETE** `ObjectiveItem` from Hierarchy

### 8. Create Timer (Optional)

1. Right-click `ObjectivePanel` → UI → **Text - TextMeshPro**
2. Rename to `TimerText`
3. **Settings:**
   - Text: "1:00"
   - Font Size: 48-56 (large and visible!)
   - Font Style: Bold
   - Color: White
   - Alignment: Center
   - **RectTransform:**
     - Anchor: Bottom-Center
     - Width: Stretch
     - Height: 70
     - Position Y: 10

### 9. Link Everything in ObjectiveUI

Select `ObjectivePanel`, in the Inspector find ObjectiveUI component:

- **Objective Panel**: Drag `ObjectivePanel` itself
- **Objective Container**: Drag `ObjectiveContainer`
- **Objective Prefab**: Drag prefab from **Assets/Prefabs** folder (blue icon)
- **Timer Text**: Drag `TimerText`

---

## Why This Approach is Better

✅ **Works with SafeArea** - Automatically adjusts for notches/cutouts
✅ **Proper mobile scale** - Uses 28-56pt font sizes (readable on phones)
✅ **Flexible layout** - Expands/contracts based on number of objectives
✅ **No board overlap** - Lives in UI space, separate from world space board

---

## Visual Layout Result

```
┌────────────────────────────────┐
│ ■ SafeArea (auto-adjusts)     │
│ ┌─────────────────────┐        │
│ │   OBJECTIVES        │        │
│ ├─────────────────────┤        │
│ │ ○ Reach 1000 Points │        │
│ │                0/1000        │
│ │                              │
│ │ ○ Collect Red Animals │      │
│ │               15/20          │
│ │                              │
│ │ ○ Clear All Ice      │       │
│ │                3/5           │
│ ├─────────────────────┤        │
│ │      0:45            │       │
│ └─────────────────────┘        │
│                                │
│   [Board in world space]       │
│       below UI layer           │
└────────────────────────────────┘
```

---

## Adjusting Position

If the panel overlaps with your existing score/moves text:

**Move it down:**
1. Select `ObjectivePanel`
2. Change Position Y to something like -100 or -150
3. Test on different aspect ratios in Game view

**Move existing UI up:**
- Alternatively, move Score/Moves text higher to make room

**Put it on the right:**
- Change anchor to Top-Right
- Set Position X to -20 (negative for right side)

---

## Test It

1. **Create simple level data** with one Score objective (500 points)
2. **Assign to Board**
3. **Play**
4. You should see the objective appear and update!

---

## Pro Tips

**For Better Visuals:**
- Add a subtle border (Outline component) to ObjectivePanel
- Use gradient backgrounds
- Add icons for each objective type
- Animate checkmarks when objectives complete

**For Different Screen Sizes:**
- Test in Game view with different aspect ratios (16:9, 19.5:9, etc.)
- SafeArea will handle notches automatically
- Adjust font sizes if too small/large

**Performance:**
- The system is already optimized
- Updates only when progress changes
- No Update() loops polling

---

You're all set! The UI will work smoothly with your SafeArea system and look great on all devices! 🎯
