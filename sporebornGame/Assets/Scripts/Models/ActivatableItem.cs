using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Items/ActivatableItem")]
public class ActivatableItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxCharges = 6;

    public enum ActivatableItemType { None, Forcefield, Octoshot }
    public ActivatableItemType itemType;

    public float duration;
    
    
    

    public virtual void Activate(GameObject user)
    {
        var presenter = user.GetComponent<PlayerPresenter>();
        switch (itemType)
        {
            case ActivatableItemType.Forcefield:
                if (presenter != null)
                    presenter.StartCoroutine(ApplyForcefield(presenter));
                break;
            case ActivatableItemType.Octoshot:
                FireOctoshot(user);
                break;
                // ...other cases
        }
    }

    private IEnumerator ApplyForcefield(PlayerPresenter presenter)
    {
        float blinkDuration = 3f;
        float blinkRate = 0.2f;
        float totalDuration = duration;
        float solidTime = Mathf.Max(0, totalDuration - blinkDuration);

        presenter.SetInvulnerable(true);

        
        if (presenter.forcefieldRenderer != null)
            presenter.forcefieldRenderer.enabled = true;

        yield return new WaitForSeconds(solidTime);

        // Blinking for the last blinkDuration seconds
        float blinkEndTime = Time.time + blinkDuration;
        bool visible = false;
        while (Time.time < blinkEndTime)
        {
            visible = !visible;
            if (presenter.forcefieldRenderer != null)
                presenter.forcefieldRenderer.enabled = visible;
            float remaining = blinkEndTime - Time.time;
            yield return new WaitForSeconds(Mathf.Min(blinkRate, remaining));
        }

        // Ensure forcefield is hidden and invulnerability is off
        if (presenter.forcefieldRenderer != null)
            presenter.forcefieldRenderer.enabled = false;
        presenter.SetInvulnerable(false);
    }

    private void FireOctoshot(GameObject user)
    {
        // Get the player's shooting component
        var shooting = user.GetComponent<PlayerShootingPresenter>();
        if (shooting == null || shooting.projectilePrefab == null)
            return;

        // Create 8 projectiles in a circle (45 degrees apart)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f; // 0, 45, 90, 135, 180, 225, 270, 315
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            // Instantiate projectile
            var go = Object.Instantiate(shooting.projectilePrefab,
                                       shooting.firePoint.position,
                                       rotation);

            // Scale the projectile to match player's settings
            go.transform.localScale *= shooting.projectileSize;

            // Configure projectile
            var proj = go.GetComponent<ProjectilePresenter>();
            if (proj != null)
            {
                // Set the same properties as player's projectiles
                proj.speed = shooting.projectileSpeed;
                proj.damage = shooting.projectileDamage * (1f + shooting.damageBonus);
                proj.lifetime = shooting.projectileLifetime;

                // Apply special properties
                proj.enableHoming = shooting.homingEnabled;
                proj.homingStrength = shooting.homingStrength;
                proj.rubberEnabled = shooting.rubberEnabled;
                proj.bounceCount = shooting.bounceCount;

                // Set direction based on rotation
                proj.originalDirection = (rotation * Vector3.right).normalized;

                // Slow effect
                proj.slowOnHitEnabled = shooting.slowOnHitEnabled;
                proj.slowMultiplier = shooting.slowMultiplier;
                proj.slowDuration = shooting.slowDuration;

                // Set color
                proj.SetColor(shooting.projectileColor);
            }
        }
    }
}
