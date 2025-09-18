using UnityEngine;
using System.Collections;
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
        if (EnemiesInScene == 0)
        {
            map.RoomCompleted();
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
        List<Vector3> SpawnableTiles = map.GetSpawnLocations();
        // Resets every new room
        EnemiesInScene = 0;

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