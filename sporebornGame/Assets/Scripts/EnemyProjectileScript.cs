using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))] // e.g., CircleCollider2D (IsTrigger = true)
public class EnemyProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] float speed = 10f;
    [SerializeField] float lifetime = 2f;

    [Header("Damage")]
    [SerializeField] int damage = 1;

    [Header("Collision")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] string environmentLayerName = "Environment";

    // runtime
    Vector2 direction;
    float timer;
    Rigidbody2D rb;
    int environmentLayer;

    /// <summary>
    /// Shooter calls this after instantiating and (optionally) setting rotation.
    /// Do NOT change rotation here; shooter already oriented the root.
    /// </summary>
    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;

        // Do not re-aim here (leaves any shooter-set rotation intact).
        // transform.right = direction;

        if (rb) rb.linearVelocity = direction * speed;  // physics-based travel
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true; // never spin from physics
        }

        // Ensure trigger collider (e.g., CircleCollider2D) is set in Inspector
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;

        environmentLayer = LayerMask.NameToLayer(environmentLayerName);
        if (environmentLayer == -1)
            Debug.LogError($"EnemyProjectile: Layer '{environmentLayerName}' not found.");

        // Ignore enemies (also configure in Physics 2D Layer Matrix)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int myLayer = gameObject.layer;
        if (enemyLayer >= 0 && myLayer >= 0)
            Physics2D.IgnoreLayerCollision(myLayer, enemyLayer, true);
    }

    void OnEnable()
    {
        timer = 0f; // for pooling
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    // Trigger path (preferred)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            var pp = other.GetComponent<PlayerPresenter>() ?? other.GetComponentInParent<PlayerPresenter>();
            if (pp) pp.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == environmentLayer)
        {
            Destroy(gameObject);
            return;
        }
    }

    // Safety if collider ever isn't a trigger
    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.CompareTag(playerTag))
        {
            var pp = c.collider.GetComponent<PlayerPresenter>() ?? c.collider.GetComponentInParent<PlayerPresenter>();
            if (pp) pp.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (c.collider.gameObject.layer == environmentLayer)
        {
            Destroy(gameObject);
        }
    }
}
