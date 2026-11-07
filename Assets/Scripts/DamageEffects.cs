
using System.Collections;
using UnityEngine;

public class DamageEffects : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private ParticleSystem destroyParticles;

    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void PlayHit()
    {
        // Вспышка спрайта
        if (spriteRenderer != null)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        // Частицы попадания
        if (hitParticles != null)
        {
            ParticleSystem particles = Instantiate(
                hitParticles,
                transform.position,
                Quaternion.identity
            );

            particles.Play();

            Destroy(particles.gameObject, 3f);
        }
    }

    public void PlayDestroyed()
    {
        if (destroyParticles == null)
            return;

        ParticleSystem particles = Instantiate(
            destroyParticles,
            transform.position,
            Quaternion.identity
        );

        particles.Play();

        Destroy(particles.gameObject, 3f);
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }
}
