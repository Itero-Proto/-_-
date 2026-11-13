
using System.Collections;
using UnityEngine;

public class FarmOwnerController : MonoBehaviour
{
    private Vector3 shotgunStartPosition;
    private Coroutine recoilCoroutine;
    [Header("Target")]
    [SerializeField] private Transform player;
    [Header("Shotgun")]
    [SerializeField] private ShotgunPellet pelletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float pelletSpeed = 12f;

    [Header("Shotgun Recoil")]
    [SerializeField] private Transform shotgunTransform;

    [SerializeField] private float recoilDistance = 0.18f;
    [SerializeField] private float recoilDuration = 0.08f;
    [SerializeField] private float returnDuration = 0.12f;

    [SerializeField] private float recoilShake = 2f;
    [Header("Shotgun Hit Reaction")]
    [SerializeField] private float hitShakeDistance = 0.12f;
    [SerializeField] private float hitShakeDuration = 0.12f;
    [SerializeField] private float hitReturnDuration = 0.1f;
    [Header("Shotgun Defeat Animation")]
    [SerializeField] private float shotgunFlyDistance = 7f;
    [SerializeField] private float shotgunFlyDuration = 1.2f;
    [SerializeField] private float minSpinTurns = 3f;
    [SerializeField] private float maxSpinTurns = 5f;

    [Header("Attack Timing")]
    [SerializeField] private float minFirstShotDelay = 2f;
    [SerializeField] private float maxFirstShotDelay = 4f;

    [SerializeField] private float singleShotDelay = 1f;
    [SerializeField] private float fastShotDelay = 0.5f;
    [SerializeField] private float burstShotDelay = 0.33f;

    [SerializeField] private float minRestDelay = 1.5f;
    [SerializeField] private float maxRestDelay = 2f;

    [Header("Attack Chances")]
    [Range(0, 100)]
    [SerializeField] private int singleShotChance = 50;

    [Range(0, 100)]
    [SerializeField] private int fastAttackChance = 30;

