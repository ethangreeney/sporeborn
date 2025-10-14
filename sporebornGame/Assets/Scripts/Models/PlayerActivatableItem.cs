using UnityEngine;

public class PlayerActivatableItem : MonoBehaviour
{
    public ActivatableItem equippedItem;
    public int currentCharges = 0;

    public System.Action<int, int> OnChargeChanged; // For UI updates

    void Update()
    {
        if (equippedItem != null && Input.GetKeyDown(KeyCode.Space))
        {
            if (currentCharges >= equippedItem.maxCharges)
            {
                equippedItem.Activate(gameObject);
                currentCharges = 0;
                OnChargeChanged?.Invoke(currentCharges, equippedItem.maxCharges);
            }
        }
        // TEMP: Press C to add a charge for testing
        if (Input.GetKeyDown(KeyCode.C))
        {
            AddCharge(1);
        }
    }

    public void AddCharge(int amount = 1)
    {
        if (equippedItem == null) return;
        currentCharges = Mathf.Min(currentCharges + amount, equippedItem.maxCharges);
        OnChargeChanged?.Invoke(currentCharges, equippedItem.maxCharges);
    }
}
