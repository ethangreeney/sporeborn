using UnityEngine;

public class HealthModel : MonoBehaviour
{
    public int maxHealth;
    public int currHealth;

    public SpriteRenderer spriteRenderer;

    public HealthModel(int maxHealth, int currHealth)
    {
        this.maxHealth = maxHealth;
        this.currHealth = currHealth;
    }

    public void Damage(int damagedAmount)
    {
        currHealth -= damagedAmount;
        if (currHealth < 0) currHealth = 0; // Prevents negative 
    }

    public void Health(int healAmount)
    {
        currHealth += healAmount;
        if (currHealth > maxHealth) currHealth = maxHealth;
        
    }

    public int GetHealthPercentage()
    {
         return (int)((float)currHealth / maxHealth * 100);
    }
}
