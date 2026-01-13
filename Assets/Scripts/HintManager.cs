using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required for Lists

public class HintManager : MonoBehaviour {

    public Board board;
    public float hintDelay = 3f;
    private float hintDelaySeconds;
    
    [Header("Visuals")]
    public GameObject glowPrefab; 
    
    // Tracks ALL current glow objects so we can delete them later
    private List<GameObject> activeGlows = new List<GameObject>(); 
    
    void Start() {
        hintDelaySeconds = hintDelay;
        if(board == null) board = FindFirstObjectByType<Board>();
    }

    void Update() {
        hintDelaySeconds -= Time.deltaTime;
        
        // Show hint only if timer is up AND we aren't already showing one
        if (hintDelaySeconds <= 0 && activeGlows.Count == 0) {
            MarkHint();
        }
    }

    public void ResetTimer() {
        hintDelaySeconds = hintDelay;
        StopHint();
    }

    void MarkHint() {
        // Get the pair of dots from the board
        List<GameObject> move = board.CheckForMatches();
        
        if (move != null) {
            foreach(GameObject dot in move) {
                if(dot != null) {
                    // Create a glow for this dot
                    GameObject newGlow = Instantiate(glowPrefab, dot.transform.position, Quaternion.identity);
                    
                    // Parent to the dot so it follows swaps
                    newGlow.transform.SetParent(dot.transform);
                    
                    // Add to our list for tracking
                    activeGlows.Add(newGlow);
                    
                    // Start the breathing animation
                    StartCoroutine(FadeGlow(newGlow.GetComponent<SpriteRenderer>()));
                }
            }
        }
    }
    
    void StopHint() {
        // Destroy all glows in the list
        for(int i = 0; i < activeGlows.Count; i++) {
            if(activeGlows[i] != null) {
                Destroy(activeGlows[i]);
            }
        }
        activeGlows.Clear();
    }
    
    IEnumerator FadeGlow(SpriteRenderer glowSprite) {
        if(glowSprite == null) yield break;

        float alphaSpeed = 2f; 
        float minAlpha = 0f;
        float maxAlpha = 0.6f; 

        while(glowSprite != null) {
            
            // Fade In
            float t = 0f;
            while(t < 1f) {
                if(glowSprite == null) yield break;
                float newAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);
                Color c = glowSprite.color;
                glowSprite.color = new Color(c.r, c.g, c.b, newAlpha);
                t += Time.deltaTime * alphaSpeed;
                yield return null;
            }

            // Fade Out
            t = 0f;
            while(t < 1f) {
                if(glowSprite == null) yield break;
                float newAlpha = Mathf.Lerp(maxAlpha, minAlpha, t);
                Color c = glowSprite.color;
                glowSprite.color = new Color(c.r, c.g, c.b, newAlpha);
                t += Time.deltaTime * alphaSpeed;
                yield return null;
            }
        }
    }
}