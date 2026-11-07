
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

        Debug.Log(
            "Хозяин фермы получил урон! Осталось HP: " + currentHealth
        );

        if (currentHealth <= 0)
            DefeatOwner();
    }

    private void DefeatOwner()
    {
        if (isDefeated)
            return;

        isDefeated = true;

        StopAllCoroutines();

        if (damageEffects != null)
            damageEffects.PlayDestroyed();

        // Отключаем столкновения владельца.
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
            col.enabled = false;

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
}
