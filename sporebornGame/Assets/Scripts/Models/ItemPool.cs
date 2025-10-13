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
}