using UnityEngine;

public class HealthModel : MonoBehaviour
{
    public float maxHealth;
    public float currHealth;

    public SpriteRenderer spriteRenderer;

    public HealthModel(float maxHealth, float currHealth)
    {
        this.maxHealth = maxHealth;
        this.currHealth = currHealth;
    }

    public void Damage(float damagedAmount)
    {
        currHealth -= damagedAmount;
        if (currHealth < 0) currHealth = 0; // Prevents negative 
    }

    public void Health(float healAmount)
    {
        currHealth += healAmount;
        if (currHealth > maxHealth) currHealth = maxHealth;
        
    }

    public float GetHealthPercentage()
    {
        return (currHealth / maxHealth * 100);
    }
}
