using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public Image grayout;
    private Item item;
    private bool collected;
    private InventoryPanelPresenter panel;

    public void Setup(Item item, bool collected, InventoryPanelPresenter panel)
    {
        this.item = item;
        this.collected = collected;
        this.panel = panel;

        icon.sprite = item.itemIcon;
        icon.color = collected ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);
        grayout.enabled = !collected;
    }

    public void OnPointerEnter(PointerEventData e) => panel.ShowTooltip(item, collected);
    public void OnPointerExit(PointerEventData e) => panel.HideTooltip();
}