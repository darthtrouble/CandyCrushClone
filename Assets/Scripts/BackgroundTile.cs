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

    void UpdateSprite() {
        if (hitPoints <= 0) {
            hitPoints = 0;
            // CHANGE: Set Alpha (last number) to 0 so it is INVISIBLE
            spriteRenderer.color = new Color(1f, 1f, 1f, 0f); 
        } 
        else {
            // CHANGE: Set Alpha to 1 so the ICE is VISIBLE
            // (You can tweak these numbers for different ice shades)
            spriteRenderer.color = new Color(0.5f, 0.8f, 1f, 0.8f);
        }
    }
}