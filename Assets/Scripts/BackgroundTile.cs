using UnityEngine;

public class BackgroundTile : MonoBehaviour {

    public int hitPoints; // 0 = Normal (Invisible), 1 = Ice (Visible)
    private SpriteRenderer spriteRenderer;

    public void Setup(int hp) {
        hitPoints = hp;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    public void TakeDamage(int damage) {
        hitPoints -= damage;
        UpdateSprite();
    }

    public Sprite iceSprite; // Assign in Inspector

    void UpdateSprite() {
        if (hitPoints <= 0) {
            hitPoints = 0;
            // Invisible when destroyed
            spriteRenderer.color = new Color(1f, 1f, 1f, 0f); 
        } 
        else {
            // ICE STATE
            if (iceSprite != null) {
                // 1. Transparency: Set alpha to 0.4f (was 0.6f)
                spriteRenderer.sprite = iceSprite;
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);
                
                // 2. Sorting: Put ON TOP of animals
                // "Units" is the layer animals are on. Order 10 puts it above them (usually 0)
                spriteRenderer.sortingLayerName = "Units"; 
                spriteRenderer.sortingOrder = 10;
                
                // 3. Size: Start at 0.5 (was 0.6)
                transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }
            else {
                // Fallback: Tint the existing sprite blue
                spriteRenderer.color = new Color(0.5f, 0.8f, 1f, 0.8f);
            }
        }
    }
}