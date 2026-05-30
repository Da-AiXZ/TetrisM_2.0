using UnityEngine;

// APK MyCamera.cs: camera shake on landing/clear/explosion
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    private static float shakeIntensity = 0f;
    private static float shakeDuration = 0f;
    private static float shakeTimer = 0f;
    private Vector3 originalPos;

    private void Awake()
    {
        Instance = this;
        originalPos = transform.position;
    }

    // APK: Shake(xOffset, duration) - xOffset is0 (only random shake)
    public static void Shake(float xOffset, float duration)
    {
        shakeIntensity = 0.15f;
        shakeDuration = duration;
        shakeTimer = 0f;
    }

    private void FixedUpdate()
    {
        if (shakeTimer >= shakeDuration)
        {
            shakeIntensity = 0f;
            if (Instance != null)
                Instance.transform.position = Instance.originalPos;
            return;
        }

        shakeTimer += Time.fixedDeltaTime;
        float progress = shakeTimer / shakeDuration;
        float currentIntensity = shakeIntensity * (1f - progress);
        float x = Random.Range(-currentIntensity, currentIntensity);
        float y = Random.Range(-currentIntensity, currentIntensity);
        if (Instance != null)
            Instance.transform.position = Instance.originalPos + new Vector3(x, y, 0);
    }
}
