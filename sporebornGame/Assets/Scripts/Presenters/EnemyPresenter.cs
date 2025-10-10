using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyPresenter : MonoBehaviour
{
    // Stores all enemy prefabs in unity
    public List<GameObject> EnemyList;
    // Stores all boss prefabs
    public List<GameObject> BossList;

    // Tracks number of enemies in scene
    private static int EnemiesInScene;

    private MapPresenter map;

    // Portal prefab for when boss is defeated
    public GameObject PortalPrefab;
    private GameObject activePortal;


    // List of Enemy Drops - Drag Prefabs into in unity
    public List<GameObject> EnemyDrops;

    // Keeps track of active drops per room
    private List<GameObject> ActiveDrops;

    // Distribution for the drops 
    public List<GameObject> ItemDropBucket;

    // Change of an enemy dropping an item 
    public double ItemSpawnChance = 0.5;

    // Generate random numbers
    System.Random rng;
    

    void Start()
    {
        map = FindFirstObjectByType<MapPresenter>();
        rng = new System.Random();

        // Creates list to track Enemy Drops in Scene
        ActiveDrops = new List<GameObject>();

        // Create a distribution of items that will spawn
        GameObject Nectar = EnemyDrops[0];
        GameObject Heart = EnemyDrops[1];
        // Refills everytime its empty - creates more fair randomness
        ItemDropBucket = new List<GameObject> { Heart, Heart, Nectar, Nectar, Nectar, Nectar};
    }

    public void EnemyDies(Vector3 deathPosition)
    {
        EnemiesInScene--;

        // Spawn Loot Item upon enemy death
        SpawnItem(deathPosition);

        // Unlocks door once all enemies/boss is defeated
        if (EnemiesInScene == 0)
        {
            map.ToggleLockDoors(false);
            map.RoomCompleted();

            // Only spawn item if this is the boss room
            if (map.CurrentPlayerRoom.RoomType == RoomType.Boss)
            {
                var itemPresenter = FindFirstObjectByType<ItemPresenter>();
                if (itemPresenter != null)
                {
                    itemPresenter.PlaceItemInItemRoom(map.CurrentPlayerRoom);
                }
                else
                {
                    Debug.LogWarning("ItemPresenter not found in scene.");
                }

                // Spawn portal to exit level
                if (PortalPrefab != null)
                {
                    SpawnPortal();
                }
                else
                {
                    Debug.LogWarning("PortalPrefab is not assigned in the EnemyPresenter.");
                }
            }
        }
    }

    public void SpawnEnemies(GameObject CurrentRoomInstance, Room CurrentRoom)
    {
        List<Vector3> SpawnableTiles = map.GetSpawnLocations();
        // Resets every new room
        EnemiesInScene = 0;

        int MaxEnemies = NumberOfEnemies(CurrentRoom);

        for (int i = 0; i < MaxEnemies; i++)
        {
            int RandomSpawnLocation = rng.Next(0, SpawnableTiles.Count);
            int RandomEnemyType = rng.Next(0, EnemyList.Count);
            // Temp just picks first enemy type from list
            Instantiate(EnemyList[RandomEnemyType], SpawnableTiles[RandomSpawnLocation], Quaternion.identity);
            // Prevents enemies from spawning at same location
            SpawnableTiles.RemoveAt(RandomSpawnLocation);

            // Add to list so can track number of enemies
            EnemiesInScene++;
        }
    }

    // Picks a random number of enemies to spawn based on the size of the room
    private int NumberOfEnemies(Room CurrentRoom)
    {
        // Based of how many indexes the room takes up on FloorPlan
        switch (CurrentRoom.OccupiedIndexes.Count)
        {
            case 1:
                return rng.Next(3, 5);

            case 2:
                return rng.Next(4, 8);

            case 3:
                return rng.Next(4, 10);

            case 4:
                return rng.Next(5, 12);
        }

        Debug.Log("Room has invalid number of occupied Indexes");
        return 1;
    }

    public void SpawnBoss(GameObject CurrentRoomInstance, Room CurrentRoom)
    {
        EnemiesInScene = 1;
        Instantiate(BossList[0], Vector3.zero, Quaternion.identity);
    }

    public void RemovePortal()
    {
        if (activePortal != null)
        {
            Destroy(activePortal);
            activePortal = null;
        }
    }

    public void SpawnPortal()
    {
        Debug.Log($"SpawnPortal called. PortalPrefab: {PortalPrefab}, activePortal: {activePortal}");
        if (PortalPrefab != null && activePortal == null)
        {
            Vector3 portalPosition = new Vector3(0, 4, 0);
            activePortal = Instantiate(PortalPrefab, portalPosition, Quaternion.identity);
            Debug.Log("Portal spawned.");
        }
        else
        {
            Debug.LogWarning("Portal not spawned. Either PortalPrefab is null or activePortal is not null.");
        }
    }


    // Handles All enemy drops ------------------------------------------------------------------

    // Triggered on enemy death
    public void SpawnItem(Vector3 EnemyPosition)
    {
        // Chance to fail to spawn item
        if(rng.NextDouble() < ItemSpawnChance)
        {
            return;
        }

        // Picks a RandomItem from ItemBucket
        GameObject RandomEnemyDrop = ItemDropBucket[rng.Next(0, EnemyDrops.Count-1)];

        ItemDropBucket.Remove(RandomEnemyDrop);

        // Refill Item Bucket if empty
        if (ItemDropBucket.Count < 0)
        {
            RefillItemBucket();
        }

        // Adds to list of active items 
        ActiveDrops.Add(Instantiate(RandomEnemyDrop, EnemyPosition, Quaternion.identity));
    }

    // Destroys Specific item
    public void DestroyItem(GameObject CurrentDrop)
    {
        Destroy(CurrentDrop);
        ActiveDrops.Remove(CurrentDrop);
    }

    // Destroys all items dropped by Enemies
    public void DestroyAllItems()
    {
        foreach (GameObject drop in ActiveDrops)
        {
            Destroy(drop);
        }
        ActiveDrops.Clear();
    }


    // After all items are spawned then creates a new bucket of same distribution to create more reliable randomness
    // This means that the player can't be super unlucky and only get one type of item for an extended time
    public void RefillItemBucket()
    {
        GameObject Nectar = EnemyDrops[0];
        GameObject Heart = EnemyDrops[1];
        ItemDropBucket = new List<GameObject> { Heart, Heart, Nectar, Nectar, Nectar, Nectar};
    }

}