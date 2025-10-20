using UnityEngine;

public class EnemyProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] float speed = 10f;
    [SerializeField] float lifetime = 2f;

    [Header("Damage")]
    [SerializeField] int damage = 1;                 // tweak in Inspector

    [Header("Collision")]
    [SerializeField] string playerTag = "Player";    // your player tag
    [SerializeField] string environmentLayerName = "Environment"; // your walls layer

    // runtime
    Vector2 direction;
    float timer;
    Rigidbody2D rb;
    int environmentLayer;

    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
        transform.right = direction;

        if (rb) rb.linearVelocity = direction * speed;   // physics-based movement
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        environmentLayer = LayerMask.NameToLayer(environmentLayerName);
        if (environmentLayer == -1)
            Debug.LogError($"EnemyProjectile: Layer '{environmentLayerName}' not found.");

        // Ensure we pass through enemies (also set in your layer matrix)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int myLayer = gameObject.layer; // should be EnemyProjectile
        if (enemyLayer >= 0 && myLayer >= 0)
            Physics2D.IgnoreLayerCollision(myLayer, enemyLayer, true);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    // TRIGGERS (recommended: make the projectile collider IsTrigger = ON)
    void OnTriggerEnter2D(Collider2D other)
    {
        // Player hit -> deal damage + destroy
        if (other.CompareTag(playerTag))
        {
            PlayerPresenter pp = other.GetComponent<PlayerPresenter>();
            if (!pp) pp = other.GetComponentInParent<PlayerPresenter>();
            if (pp) pp.TakeDamage(damage);
            else Debug.LogWarning("Player hit but no PlayerPresenter found on object or parent.", other);

            Destroy(gameObject);
            return;
        }

        // Environment -> destroy
        if (other.gameObject.layer == environmentLayer)
        {
            Destroy(gameObject);
            return;
        }

        // Enemies are ignored by the collision matrix / IgnoreLayerCollision
    }

    // SAFETY: if your projectile collider ever isn't a trigger, this still works.
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
