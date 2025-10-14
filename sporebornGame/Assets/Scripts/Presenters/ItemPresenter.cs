using System.Linq;

using UnityEngine;

public class ItemPresenter : MonoBehaviour
{
    public ItemPool itemPool;
    public PlayerInventory inventory;
    private GameObject activeItem;

    public void PlaceItemInItemRoom(Room room)
    {
        if (room.itemCollected) return;
        if (room.assignedItemPrefab == null) room.assignedItemPrefab = PickRandomItem();
        if (room.assignedItemPrefab == null) return;

        activeItem = Instantiate(room.assignedItemPrefab, Vector3.zero, Quaternion.identity);
        activeItem.GetComponent<CollectionModel>().room = room;
    }

    public void RemoveItemFromRoom()
    {
        if (activeItem != null) Destroy(activeItem);
    }

    private GameObject PickRandomItem()
    {
        return itemPool.itemPrefabs
        .Where(item => !inventory.HasCollected(item.GetComponent<CollectionModel>().item.itemName))
        .OrderBy(_ => Random.value)
        .FirstOrDefault();
    }

    public void NotifyItemCollected(Room room) => room.itemCollected = true;
}
