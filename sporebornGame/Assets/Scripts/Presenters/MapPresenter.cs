using System;
using System.Collections.Generic;
using Pathfinding.Examples;
using UnityEngine;

public class MapPresenter : MonoBehaviour
{
    private MapModel model;

    private List<Cell> SpawnedCells;
    public TextAsset tileMapJsonFile;

    [Header("Room Prefabs")]
    public List<GameObject> RoomPrefabs;

    Vector3 LastDoorHit;

    [Header("Player")]
    GameObject Player;

    void Start()
    {
        // Generates new map
        model = new MapModel(10, 20);

        // Gets the list of rooms
        SpawnedCells = model.GetCellList;

        // Generates the first room
        Cell StarterRoom = SpawnedCells[model.GetStartingRoomIndex];
        BuildRoom(StarterRoom, Vector3.zero);
    }

    // Creates a new room and correctly positions the player
    public void BuildRoom(Cell RoomCell, Vector3 PlayerSpawnPosition)
    {
        // Place room
        String RoomName = RoomCell.RoomShape + "_" + RoomCell.RoomType;
        GameObject CurrentRoomPrefab = null;

        foreach (GameObject prefab in RoomPrefabs)
        {
            if (prefab.name == RoomName)
            {
                CurrentRoomPrefab = prefab;
            }
        }
        // Spawns in room from prefab
        GameObject RoomInstance = Instantiate(CurrentRoomPrefab, PlayerSpawnPosition, Quaternion.identity);

        // Gets all doors in scene to see which is the opposite door to the previous door position
        Door[] DoorsinScene = RoomInstance.GetComponentsInChildren<Door>();
        Door entryDoor = null;

        foreach(Door)

        Vector3 OffsetPosition = GetOffset(PlayerSpawnPosition);
        // Player location will be based on the door they enter from
        Player.transform.position = PlayerSpawnPosition;
    }

    public Vector3 GetOffset(Vector3 PlayerSpawnPosition) {
        // Doesn't need offset as this is the first room
        if (PlayerSpawnPosition != Vector3.zero)
        {
            return PlayerSpawnPosition;
        }

        if()
        // Flip on X axis
        PlayerSpawnPosition.x *= -1;

        // Flip on Y axis
        PlayerSpawnPosition.y *= -1;


        int offset = 10;

        return PlayerSpawnPosition;
    }

    public void SpawnEnemies()
    {

    }

    public void SpawnBoss()
    {
        
    }

    public Vector3 IndexToCoordinate(int index)
    {
        int x = index % 10;
        int y = index / 10;
        Vector3 cooordinate = new(x, y, 0);

        return cooordinate;
    }
}