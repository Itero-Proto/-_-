
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private float damageVolume = 0.8f;
    [SerializeField] private int maxHealth = 3;
    private DamageEffects damageEffects;
    private int currentHealth;

    private void Awake()
    {
        damageEffects = GetComponent<DamageEffects>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
    }

    public void TakeDamage()
    {
        if (currentHealth <= 0)
            return;

        currentHealth--;
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound, damageVolume);
        }
        if (damageEffects != null)
            damageEffects.PlayHit();
        if (GameFeel.Instance != null)
            GameFeel.Instance.Shake();

        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {

            if (UIManager.Instance != null)
                UIManager.Instance.ShowGameOver();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerHealth(
                currentHealth,
                maxHealth
            );
        }

    }

}
