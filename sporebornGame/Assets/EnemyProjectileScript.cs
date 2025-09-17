using UnityEngine;

public class EnemyProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] float speed = 10f;
    [SerializeField] float lifetime = 2f;

    [Header("Collision")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] string environmentLayerName = "Environment"; // <-- your walls layer

    Vector2 direction;
    float timer;
    Rigidbody2D rb;
    int environmentLayer;

    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
        transform.right = direction;
        if (rb) rb.linearVelocity = direction * speed;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        environmentLayer = LayerMask.NameToLayer(environmentLayerName);
        if (environmentLayer == -1)
            Debug.LogError($"EnemyProjectile: Layer '{environmentLayerName}' not found. Check spelling in Project Settings > Tags and Layers.");

        // pass through enemies (redundant if matrix already disables it)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0 && gameObject.layer >= 0)
            Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, true);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("hit");
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == environmentLayer)
        {
            Debug.Log("hit environment: " + other.name);
            Destroy(gameObject);
            return;
        }
    }

    // Fallback if collider ever isn’t a trigger
    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.CompareTag(playerTag))
        {
            Debug.Log("hit");
            Destroy(gameObject);
            return;
        }
        if (c.collider.gameObject.layer == environmentLayer)
        {
            Debug.Log("hit environment (collision): " + c.collider.name);
            Destroy(gameObject);
        }
    }

}
