using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour {

    public float moveSpeed = 2f;
    public float fadeSpeed = 3f;
    public float lifetime = 1f; // Guaranteed to die after this time

    private TextMeshPro textMesh;
    private Color textColor;

    void Start() {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) return;

        textColor = textMesh.color;

        // --- FIX 1: ORDER IN LAYER ---
        // Force text to render on top of sprites (usually Order 0)
        textMesh.sortingOrder = 20; 
        
        // --- FIX 2: SAFETY DESTROY ---
        // Even if the fade logic fails, this deletes the object after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    void Update() {
        if (textMesh == null) return;

        // 1. Move Up
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // 2. Fade Out
        // We modify the Alpha (transparency)
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;
    }
    
    public void SetScore(int scoreValue) {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        textMesh.text = "+" + scoreValue;
    }
}