using System;
using UnityEngine;

public class HealthModel : MonoBehaviour
{
    public float maxHealth;
    public float currHealth;

    public event Action OnHealthChanged;

    public SpriteRenderer spriteRenderer;

    public HealthModel(float maxHealth, float currHealth)
    {
        this.maxHealth = maxHealth;
        this.currHealth = currHealth;
    }

    public void Damage(float damagedAmount)
    {
        currHealth -= damagedAmount;
        OnHealthChanged?.Invoke();

        if (currHealth < 0) currHealth = 0; // Prevents negative 
    }

    public void Heal(float healAmount)
    {
        currHealth += healAmount;
        if (currHealth > maxHealth) currHealth = maxHealth;
        OnHealthChanged?.Invoke();

    }

    public float GetHealthPercentage()
    {
        return (currHealth / maxHealth * 100);
    }
}
