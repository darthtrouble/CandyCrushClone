using UnityEngine;

public class PulseUI : MonoBehaviour {
    public float speed = 2f;
    public float scaleAmount = 0.1f;
    private Vector3 startScale;

    void Start() {
        startScale = transform.localScale;
    }

    void Update() {
        float wave = Mathf.Sin(Time.time * speed);
        float scaleOffset = wave * scaleAmount;
        transform.localScale = startScale + (Vector3.one * scaleOffset);
    }
}