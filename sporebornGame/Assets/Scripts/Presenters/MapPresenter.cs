using System;
using System.Collections.Generic;
using UnityEngine;

public class MapPresenter : MonoBehaviour
{
    private MapModel model;

    private List<Cell> SpawnedCells;
    public TextAsset tileMapJsonFile;

    [Header("Room Prefabs")]
    public List<GameObject> RoomPrefabs;

    [Header("Player")]
    PlayerModel player;

    void Start()
    {
        // Generates new map
        model = new MapModel(10, 20);

        // Gets the list of rooms
        SpawnedCells = model.GetCellList;

        // Generates the first room
        Cell StarterRoom = SpawnedCells[model.GetStartingRoomIndex];
        BuildRoom(StarterRoom.RoomShape, StarterRoom.RoomType);
    }

    // Creates a new room and correctly positions the player
    public void BuildRoom(RoomShape shape, RoomType type)
    {
        // Place room
        String RoomName = shape + "_" + type;
        Instantiate(RoomPrefabs.Find(RoomName), Vector3.zero, Quaternion.identity);

        // Player location will be based on the door they enter from
        player.setLocation(PlayerSpawnLocation());
    }

    public Vector3 PlayerSpawnLocation()
    {
        Vector3 PlayerLocation = new();

        // If we are spawning in the starter room then player spawns in centre
        if (true)
        {
            PlayerLocation = Vector3.zero;
        }

        return PlayerLocation;
    }

    public void GetRoomPrefab(RoomShape shape)
    {
        // switch(shape){
        //     OnebyOne:
        // }
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