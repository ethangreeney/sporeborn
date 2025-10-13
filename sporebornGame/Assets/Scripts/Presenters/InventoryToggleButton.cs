using UnityEngine;
using UnityEngine.UI;

public class InventoryToggleButton : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Sprite chestClosed;
    public Sprite chestOpen;
    public Image img;
    private bool isOpen;

    void Awake()
    {
        inventoryPanel.SetActive(false);
        img.sprite = chestClosed;
        isOpen = false;
    }
    public void Toggle()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        img.sprite = isOpen ? chestOpen : chestClosed;
    }
}