using UnityEngine;
using UnityEngine.UI;

public class ActivatableItemUI : MonoBehaviour
{
    public Image batteryImage;
    public Sprite[] batteryStates;
    public PlayerActivatableItem playerItem;
    public Image iconImage;

    private ActivatableItem lastEquippedItem; // Cache to track changes

    void Awake()
    {
        playerItem.OnChargeChanged += UpdateBattery;
        
        // Initial update
        UpdateBattery(playerItem.currentCharges, 
            playerItem.equippedItem != null ? playerItem.equippedItem.maxCharges : 1);
        UpdateUI();
    }

    void Update()
    {
        // Only update when equipped item changes
        if (lastEquippedItem != playerItem.equippedItem && playerItem.equippedItem != null)
        {
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        // Cache the reference to avoid repeated property access
        lastEquippedItem = playerItem.equippedItem;
        bool hasItem = lastEquippedItem != null;
        
        // Update visibility
        batteryImage.enabled = hasItem;
        
        // Update icon
        if (iconImage != null)
        {
            if (hasItem && lastEquippedItem.icon != null)
            {
                iconImage.sprite = lastEquippedItem.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
    }

    void UpdateBattery(int current, int max)
    {
        if (batteryImage != null && batteryStates.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(current, 0, batteryStates.Length - 1);
            batteryImage.sprite = batteryStates[spriteIndex];
        }
    }
}
