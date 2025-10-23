using UnityEngine;

public class PetFollower : MonoBehaviour
{
    private GameObject player;
    private Vector2 currentOffsetDir = Vector2.left;
    private Vector2 targetOffsetDir = Vector2.left;
    private Rigidbody2D rb;

    public float minIdleDistance = 0.5f;
    public float maxFollowDistance = 2f;
    public float offsetSmoothSpeed = 5f;
    public float followSpeed = 8f;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public float shootInterval = 3f;
    private float shootTimer = 0f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(horizontal, vertical);

        if (moveInput.magnitude > 0.01f)
        {
            targetOffsetDir = moveInput.normalized;
        }

        currentOffsetDir = Vector2.Lerp(currentOffsetDir, targetOffsetDir, Time.fixedDeltaTime * offsetSmoothSpeed);

        float distance = (moveInput.magnitude > 0.01f)
            ? Mathf.Lerp(minIdleDistance, maxFollowDistance, moveInput.magnitude)
            : minIdleDistance;

        Vector2 targetPos = (Vector2)player.transform.position - currentOffsetDir * distance;
        Vector2 direction = targetPos - rb.position;

        rb.linearVelocity = direction.normalized * followSpeed;

        if (direction.magnitude < 1.5f)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Shooting logic
        shootTimer += Time.fixedDeltaTime;
        if (shootTimer >= shootInterval)
        {
            shootTimer = 0f;
            ShootAtClosestEnemy();
        }
    }

    void ShootAtClosestEnemy()
    {
        var enemy = FindClosestEnemy();
        if (enemy == null) return;

        Vector2 shootDir = ((Vector2)enemy.position - rb.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var presenter = proj.GetComponent<ProjectilePresenter>();
        if (presenter != null)
        {
            presenter.originalDirection = shootDir;
        }
    }

    Transform FindClosestEnemy()
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
}