using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyDropPresenter : MonoBehaviour
{

    private System.Random rng;
    public List<GameObject> ActiveDrops;

    void Start()
    {

    }

    // Triggered on enemy death
    public void SpawnItem(List<GameObject> EnemyItemPool, Vector3 EnemyPosition)
    {
        
        // Randomly picks an item from Item pool to try and spawn
        foreach (GameObject RandomEnemyDrop in EnemyItemPool.OrderBy(index => rng.Next()))
        {
            DropModel item = RandomEnemyDrop.GetComponent<DropModel>();

            // Item won't spawn if random number is not within drop chance
            if (rng.NextDouble() > item.DropChance)
            {
                return;
            }

            // Adds to list of active items 
            ActiveDrops.Add(Instantiate(RandomEnemyDrop, EnemyPosition, Quaternion.identity));
            return;
        }

    }

    public void DestroyItem(GameObject CurrentDrop)
    {
        Destroy(CurrentDrop);
        ActiveDrops.Remove(CurrentDrop);
    }

    // Called when Player leaves room
    public void DestroyAllItems()
    {
        foreach (GameObject drop in ActiveDrops)
        {
            Destroy(drop);
        }
        ActiveDrops.Clear();
    }
}
