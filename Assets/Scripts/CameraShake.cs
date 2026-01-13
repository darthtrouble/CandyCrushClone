using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {

    // Shake function taking Duration (how long) and Magnitude (how strong)
    public IEnumerator Shake(float duration, float magnitude) {
        
        Vector3 originalPos = new Vector3(0, 0, -10f); // Default camera position
        // If your camera moves (e.g. following player), capture transform.localPosition instead.
        // For this puzzle game, the camera is static, so hardcoding or capturing start pos works.
        // Let's capture current pos to be safe:
        originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration) {
            // Generate random offset
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Apply offset
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; // Wait for next frame
        }

        // Reset to original position
        transform.localPosition = originalPos;
    }
}