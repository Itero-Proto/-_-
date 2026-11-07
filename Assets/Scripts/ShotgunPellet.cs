
using UnityEngine;

public class ShotgunPellet : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;

    private Vector2 direction;
    private FarmOwnerController owner;

    private bool isReflected;
    private bool hasHit;

    private Collider2D pelletCollider;
    private Collider2D[] ownerColliders;

    private void Awake()
    {
        pelletCollider = GetComponent<Collider2D>();
    }

    public void Launch(
        Vector2 shotDirection,
        float projectileSpeed,
        FarmOwnerController shooter)
    {
        owner = shooter;
        speed = projectileSpeed;
        direction = shotDirection.normalized;

        // Не даём дробинке столкнуться с владельцем при вылете.
        if (owner != null)
        {
            ownerColliders = owner.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D ownerCollider in ownerColliders)
            {
                if (ownerCollider != null && pelletCollider != null)
                {
                    Physics2D.IgnoreCollision(
                        pelletCollider,
                        ownerCollider,
                        true
                    );
                }
            }
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (hasHit)
            return;

        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        // Дробинка летит к игроку.
        if (!isReflected && other.CompareTag("Player"))
        {
            if (owner != null)
                owner.HandlePlayerHit(this);

            return;
        }

        // Отражённая дробинка возвращается владельцу.
        if (isReflected)
        {
            FarmOwnerController hitOwner =
                other.GetComponentInParent<FarmOwnerController>();

            if (hitOwner == owner && owner != null)
            {
                hasHit = true;

                owner.TakeDamage();

                Destroy(gameObject);
            }
        }
    }

    public void Reflect(Vector2 targetPosition)
    {
        if (hasHit || isReflected)
            return;

        isReflected = true;

        direction = (
            targetPosition - (Vector2)transform.position
        ).normalized;

        // Теперь дробинка может столкнуться с владельцем.
        if (ownerColliders != null)
        {
            foreach (Collider2D ownerCollider in ownerColliders)
            {
                if (ownerCollider != null && pelletCollider != null)
                {
                    Physics2D.IgnoreCollision(
                        pelletCollider,
                        ownerCollider,
                        false
                    );
                }
            }
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.color = Color.cyan;
    }

    public void DestroyPellet()
    {
        if (hasHit)
            return;

        hasHit = true;
        Destroy(gameObject);
    }
}
