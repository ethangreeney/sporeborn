using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu]
public class ItemPool : ScriptableObject
{
    public List<GameObject> itemPrefabs;

    public IEnumerable<Item> GetItems()
    {
        foreach (var prefab in itemPrefabs)
            yield return prefab.GetComponent<CollectionModel>().item;
    }

    public GameObject GetPrefab(string itemName)
    {
        foreach (var prefab in itemPrefabs)
        {
            var item = prefab.GetComponent<CollectionModel>().item;
            if (item.itemName == itemName) return prefab;
        }
        return null;
    }
}