    [Header("Boss Health")]
    [SerializeField] private int maxHealth = 10;
    [Header("Secret Level Victory")]
    [SerializeField] private SecretVictoryUI secretVictoryUI;
    [Header("Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    [Range(0f, 1f)]
    [SerializeField] private float shootVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float hitVolume = 1f;

    [Header("Sound Pitch")]
    [SerializeField] private float minShootPitch = 0.9f;
    [SerializeField] private float maxShootPitch = 1.1f;
    private DamageEffects damageEffects;

    private int currentHealth;
    private bool isDefeated;

    private PlayerParry playerParry;
    private PlayerHealth playerHealth;

    private void Start()
    {
        currentHealth = maxHealth;

        playerParry = FindFirstObjectByType<PlayerParry>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (damageEffects == null)
            damageEffects = GetComponent<DamageEffects>();
        if (shotgunTransform != null)
            shotgunStartPosition = shotgunTransform.localPosition;
        Debug.Log("ХОЗЯИН ФЕРМЫ: бой начался! HP: " + currentHealth);

        StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        // Случайная задержка перед первым выстрелом.
        float firstDelay = Random.Range(
            minFirstShotDelay,
            maxFirstShotDelay
        );

        yield return new WaitForSeconds(firstDelay);

        while (!isDefeated)
        {
            int attackType = Random.Range(0, 100);

            if (attackType < singleShotChance)
            {
                // Одиночный выстрел.
                Debug.Log("ФЕРМЕР: одиночный выстрел!");

                FireShotgun();

                yield return new WaitForSeconds(singleShotDelay);
            }
            else if (attackType < singleShotChance + fastAttackChance)
            {
                // Два быстрых выстрела.
                Debug.Log("ФЕРМЕР: быстрая стрельба!");

                for (int i = 0; i < 2; i++)
                {
                    FireShotgun();

                    if (i < 1)
                        yield return new WaitForSeconds(fastShotDelay);
                }
            }
            else
            {
                // Три очень быстрых выстрела.
                Debug.Log("ФЕРМЕР: ОЧЕРЕДЬ ИЗ ТРЁХ!");

                for (int i = 0; i < 3; i++)
                {
                    FireShotgun();

                    if (i < 2)
                        yield return new WaitForSeconds(burstShotDelay);
                }
            }

            // Передышка после выбранной атаки.
            float restDelay = Random.Range(
                minRestDelay,
                maxRestDelay
            );

            yield return new WaitForSeconds(restDelay);
        }
    }

    private void FireShotgun()
    {
        if (player == null || firePoint == null || pelletPrefab == null)
        {
            Debug.LogWarning(
                "Хозяин фермы: не назначены ссылки для выстрела!"
            );
            return;
        }

        // Направление от ружья к игроку.
        Vector2 direction = (
            (Vector2)player.position - (Vector2)firePoint.position
        ).normalized;

        // Создаём только одну пулю.
        ShotgunPellet pellet = Instantiate(
            pelletPrefab,
            firePoint.position,
            Quaternion.identity
        );
        pellet.Launch(direction, pelletSpeed, this);

        PlayShootSound();
        PlayShotgunRecoil();
    }

    // Вызывается отдельно для каждой пули.
    public void HandlePlayerHit(ShotgunPellet pellet)
    {
        if (isDefeated || pellet == null)
            return;

        if (playerParry != null && playerParry.IsParrying)
        {
            bool isPerfect = playerParry.IsPerfectParrying;

            playerParry.SuccessfulParry(isPerfect);

            // Отражаем только ту пулю, которая коснулась игрока.
            pellet.Reflect(transform.position);

            Debug.Log("ДРОБИНКА ОТРАЖЕНА!");
        }
        else
        {
            // Каждая пропущенная пуля наносит отдельный урон.
            if (playerHealth != null)
                playerHealth.TakeDamage();

            pellet.DestroyPellet();

            Debug.Log("Игрок получил попадание!");
        }
    }
    public void TakeDamage()
    {
        if (isDefeated)
            return;

        currentHealth--;

        PlayHitSound();

        if (currentHealth < 0)
            currentHealth = 0;

        if (damageEffects != null)
            damageEffects.PlayHit();

        // Ружьё дёргается от попадания.
        PlayShotgunHitReaction();

        if (currentHealth <= 0)
            DefeatOwner();
    }
    private void DefeatOwner()
    {
        if (isDefeated)
            return;

        isDefeated = true;
        PlayDeathSound();
        // Останавливаем атаки и отдачу.
        StopAllCoroutines();

        if (damageEffects != null)
            damageEffects.PlayDestroyed();

        // Отключаем столкновения владельца.
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
            col.enabled = false;

        // Отправляем ружьё в полёт.
        if (shotgunTransform != null)
            StartCoroutine(ShotgunFlyAwayRoutine());

        Debug.Log("ХОЗЯИН ФЕРМЫ ПОБЕЖДЁН!");

        if (secretVictoryUI != null)
        {
            secretVictoryUI.ShowVictory();
        }
        else
        {
            Debug.LogWarning("SecretVictoryUI не назначен!");
        }
    }
    private void PlayShootSound()
    {
        if (audioSource == null || shootSound == null)
            return;

        // Немного меняем высоту тона каждого выстрела.
        audioSource.pitch = Random.Range(
            minShootPitch,
            maxShootPitch
        );

        audioSource.PlayOneShot(shootSound, shootVolume);
    }
    private void PlayDeathSound()
    {
        if (audioSource == null || deathSound == null)
            return;

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(deathSound, 0.9f);
    }
    private void PlayShotgunHitReaction()
    {
        if (shotgunTransform == null || isDefeated)
            return;

        // Останавливаем предыдущую реакцию.
        if (recoilCoroutine != null)
            StopCoroutine(recoilCoroutine);

        recoilCoroutine = StartCoroutine(ShotgunHitReactionRoutine());
    }

    private IEnumerator ShotgunHitReactionRoutine()
    {
        Vector3 startPosition = shotgunStartPosition;

        // Случайное направление рывка.
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        Vector3 shakePosition =
            startPosition + new Vector3(
                randomDirection.x,
                randomDirection.y,
                0f
            ) * hitShakeDistance;

        // Резкий рывок.
        float timer = 0f;

        while (timer < hitShakeDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / hitShakeDuration);

            shotgunTransform.localPosition = Vector3.Lerp(
                startPosition,
                shakePosition,
                progress
            );

            yield return null;
        }

        // Плавное возвращение.
        timer = 0f;

        while (timer < hitReturnDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / hitReturnDuration);

            float curve = 1f - Mathf.Pow(1f - progress, 3f);

            shotgunTransform.localPosition = Vector3.Lerp(
                shakePosition,
                startPosition,
                curve
            );

            yield return null;
        }

        shotgunTransform.localPosition = startPosition;

        recoilCoroutine = null;
    }
    private void PlayHitSound()
    {
        if (audioSource == null || hitSound == null)
            return;

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(hitSound, hitVolume);
    }
    private void PlayShotgunRecoil()
    {
        if (shotgunTransform == null)
            return;

        if (recoilCoroutine != null)
            StopCoroutine(recoilCoroutine);

        recoilCoroutine = StartCoroutine(ShotgunRecoilRoutine());
    }

