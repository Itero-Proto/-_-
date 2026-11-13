
using System;
using System.Collections;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    public Action OnFirstAttackStarted;

    [Header("Laser Settings")]
    [SerializeField] private float laserSpeed = 10f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip shotSound;
    [Range(0f, 1f)]
    [SerializeField] private float chargeVolume = 0.8f;
    [Range(0f, 1f)]
    [SerializeField] private float shotVolume = 1f;

    [Header("Activation")]
    [SerializeField] private bool startOnAwake = true;

    private bool attackLoopStarted;

    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Laser")]
    [SerializeField] private LaserProjectile laserPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Aim Line")]
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private Color aimStartColor = new Color(0.4f, 0f, 0f, 0.4f);
    [SerializeField] private Color aimEndColor = Color.red;

    [Header("Aim Line Width")]
    [SerializeField] private float startAimWidth = 0.025f;
    [SerializeField] private float endAimWidth = 0.2f;

    [Header("Charge Glow")]
    [SerializeField] private SpriteRenderer chargeGlow;
    [SerializeField] private Color glowColor = Color.red;
    [SerializeField] private float maxGlowAlpha = 0.8f;
    [SerializeField] private float glowScale = 1.5f;

    [Header("Recoil")]
    [SerializeField] private float recoilDistance = 0.2f;
    [SerializeField] private float recoilDuration = 0.12f;
    [SerializeField] private float returnDuration = 0.2f;

    private Vector3 originalPosition;
    private Coroutine recoilCoroutine;
    private Vector3 originalGlowScale;

    [Header("Random Attack Timing")]
    [SerializeField] private float minFirstAttackDelay = 1.5f;
    [SerializeField] private float maxFirstAttackDelay = 4f;

    [SerializeField] private float minAttackDelay = 1.5f;
    [SerializeField] private float maxAttackDelay = 4f;

    [Header("Random Charge Time")]
    [SerializeField] private float minChargeTime = 0.6f;
    [SerializeField] private float maxChargeTime = 1.2f;

    [SerializeField] private float recoveryTime = 0.9f;

    private SpriteRenderer spriteRenderer;
    private Color normalColor;

    [Header("Damage Effects")]
    [SerializeField] private DamageEffects damageEffects;

    [Header("Hit Effect")]
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private float hitDuration = 0.12f;
    [SerializeField] private float hitScaleMultiplier = 1.15f;

    private Vector3 originalScale;
    private Coroutine hitEffectCoroutine;

    private void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            Debug.LogWarning(gameObject.name + ": AudioSource не назначен!");

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (damageEffects == null)
            damageEffects = GetComponentInChildren<DamageEffects>();

        if (spriteRenderer != null)
            normalColor = spriteRenderer.color;

        if (aimLine == null)
            aimLine = GetComponent<LineRenderer>();

        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.useWorldSpace = true;
            aimLine.enabled = false;
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": LineRenderer не назначен!");
        }

        if (chargeGlow != null)
        {
            originalGlowScale = chargeGlow.transform.localScale;

            Color color = glowColor;
            color.a = 0f;
            chargeGlow.color = color;
        }

        if (startOnAwake)
            ActivateCamera();
    }

    public void ActivateCamera()
    {
        if (attackLoopStarted)
            return;

        attackLoopStarted = true;

        StartCoroutine(AttackLoop());

        Debug.Log(gameObject.name + ": камера активирована!");
    }

    private IEnumerator AttackLoop()
    {
        float firstDelay = UnityEngine.Random.Range(
            minFirstAttackDelay,
            maxFirstAttackDelay
        );

        yield return new WaitForSeconds(firstDelay);

        OnFirstAttackStarted?.Invoke();

        while (true)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.red;

            float currentChargeTime = UnityEngine.Random.Range(
                minChargeTime,
                maxChargeTime
            );

            // Звук подготовки к выстрелу.
            PlaySound(chargeSound, chargeVolume);

            yield return StartCoroutine(ChargeAim(currentChargeTime));

            if (aimLine != null)
                aimLine.enabled = false;

            if (player != null && laserPrefab != null && firePoint != null)
            {
                LaserProjectile laser = Instantiate(
                    laserPrefab,
                    firePoint.position,
                    Quaternion.identity
                );

                laser.Launch(player.position, laserSpeed);

                // Звук непосредственно в момент выстрела.
                PlaySound(shotSound, shotVolume);

                PlayRecoil();

                Debug.Log(gameObject.name + ": СТРЕЛЯЕТ!");
            }

            UpdateChargeGlow(0f);

            if (spriteRenderer != null)
                spriteRenderer.color = normalColor;

            yield return new WaitForSeconds(recoveryTime);

            float nextDelay = UnityEngine.Random.Range(
                minAttackDelay,
                maxAttackDelay
            );

            yield return new WaitForSeconds(nextDelay);
        }
    }

    private IEnumerator ChargeAim(float currentChargeTime)
    {
        float timer = 0f;

        aimLine.enabled = true;

        while (timer < currentChargeTime)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / currentChargeTime);

            float currentWidth = Mathf.Lerp(
                startAimWidth,
                endAimWidth,
                progress
            );

            aimLine.startWidth = currentWidth;
            aimLine.endWidth = currentWidth;

            aimLine.SetPosition(0, firePoint.position);
            aimLine.SetPosition(1, player.position);

            UpdateChargeGlow(progress);

            yield return null;
        }
    }

    private void UpdateChargeGlow(float progress)
    {
        if (chargeGlow == null)
            return;

        progress = Mathf.Clamp01(progress);

        Color color = glowColor;
        color.a = Mathf.Lerp(0f, maxGlowAlpha, progress);

        chargeGlow.color = color;

        float scale = Mathf.Lerp(1f, glowScale, progress);

        chargeGlow.transform.localScale =
            originalGlowScale * scale;
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volume);
    }

    private void PlayRecoil()
    {
        if (recoilCoroutine != null)
            StopCoroutine(recoilCoroutine);

        recoilCoroutine = StartCoroutine(RecoilRoutine());
    }

    private IEnumerator RecoilRoutine()
    {
        Vector3 direction = Vector3.zero;

        if (player != null)
            direction = (player.position - transform.position).normalized;

        Vector3 recoilPosition =
            originalPosition - direction * recoilDistance;

        float timer = 0f;

        while (timer < recoilDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / recoilDuration);

            transform.position = Vector3.Lerp(
                originalPosition,
                recoilPosition,
                progress
            );

            yield return null;
        }

        timer = 0f;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / returnDuration);

            progress = 1f - Mathf.Pow(1f - progress, 3f);

            transform.position = Vector3.Lerp(
                recoilPosition,
                originalPosition,
                progress
            );

            yield return null;
        }

        transform.position = originalPosition;
        recoilCoroutine = null;
    }

    public void PlayHitEffect()
    {
        if (damageEffects != null)
            damageEffects.PlayHit();

        if (hitEffectCoroutine != null)
            StopCoroutine(hitEffectCoroutine);

        hitEffectCoroutine = StartCoroutine(HitEffectRoutine());
    }

    private IEnumerator HitEffectRoutine()
    {
        Color previousColor = spriteRenderer != null
            ? spriteRenderer.color
            : normalColor;

        if (spriteRenderer != null)
            spriteRenderer.color = hitColor;

        Vector3 enlargedScale = originalScale * hitScaleMultiplier;

        float timer = 0f;

        while (timer < hitDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / hitDuration);

            transform.localScale = Vector3.Lerp(
                originalScale,
                enlargedScale,
                progress
            );

            yield return null;
        }

        timer = 0f;

        while (timer < hitDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / hitDuration);

            transform.localScale = Vector3.Lerp(
                enlargedScale,
                originalScale,
                progress
            );

            yield return null;
        }

        transform.localScale = originalScale;

        if (spriteRenderer != null)
            spriteRenderer.color = previousColor;

        hitEffectCoroutine = null;
    }
}
