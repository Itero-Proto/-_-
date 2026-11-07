
using UnityEngine;
using System.Collections;

public class CameraHealth : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private float damageVolume = 0.8f;
    [Header("Health")]
    private DamageEffects damageEffects;
    [SerializeField] private int maxHealth = 3;

    [Header("Hit Feedback")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float destroyDelay = 0.5f;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private bool isDestroyed;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        damageEffects = GetComponent<DamageEffects>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void TakeDamage()
    {
        if (isDestroyed)
            return;

        currentHealth--;
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound, damageVolume);
        }
        if (damageEffects != null)
            damageEffects.PlayHit();
        // лёгкая тряска камеры
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.1f, 0.1f);
        Debug.Log("Здоровье камеры: " + currentHealth);

        StopAllCoroutines();
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            if (damageEffects != null)
                damageEffects.PlayDestroyed();

            DestroyCamera();
        }
    }

    private IEnumerator HitFlash()
    {
        spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(flashDuration);

        if (!isDestroyed)
            spriteRenderer.color = originalColor;
    }
    private void DestroyCamera()
    {
        isDestroyed = true;

        // Мощная тряска при уничтожении
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.35f, 0.25f);

        Debug.Log(gameObject.name + " УНИЧТОЖЕНА!");

        spriteRenderer.color = Color.black;

        if (LevelObjective.Instance != null)
        {
            LevelObjective.Instance.CameraDestroyed();
        }

        Destroy(gameObject, destroyDelay);
    }
}
