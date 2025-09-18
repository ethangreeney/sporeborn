using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyPresenter : MonoBehaviour
{
    // Stores all enemy prefabs in unity
    public List<GameObject> EnemyList;
    // Stores all boss prefabs
     public List<GameObject> BossList;

    // Tracks number of enemies in scene
    private static int EnemiesInScene;

    private MapPresenter map;
    private EnemyModel enemyModel;

    // Generate random numbers
    System.Random rng;

    bool isDead;

    void Start()
    {
        map = FindFirstObjectByType<MapPresenter>();
        rng = new System.Random();
    }

    public void SpawnBoss(GameObject CurrentRoomInstance, Room CurrentRoom)
    {

    }

    public void EnemyDies()
    {
        EnemiesInScene--;
        // Unlocks door once all enemies are defeated
        if (EnemiesInScene == 0)
        {
            map.ToggleLockDoors(false);
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
}