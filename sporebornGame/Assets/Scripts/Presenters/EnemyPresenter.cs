using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

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

    public GameObject HeartPrefab;
    public float heartDropChance = 0.15f;

    // Track active hearts in current room
    private List<GameObject> activeHearts = new List<GameObject>();

    // Store heart positions for respawning
    private Dictionary<int, List<Vector3>> roomHeartPositions = new Dictionary<int, List<Vector3>>();

    // Generate random numbers
    System.Random rng;

    void Start()
    {
        map = FindFirstObjectByType<MapPresenter>();
        rng = new System.Random();
    }

    public void EnemyDies(Vector3 deathPosition)
    {
        EnemiesInScene--;

        if (HeartPrefab != null && Random.value < heartDropChance)
        {
            GameObject heart = Instantiate(HeartPrefab, deathPosition, Quaternion.identity);
            HeartData heartData = heart.AddComponent<HeartData>();
            heartData.roomOriginIndex = map.CurrentPlayerRoom.OriginIndex;
            activeHearts.Add(heart);

            // Save position for respawn
            int roomIndex = map.CurrentPlayerRoom.OriginIndex;
            if (!roomHeartPositions.ContainsKey(roomIndex))
                roomHeartPositions[roomIndex] = new List<Vector3>();
            roomHeartPositions[roomIndex].Add(deathPosition);
        }
        
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

    // Clear hearts when leaving room
    public void ClearHearts()
    {
        foreach (var heart in activeHearts)
        {
            if (heart != null)
                Destroy(heart);
        }
        activeHearts.Clear();
    }

    // Spawn hearts when entering room
    public void SpawnHeartsInRoom(Room room)
    {
        int roomIndex = room.OriginIndex;
        if (roomHeartPositions.ContainsKey(roomIndex))
        {
            foreach (var pos in roomHeartPositions[roomIndex])
            {
                GameObject heart = Instantiate(HeartPrefab, pos, Quaternion.identity);
                HeartData heartData = heart.AddComponent<HeartData>();
                heartData.roomOriginIndex = roomIndex;
                activeHearts.Add(heart);
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
        if (PortalPrefab != null && activePortal == null)
        {
            Vector3 portalPosition = new Vector3(0, 4, 0);
            activePortal = Instantiate(PortalPrefab, portalPosition, Quaternion.identity);
        }
    }

    public void OnHeartCollected(GameObject heart)
    {
        HeartData heartData = heart.GetComponent<HeartData>();
        if (heartData != null)
        {
            int roomIndex = heartData.roomOriginIndex;
            Vector3 pos = heart.transform.position;
            if (roomHeartPositions.ContainsKey(roomIndex))
            {
                var list = roomHeartPositions[roomIndex];
                list.RemoveAll(p => Vector3.Distance(p, pos) < 0.1f);
            }
        }
        activeHearts.Remove(heart);
    }

    public void ResetHearts()
    {
        roomHeartPositions.Clear();
        ClearHearts();
    }
}

// Simple component to track which room a heart belongs to
public class HeartData : MonoBehaviour
{
    public int roomOriginIndex;
}