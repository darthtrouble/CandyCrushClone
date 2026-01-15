using UnityEngine;

public class BackgroundScaler : MonoBehaviour {
    void Start() {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // 1. Reset Scale to 1 to do correct math
        transform.localScale = Vector3.one;

        // 2. Get the Sprite dimensions
        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;

        // 3. Get the Screen dimensions (World Units)
        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        // 4. Calculate Scale needed
        Vector3 newScale = transform.localScale;
        newScale.x = worldScreenWidth / width;
        newScale.y = worldScreenHeight / height;
        
        // 5. Apply (Use the larger one to keep aspect ratio, or stretch both to fill)
        // Option A: Stretch to Fill (might distort slightly)
        transform.localScale = new Vector3(Mathf.Max(newScale.x, newScale.y), Mathf.Max(newScale.x, newScale.y), 1);
    }

    void Update() {
        // 6. Follow the Camera position (on X and Y only)
        // Keep Z at 10 so it stays in the background
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 10f);
    }
}