
using UnityEngine;

public class ParryEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float maxScale = 2f;

    private float timer;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        timer = duration;
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        float progress = 1f - timer / duration;

        transform.localScale = Vector3.one * Mathf.Lerp(0f, maxScale, progress);

        Color color = spriteRenderer.color;
        color.a = 1f - progress;
        spriteRenderer.color = color;

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
