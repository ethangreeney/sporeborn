using UnityEngine;

public class PlayerShootingPresenter : MonoBehaviour
{
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public float projectileDamage = 1;
    public float projectileLifetime = 2f;
    public float fireRate = 0.2f; 
    
    private float fireCooldown = 0f;

    public float projectileSize = 1f; // 1 = normal size

    [Header("Homing Settings")]
    public bool homingEnabled;
    public float homingStrength; // How fast bullets curve toward target

    [Header("Shot Multipliers")]
    public int projectileCount = 1;      // how many bullets to shoot at once
    public float spreadAngle = 15f;      // angle between bullets
    public float damageBonus = 0f; // e.g., 0.5 = +50% damage

    [Header("Rubber Bullets Settings")]
    public bool rubberEnabled = false;  // Does the player have rubber bullets?
    public int bounceCount = 1;         // How many bounces each projectile can have

    [Header("Slow Effect Settings")]
    public bool slowOnHitEnabled = false;
    public float slowMultiplier = 1f;
    public float slowDuration = 0f;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (Input.GetMouseButton(0) && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = fireRate;
        }
    }

    void Shoot()
{
    Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    Vector2 dir = (mouse - firePoint.position).normalized;
    float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

    for (int i = 0; i < projectileCount; i++)
    {
        float angleOffset = 0f;

        if (projectileCount > 1)
        {
            float totalSpread = spreadAngle * (projectileCount - 1);
            angleOffset = -totalSpread / 2f + (i * spreadAngle);
        }

        Quaternion rot = Quaternion.AngleAxis(baseAngle + angleOffset, Vector3.forward);
        var go = Instantiate(projectilePrefab, firePoint.position, rot);

        // Apply size scaling
        go.transform.localScale *= projectileSize;

        var proj = go.GetComponent<ProjectilePresenter>();

        if (proj != null)
        {
            proj.speed = projectileSpeed;
            proj.damage = projectileDamage * (1f + damageBonus)/ projectileCount;
            proj.lifetime = projectileLifetime;

            // Homing
            proj.enableHoming = homingEnabled;
            proj.homingStrength = homingStrength;

            // Rubber bullets
            proj.rubberEnabled = rubberEnabled;
            proj.bounceCount = bounceCount;

            proj.originalDirection = (rot * Vector3.right).normalized; // direction of bullet

            // Slow effect
            proj.slowOnHitEnabled = slowOnHitEnabled;
            proj.slowMultiplier = slowMultiplier;
            proj.slowDuration = slowDuration;
            }
    }
}


}