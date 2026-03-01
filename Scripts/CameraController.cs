using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    /*
    For isometric camera set up
    offsets = Vector3 (9, 12, -10)
    Rotation = Vector3 ( 40, -45, 0)
    */
    
    public Transform m_target;
    public Vector3 m_cameraOffsets;

    [Header("Camera Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;

    private Vector3 shakeOffset = Vector3.zero;
    private bool isShaking = false;

    // Static reference for easy access
    public static CameraController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (m_target != null)
        {
            transform.position = m_target.position + m_cameraOffsets + shakeOffset;
        }
    }

    public void Shake()
    {
        Shake(shakeDuration, shakeMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            shakeOffset = new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        isShaking = false;
    }
}
