using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {

    private Vector3 trueOriginPosition;
    private bool isShaking = false;
    private Coroutine currentShakeRoutine;

    public IEnumerator Shake(float duration, float magnitude) {
        
        if (PlayerPrefs.GetInt("ShakeEnabled", 1) == 0) {
            yield break;
        }

        // 1. If we are NOT already shaking, save the current spot as the "True Home".
        //    If we ARE already shaking, we trust the 'trueOriginPosition' we saved earlier.
        if (!isShaking) {
            trueOriginPosition = transform.localPosition;
            isShaking = true;
        }

        // 2. If a shake is already running, stop it so we can restart with the new timer
        if (currentShakeRoutine != null) {
            StopCoroutine(currentShakeRoutine);
        }

        // 3. Start the actual movement loop
        currentShakeRoutine = StartCoroutine(DoShake(duration, magnitude));
        yield return currentShakeRoutine;
    }

    private IEnumerator DoShake(float duration, float magnitude) {
        float elapsed = 0.0f;

        while (elapsed < duration) {
            // Shake relative to the TRUE ORIGIN, not the current position
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(trueOriginPosition.x + x, trueOriginPosition.y + y, trueOriginPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4. Reset to the True Home and clear the flag
        transform.localPosition = trueOriginPosition;
        isShaking = false;
    }
}