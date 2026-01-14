using UnityEngine;

public class SafeArea : MonoBehaviour {
    
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    void Awake() {
        panel = GetComponent<RectTransform>();
        Refresh();
    }

    void Update() {
        if (lastSafeArea != Screen.safeArea) {
            Refresh();
        }
    }

    void Refresh() {
        Rect safeArea = Screen.safeArea;

        if (safeArea != lastSafeArea) {
            lastSafeArea = safeArea;
            ApplySafeArea(safeArea);
        }
    }

    void ApplySafeArea(Rect r) {
        // Check for invalid screen startup state on some Android devices
        if (Screen.width > 0 && Screen.height > 0) {
            // Convert safe area rectangle from Screen Space to Anchor Space (0 to 1)
            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
        }
    }
}