    private IEnumerator ShotgunRecoilRoutine()
    {
        Vector3 startPosition = shotgunStartPosition;

        // Направление отдачи — назад относительно ружья.
        Vector3 recoilDirection = -shotgunTransform.right;

        Vector3 recoilPosition =
            startPosition + recoilDirection * recoilDistance;

        // Резкий отскок назад.
        float timer = 0f;

        while (timer < recoilDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / recoilDuration);

            // Быстрый резкий отскок.
            float curve = 1f - Mathf.Pow(1f - progress, 3f);

            shotgunTransform.localPosition = Vector3.Lerp(
                startPosition,
                recoilPosition,
                curve
            );

            yield return null;
        }

        // Возвращение в исходное положение.
        timer = 0f;

        while (timer < returnDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / returnDuration);

            // Плавное возвращение.
            float curve = 1f - Mathf.Pow(1f - progress, 3f);

            shotgunTransform.localPosition = Vector3.Lerp(
                recoilPosition,
                startPosition,
                curve
            );

            yield return null;
        }

        shotgunTransform.localPosition = startPosition;

        recoilCoroutine = null;
    }
    private IEnumerator ShotgunFlyAwayRoutine()
    {
        Vector3 startPosition = shotgunTransform.position;
        Quaternion startRotation = shotgunTransform.rotation;

        // Выбираем случайное направление.
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        Vector3 flyDirection = new Vector3(
            randomDirection.x,
            randomDirection.y,
            0f
        );

        // Случайное количество оборотов и направление вращения.
        float spinTurns = Random.Range(minSpinTurns, maxSpinTurns);

        float spinDirection = Random.value < 0.5f ? -1f : 1f;

        float timer = 0f;

        while (timer < shotgunFlyDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                timer / shotgunFlyDuration
            );

            // Постепенно ускоряем полёт.
            float movementProgress = progress * progress;

            shotgunTransform.position =
                startPosition +
                flyDirection * shotgunFlyDistance * movementProgress;

            // Вращаем ружьё.
            float rotation = spinDirection * spinTurns * 360f * progress;

            shotgunTransform.rotation =
                startRotation * Quaternion.Euler(0f, 0f, rotation);

            yield return null;
        }

        // Финальное положение.
        shotgunTransform.position =
            startPosition + flyDirection * shotgunFlyDistance;

        shotgunTransform.rotation =
            startRotation * Quaternion.Euler(
                0f,
                0f,
                spinDirection * spinTurns * 360f
            );
    }
}
