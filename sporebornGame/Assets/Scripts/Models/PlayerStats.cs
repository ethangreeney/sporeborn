using System;

using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseMoveSpeed = 5f;
    public float baseFireRate = 0.2f;
    public float baseProjectileSpeed = 12f;
    public float baseDamage = 1f;

    [Header("Current Stats")]

    public float currentMoveSpeed;
    public float currentFireRate;
    public float currentProjectileSpeed;
    public float currentDamage;

    public event Action OnStatsChanged;

    void Start()
    {
        // Initialize current stats to base values
        currentMoveSpeed = baseMoveSpeed;
        currentFireRate = baseFireRate;
        currentProjectileSpeed = baseProjectileSpeed;
        currentDamage = baseDamage;
    }

    public void NotifyStatsChanged() => OnStatsChanged?.Invoke();

    // Helper methods to check if stats changed
    public bool IsMoveSpeedModified() => !Mathf.Approximately(currentMoveSpeed, baseMoveSpeed);

    public bool IsFireRateModified() => !Mathf.Approximately(currentFireRate, baseFireRate);

    public bool IsProjectileSpeedModified() => !Mathf.Approximately(currentProjectileSpeed, baseProjectileSpeed);

    public bool IsDamageModified() => !Mathf.Approximately(currentDamage, baseDamage);
}