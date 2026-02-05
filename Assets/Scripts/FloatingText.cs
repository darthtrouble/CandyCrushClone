using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour {

    public float moveSpeed = 2f;
    public float fadeSpeed = 3f;
    public float lifetime = 2f; // Increased failsafe time

    private TextMeshPro textMesh;
    private Color textColor;
    private Vector3 randomOffset;
    
    // Fly-to-score logic
    private ScoreManager scoreManager;
    private int scoreValue;
    private float timeSinceStart = 0f;

    public void Init(int score, ScoreManager manager) {
        scoreValue = score;
        scoreManager = manager;
        SetScore(score);
    }

    void Start() {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) return;
        
        textColor = textMesh.color;
        
        // --- POLISH: Force high sort order and scale up slightly on spawn ---
        textMesh.sortingOrder = 20; 
        transform.localScale = Vector3.one * 0.15f; // Drastically smaller

        // Random X drift
        randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), 1f, 0);

        // Safety destroy in case logic fails
        Destroy(gameObject, lifetime);
    }

    void Update() {
        if (textMesh == null) return;

        timeSinceStart += Time.deltaTime;

        // PHASE 1: POP & WAIT (0 to 0.4s) -> Reduced wait slightly
        if (timeSinceStart < 0.4f) {
            // Move Up with drift
            transform.position += (Vector3.up * moveSpeed + randomOffset) * Time.deltaTime;

            // Pop scaling
            // Target 0.25f is extremely compact
            if(transform.localScale.x < 0.25f) {
                transform.localScale += Vector3.one * Time.deltaTime * 2f; // Slower pop
            }
        }
        // PHASE 2: FLY TO SCORE (0.4s onwards)
        else {
             if (scoreManager != null && scoreManager.scoreText != null) {
                 Vector3 targetPos;
                 RectTransform targetRect = scoreManager.scoreText.rectTransform;
                 Canvas canvas = targetRect.GetComponentInParent<Canvas>();

                 Vector3 screenPoint;
                 if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                     // In Overlay, position IS screen coords
                     screenPoint = targetRect.position; 
                 } else {
                     // In Camera/World space, map to screen using the rendering camera
                     Camera uiCam = (canvas != null && canvas.worldCamera != null) ? canvas.worldCamera : Camera.main;
                     screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, targetRect.position);
                 }

                 // Project Screen Point back to World at correct depth (Z=0 for board)
                 // Camera at z=-10 (usually), Board at 0 => Distance 10
                 float zDepth = Mathf.Abs(Camera.main.transform.position.z);
                 targetPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, zDepth));

                 // Lerp towards target
                 transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 8f);
                 transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.1f, Time.deltaTime * 5f); 

                 // Check arrival
                 if (Vector3.Distance(transform.position, targetPos) < 1.0f) {
                     // Trigger Score Update
                     scoreManager.AddVisibleScore(scoreValue);
                     Destroy(gameObject);
                 }
             }
             else {
                 // Fallback if no manager: just fade out
                 if(textColor.a > 0) {
                    textColor.a -= fadeSpeed * Time.deltaTime;
                    textMesh.color = textColor;
                 }
             }
        }
    }
    
    public void SetScore(int value) {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null) textMesh.text = "+" + value;
    }
}