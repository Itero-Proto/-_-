
using UnityEngine;
using System.Collections;

public class GameFeel : MonoBehaviour
{
    public static GameFeel Instance;

    [Header("Hit Stop")]
    [SerializeField] private float hitStopDuration = 0.06f;
    [SerializeField] private float hitStopScale = 0.05f;

    [Header("Screen Shake")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.15f;

    private Coroutine shakeCoroutine;
    private Transform cameraTransform;
    private Vector3 cameraStartPosition;

    private void Awake()
    {
        Instance = this;

        cameraTransform = Camera.main.transform;
        cameraStartPosition = cameraTransform.position;
    }

    public void HitStop()
    {
        StartCoroutine(HitStopCoroutine());
    }

    private IEnumerator HitStopCoroutine()
    {
        Time.timeScale = hitStopScale;

        yield return new WaitForSecondsRealtime(hitStopDuration);

        Time.timeScale = 1f;
    }

    public void Shake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        float timer = shakeDuration;

        while (timer > 0f)
        {
            Vector2 offset = Random.insideUnitCircle * shakeMagnitude;

            cameraTransform.position = cameraStartPosition + new Vector3(
                offset.x,
                offset.y,
                0f
            );

            timer -= Time.unscaledDeltaTime;

            yield return null;
        }

        cameraTransform.position = cameraStartPosition;
        shakeCoroutine = null;
    }
}
