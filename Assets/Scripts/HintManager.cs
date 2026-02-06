using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required for Lists

public class HintManager : MonoBehaviour {

    public Board board;
    public float hintDelay = 3f;
    private float hintDelaySeconds;
    
    [Header("Visuals")]
    public GameObject glowPrefab; 
    public GameObject handPrefab; // DRAG YOUR HAND / FINGER SPRITE HERE
    
    // Tracks ALL current glow objects so we can delete them later
    private List<GameObject> activeGlows = new List<GameObject>(); 
    
    void Start() {
        hintDelaySeconds = hintDelay;
        if(board == null) board = FindFirstObjectByType<Board>();
        
        // --- TUTORIAL LOGIC ---
        // If it's the very first level, show hints almost immediately to guide the player
        if (PlayerPrefs.GetInt("CurrentLevel", 0) == 0) {
            hintDelaySeconds = 0.5f; 
        }
    }

    void Update() {
        if(board.currentState != GameState.move) return; // Only hint when player can move
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
        List<Dot> move = board.CheckForMatches(); // Now returns Dot components
        
        if (move != null) {
            foreach(Dot dot in move) { // Changed from GameObject to Dot
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
            
            // --- SPAWN HAND TUTORIAL ---
            if (handPrefab != null && move.Count >= 2) {
                // Determine direction
                // move[0] is usually the one being swapped "from" in the logic, but the list order determines points
                Vector3 start = move[0].transform.position;
                Vector3 end = move[1].transform.position;
                
                GameObject hand = Instantiate(handPrefab, start, Quaternion.identity);
                activeGlows.Add(hand); // Add to list so it gets destroyed on touch
                
                TutorialHand th = hand.GetComponent<TutorialHand>();
                if(th == null) th = hand.AddComponent<TutorialHand>();
                
                th.Setup(start, end);
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