using UnityEngine;
using System.Collections;

public class TutorialHand : MonoBehaviour {

    public Vector3 startPos;
    public Vector3 endPos;
    public float speed = 2f;
    public float handScale = 0.6f; // Reduced default size
    
    private SpriteRenderer sr;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        if(sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void Setup(Vector3 start, Vector3 end) {
        // Set scale first so bounds are accurate
        transform.localScale = Vector3.one * handScale; 
        
        // Calculate offset to make Top-Left corner point to the target
        // Assuming Pivot is Center: Top-Left is at (-extents.x, +extents.y) relative to center
        // To place Top-Left at 'start', we move the Center RIGHT by extents.x and DOWN by extents.y
        Vector3 offset = Vector3.zero;
        if (sr != null) {
            offset = new Vector3(sr.bounds.extents.x, -sr.bounds.extents.y, 0);
        }
        
        startPos = start + offset;
        endPos = end + offset;
        
        transform.position = startPos;
        StartCoroutine(SwipeRoutine());
    }

    IEnumerator SwipeRoutine() {
        while (true) {
            float t = 0;
            transform.position = startPos;
            
            // 1. Fade In / Scale Up
            if(sr) sr.color = new Color(1, 1, 1, 1);
            transform.localScale = Vector3.one * handScale * 1.2f; // Press down size relative to base scale
            yield return new WaitForSeconds(0.2f);

            // 2. Move
            while (t < 1f) {
                t += Time.deltaTime * speed;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            // 3. Fade Out
            if (sr) {
                float fadeT = 0;
                while(fadeT < 1f) {
                    fadeT += Time.deltaTime * 5f;
                    sr.color = new Color(1, 1, 1, 1f - fadeT);
                    yield return null;
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
}
