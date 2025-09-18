using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class EnemyPresenter : MonoBehaviour
{
    public HealthModel health;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    // Stores all enemy prefabs in unity
    public List<GameObject> EnemyList;
    // Tracks number of enemies in scene
    private static int EnemiesInScene;

    private MapPresenter map;
    private Tilemap FloorTilemap;
    private Grid RoomGrid;

    // Generate random numbers
    System.Random rng;

    bool isDead;

    void Awake()
    {
        rng = new System.Random();
        map = FindFirstObjectByType<MapPresenter>();

        if (health == null) health = GetComponent<HealthModel>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (health != null && health.currHealth <= 0)
        {
            health.currHealth = health.maxHealth > 0 ? health.maxHealth : 1;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || health == null) return;

        health.Damage(amount);
        if (spriteRenderer != null) StartCoroutine(HitFlash());

        if (health.currHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitFlash()
    {
        // conflicts with boss charging animation (to be reconsidered)
        var original = spriteRenderer.color;
        spriteRenderer.color = Color.black;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }

    void Die()
    {
        isDead = true;

        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        // fall back as there is currently no death animatior
        else
        {
            Destroy(gameObject, 0.2f);
        }

        // Decrease number of enemies
        EnemiesInScene--;

        // Unlock doors if no more enemies left
        Debug.Log("Enemies in Scene: "+EnemiesInScene);
        if (EnemiesInScene == 0)
        {
            Debug.Log("No More enemies in scene");
            map.ToggleLockDoors(false);
        }
    }
    // currently unreachable code
    public void OnDeathAnimationComplete()
    {
        Destroy(gameObject);
    }

    public void SpawnEnemies(GameObject CurrentRoomInstance, Room CurrentRoom)
    {
        FloorTilemap = GetFloorTileLayer(CurrentRoomInstance);
        List<Vector3> SpawnableTiles = new List<Vector3>();
        // Resets every new room
        EnemiesInScene = 0;

        if (FloorTilemap == null)
        {
            Debug.LogWarning("Can't find Floor Layer");
            return;
        }

        BoundsInt roomBounds = FloorTilemap.cellBounds;

        foreach (Vector3Int pos in roomBounds.allPositionsWithin)
        {
            if (FloorTilemap.HasTile(pos))
            {
                // Converts from relative pos in grid to world position in scene
                Vector3 WorldlPos = FloorTilemap.CellToWorld(pos);

                // Spawns enemies in the centre of tiles
                WorldlPos += FloorTilemap.cellSize / 2f;

                // Adds to Vector3 list to tell enemies where they can spawn
                SpawnableTiles.Add(WorldlPos);
            }
        }

        int MaxEnemies = NumberOfEnemies(CurrentRoom);

        for (int i = 0; i < MaxEnemies; i++)
        {
            int index = rng.Next(0, SpawnableTiles.Count);
            // Temp just picks first enemy type from list
            Instantiate(EnemyList[0], SpawnableTiles[index], Quaternion.identity);
            // Prevents enemies from spawning at same location
            SpawnableTiles.RemoveAt(index);

            // Add to list so can track number of enemies
            EnemiesInScene++;
        }

        Debug.Log("Total Enemies in Scene "+EnemiesInScene);

    }

    private Tilemap GetFloorTileLayer(GameObject CurrentRoomInstance)
    {
        if (CurrentRoomInstance == null)
        {
            Debug.Log("Current Room Instance is null");
        }

        // Find the Grid Object which stores all layers in RoomPrefabs
        RoomGrid = CurrentRoomInstance.GetComponentInChildren<Grid>();

        if (RoomGrid != null)
        {
            Debug.LogWarning("Found Room Grid");
            Transform FloorTileMapTransform = RoomGrid.transform.Find("FloorLayer");

            if (FloorTileMapTransform != null)
            {
                Debug.LogWarning("Found Floor Transform");
                Tilemap FloorTileMap = FloorTileMapTransform.GetComponent<Tilemap>();

                if (FloorTileMap == null)
                {
                    Debug.LogWarning("Can't find floor tile map for spawning enemies");
                    return default;
                }

                return FloorTileMap;

            }
        }
        Debug.LogWarning("Can't Find Room Grid");
        // Can't find grid or floorplan transform
        return default;
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

        Debug.Log("Room has incorrect number of occupied Indexes");
        return 1;
    }
}