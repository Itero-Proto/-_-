
using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;

    private Vector2 direction;

    private bool isReflected;
    private bool hasHit;

    private PlayerParry playerParry;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerParry = FindFirstObjectByType<PlayerParry>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();
    }
    public void Launch(Vector2 targetPosition, float projectileSpeed)
    {
        speed = projectileSpeed;

        direction = (targetPosition - (Vector2)transform.position).normalized;

        // Поворачиваем лазер по направлению движения
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void Update()
    {
        if (hasHit)
            return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        if (!isReflected && other.CompareTag("Player"))
        {
            if (playerParry != null && playerParry.IsParrying)
            {
                bool isPerfect = playerParry.IsPerfectParrying;

                playerParry.SuccessfulParry(isPerfect);
                Reflect();
            }
            else
            {
                hasHit = true;

                if (playerHealth != null)
                    playerHealth.TakeDamage();

                Destroy(gameObject);
            }

            return;
        }

        if (isReflected && other.CompareTag("SecurityCamera"))
        {
            hasHit = true;

            // Находим конкретную камеру, в которую попал лазер.
            SecurityCamera hitCamera =
                other.GetComponentInParent<SecurityCamera>();

            // Проигрываем визуальный эффект попадания.
            if (hitCamera != null)
                hitCamera.PlayHitEffect();

            // Проверяем, принадлежит ли камера боссу.
            BossController boss =
                other.GetComponentInParent<BossController>();

            if (boss != null)
            {
                boss.TakeDamage();
            }
            else
            {
                CameraHealth cameraHealth =
                    other.GetComponent<CameraHealth>();

                if (cameraHealth != null)
                    cameraHealth.TakeDamage();
            }

            Destroy(gameObject);
        }
    }
    private void Reflect()
    {
        isReflected = true;
        direction = -direction;


        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.color = Color.cyan;

        if (GameFeel.Instance != null)
            GameFeel.Instance.HitStop();

        Debug.Log("ОТЛИЧНОЕ ПАРИРОВАНИЕ! +20 ВЫНОСЛИВОСТИ!");
    }
}
