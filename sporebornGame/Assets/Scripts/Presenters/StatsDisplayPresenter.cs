using UnityEngine;

public class StatsDisplayPresenter : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("Stat Icons")]
    public StatIcon speedIcon;
    public StatIcon fireRateIcon;
    public StatIcon bulletSpeedIcon;
    public StatIcon damageIcon;

    [Header("Icon Sprites")]
    public Sprite speedSprite;
    public Sprite fireRateSprite;
    public Sprite bulletSpeedSprite;
    public Sprite damageSprite;

    void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateDisplay;
        }
    }

    void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateDisplay;
        }
    }

    void Start()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        // Speed
        if (playerStats.IsMoveSpeedModified())
        {
            speedIcon.Show();
            bool increased = playerStats.currentMoveSpeed > playerStats.baseMoveSpeed;
            speedIcon.Setup(speedSprite, true, increased);
        }
        else
        {
            speedIcon.Hide();
        }

        // Fire Rate (lower is better, so logic is inverted)
        if (playerStats.IsFireRateModified())
        {
            fireRateIcon.Show();
            bool increased = playerStats.currentFireRate < playerStats.baseFireRate;
            fireRateIcon.Setup(fireRateSprite, true, increased);
        }
        else
        {
            fireRateIcon.Hide();
        }

        // Bullet Speed
        if (playerStats.IsProjectileSpeedModified())
        {
            bulletSpeedIcon.Show();
            bool increased = playerStats.currentProjectileSpeed > playerStats.baseProjectileSpeed;
            bulletSpeedIcon.Setup(bulletSpeedSprite, true, increased);
        }
        else
        {
            bulletSpeedIcon.Hide();
        }

        // Damage
        if (playerStats.IsDamageModified())
        {
            damageIcon.Show();
            bool increased = playerStats.currentDamage > playerStats.baseDamage;
            damageIcon.Setup(damageSprite, true, increased);
        }
        else
        {
            damageIcon.Hide();
        }
    }
}