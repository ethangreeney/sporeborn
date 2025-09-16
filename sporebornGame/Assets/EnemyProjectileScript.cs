using UnityEngine;

public class EnemyProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] float speed = 10f;
    [SerializeField] float lifetime = 2f;

    // set by shooter
    Vector2 direction;
    float timer;

    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
        transform.right = direction;          // face travel direction (visual)
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

}
