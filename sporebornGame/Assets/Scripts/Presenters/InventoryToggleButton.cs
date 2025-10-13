using UnityEngine;
using UnityEngine.UI;

public class InventoryToggleButton : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Sprite chestClosed;
    public Sprite chestOpen;
    public Image img;
    private bool isOpen;

    public void Toggle()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        img.sprite = isOpen ? chestOpen : chestClosed;
    }
}