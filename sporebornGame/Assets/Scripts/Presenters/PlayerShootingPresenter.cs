using UnityEngine;

public class PlayerShootingPresenter : MonoBehaviour
{
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public int projectileDamage = 1;
    public float projectileLifetime = 2f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = (mouse - firePoint.position);
            dir.Normalize();
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            var go = Instantiate(projectilePrefab, firePoint.position, Quaternion.AngleAxis(angle, Vector3.forward));
            var proj = go.GetComponent<ProjectilePresenter>();
            if (proj != null)
            {
                proj.speed = projectileSpeed;
                proj.damage = projectileDamage;
                proj.lifetime = projectileLifetime;
            }
        }
    }
}