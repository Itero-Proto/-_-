
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerParry : MonoBehaviour
{
    public System.Action<bool> OnSuccessfulParry;

    private Animator animator;

    [Header("Parry Settings")]
    [SerializeField] private float parryDuration = 0.3f;
    [SerializeField] private float perfectParryDuration = 0.1f;
    [SerializeField] private float staminaCost = 20f;

    [Header("Effects")]
    [SerializeField] private ParryEffect parryEffectPrefab;
    [Header("Parry Effects")]
    [SerializeField] private ParryEffect normalParryEffectPrefab;
    [SerializeField] private ParryEffect perfectParryEffectPrefab;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip parrySwingSound;
    [SerializeField] private AudioClip normalParrySound;
    [SerializeField] private AudioClip perfectParrySound;
    [SerializeField] private AudioClip healSound;

    [SerializeField] private float swingVolume = 0.7f;
    [SerializeField] private float normalVolume = 0.8f;
    [SerializeField] private float perfectVolume = 1f;
    [SerializeField] private float healVolume = 0.8f;

    private PlayerStamina stamina;
    private PlayerHealth health;

    private float parryTimer;
    private float perfectTimer;

    private bool isParrying;
    private bool parrySucceeded;

    public bool IsParrying => isParrying;
    public bool IsPerfectParrying => isParrying && perfectTimer > 0f;

    private void Awake()
    {
        stamina = GetComponent<PlayerStamina>();
        health = GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Управление с клавиатуры.
        bool spacePressed =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame;

        // Управление касанием экрана телефона.
        bool screenTouched =
            Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        // Парирование при любом из этих действий.
        if (spacePressed || screenTouched)
        {
            StartParry();
        }

        if (isParrying)
        {
            parryTimer -= Time.deltaTime;
            perfectTimer -= Time.deltaTime;

            if (parryTimer <= 0f)
            {
                isParrying = false;

                if (!parrySucceeded)
                {
                    if (LevelStatistics.Instance != null)
                        LevelStatistics.Instance.RecordFailedParry();
                }
            }
        }
    }

    private void StartParry()
    {
        if (isParrying)
            return;

        if (stamina == null || !stamina.TrySpend(staminaCost))
        {
            return;
        }

        isParrying = true;
        parrySucceeded = false;

        if (animator != null)
            animator.Play("Player_Parry", 0, 0f);

        if (audioSource != null && parrySwingSound != null)
            audioSource.PlayOneShot(parrySwingSound, swingVolume);

        parryTimer = parryDuration;
        perfectTimer = perfectParryDuration;
    }

    public void SuccessfulParry(bool isPerfect)
    {
        if (!isParrying || parrySucceeded)
            return;

        parrySucceeded = true;
        // Выбираем эффект в зависимости от типа парирования.
        ParryEffect selectedEffect = isPerfect
            ? perfectParryEffectPrefab
            : normalParryEffectPrefab;

        if (selectedEffect != null)
        {
            Instantiate(
                selectedEffect,
                transform.position,
                Quaternion.identity
            );
        }
        OnSuccessfulParry?.Invoke(isPerfect);

        if (stamina != null)
            stamina.Restore(staminaCost);

        if (LevelStatistics.Instance != null)
        {
            if (isPerfect)
                LevelStatistics.Instance.RecordPerfectParry();
            else
                LevelStatistics.Instance.RecordNormalParry();
        }

        if (isPerfect)
        {
            if (health != null)
                health.Heal(1);

            if (audioSource != null && healSound != null)
                audioSource.PlayOneShot(healSound, healVolume);

            if (UIManager.Instance != null)
                UIManager.Instance.ShowPerfectParry();

            if (GameFeel.Instance != null)
                GameFeel.Instance.HitStop();

            if (audioSource != null && perfectParrySound != null)
                audioSource.PlayOneShot(perfectParrySound, perfectVolume);
        }
        else
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowNormalParry();

            if (audioSource != null && normalParrySound != null)
                audioSource.PlayOneShot(normalParrySound, normalVolume);
        }
    }
}
