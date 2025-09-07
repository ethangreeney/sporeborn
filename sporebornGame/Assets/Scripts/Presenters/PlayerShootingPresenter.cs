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
        Vector2 dir = (mouse - firePoint.position);
        dir.Normalize();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        var go = Instantiate(projectilePrefab, firePoint.position, Quaternion.AngleAxis(angle, Vector3.forward));


        // Apply size scaling
        go.transform.localScale *= projectileSize;

        var proj = go.GetComponent<ProjectilePresenter>();
        if (proj != null)
        {
            proj.speed = projectileSpeed;
            proj.damage = projectileDamage;
            proj.lifetime = projectileLifetime;
        }
    }
}
