using UnityEngine;

public class PlayerActivatableItem : MonoBehaviour
{
    public ActivatableItem equippedItem;
    public int currentCharges = 0;

    public System.Action<int, int> OnChargeChanged; // For UI updates
    public System.Action<ActivatableItem> OnItemEquipped; 

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

    }

    public void AddCharge(int amount = 1)
    {
        if (equippedItem == null) return;
        currentCharges = Mathf.Min(currentCharges + amount, equippedItem.maxCharges);
        OnChargeChanged?.Invoke(currentCharges, equippedItem.maxCharges);
    }

    //CALL THIS TO EQUIP A NEW ITEM, RESETTING CHARGES AND UPDATING UI
    public void EquipItem(ActivatableItem newItem)
    {
        equippedItem = newItem;
        currentCharges = 0;
        OnItemEquipped?.Invoke(equippedItem);
        OnChargeChanged?.Invoke(currentCharges, equippedItem != null ? equippedItem.maxCharges : 0);
    }
}
