using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectilePresenter : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 1f;
    public float lifetime = 2f;

    [Header("Homing")]
    public bool enableHoming = false;
    public float homingStrength = 5f; // Higher = faster turning

    [Header("Bouncing Settings")]
    public bool rubberEnabled = false;
    public int bounceCount = 1;

    public Vector2 originalDirection;

    [Header("Slow Effect Settings")]
    public bool slowOnHitEnabled = false;
    public float slowMultiplier = 1f;
    public float slowDuration = 0f;

    private Rigidbody2D rb;
    private Transform target;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Make the original collider a trigger for enemies
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // Bounce collider for walls
        var bounceCollider = gameObject.AddComponent<PolygonCollider2D>();
        bounceCollider.isTrigger = false;

        // Ignore collision with player
        Collider2D playerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(bounceCollider, playerCollider);

        // Ignore collision with all enemies
        EnemyModel[] enemies = Object.FindObjectsByType<EnemyModel>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null)
                Physics2D.IgnoreCollision(bounceCollider, enemyCollider);
        }

        int projLayer = LayerMask.NameToLayer("Projectile");
        gameObject.layer = projLayer;

        // disable projectile vs projectile collisions (safe to call multiple times)
        Physics2D.IgnoreLayerCollision(projLayer, projLayer, true);

    }


    void Start()
    {
        if (enableHoming)
            target = FindClosestEnemy();

        rb.linearVelocity = originalDirection.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (enableHoming && target != null)
        {
            // Calculate new direction towards target
            Vector2 direction = ((Vector2)target.position - rb.position).normalized;

            // Smoothly rotate current velocity towards target
            Vector2 newVelocity = Vector2.Lerp(rb.linearVelocity.normalized, direction, homingStrength * Time.fixedDeltaTime);

            rb.linearVelocity = newVelocity.normalized * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponentInParent<EnemyModel>();
        if (enemy != null)
        {
            SoundManager.instance.PlayEnemyHitSound();
            enemy.TakeDamage(damage);

            // --- APPLY SLOW EFFECT ---
            if (slowOnHitEnabled)
            {
                enemy.ApplySlow(slowMultiplier, slowDuration);
            }

            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we hit a wall/obstacle
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (rubberEnabled)
            {
                bounceCount--;
                if (bounceCount <= 0)
                {
                    Destroy(gameObject);
                    return;
                }

                // Determine wall orientation based on collision normal
                Vector2 normal = collision.contacts[0].normal;

                if (Mathf.Abs(normal.x) > 0.9f)
                {
                    // Vertical wall then mirror x
                    rb.linearVelocity = new Vector2(-originalDirection.x, originalDirection.y).normalized * speed;
                    originalDirection = rb.linearVelocity.normalized;
                }
                else if (Mathf.Abs(normal.y) > 0.9f)
                {
                    // Horizontal wall then mirror y
                    rb.linearVelocity = new Vector2(originalDirection.x, -originalDirection.y).normalized * speed;
                    originalDirection = rb.linearVelocity.normalized;
                }
                else
                {
                    // Diagonal wall then fallback to normal reflect
                    rb.linearVelocity = Vector2.Reflect(originalDirection, normal).normalized * speed;
                    originalDirection = rb.linearVelocity.normalized;
                }
            }
            else
            {
                // Not rubber then destroy on first hit
                Destroy(gameObject);
            }
        }
    }



    private Transform FindClosestEnemy()
    {
        EnemyModel[] enemies = Object.FindObjectsByType<EnemyModel>(FindObjectsSortMode.None);
        Transform closest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (EnemyModel enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    public void SetColor(Color color)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = color;
    }
}
