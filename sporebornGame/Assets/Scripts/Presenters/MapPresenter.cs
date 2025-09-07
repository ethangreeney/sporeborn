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

    Vector3 LastDoorHit;

    [Header("Player")]
    GameObject Player;


    void Start()
    {
        // Generates first level map
        model = new MapModel(10, 20);

        // Gets the list of rooms
        SpawnedCells = model.GetCellList;

        // Get the player
        Player = GameObject.FindGameObjectWithTag("Player");

        // Generates the first room
        Cell StarterRoom = SpawnedCells[model.GetStartingRoomIndex];
        BuildRoom(StarterRoom, Vector3.zero, null);
    }

    // Creates a new room and correctly positions the player
    public void BuildRoom(Cell RoomCell, Vector3 PlayerSpawnPosition, Door EnterDoor)
    {
        // Place room
        String RoomName = RoomCell.RoomShape + "_" + RoomCell.RoomType;
        GameObject CurrentRoomPrefab;

        foreach (GameObject prefab in RoomPrefabs)
        {
            if (prefab.name == RoomName)
            {
                CurrentRoomPrefab = prefab;
            }
        }
        // Spawns in room from prefab
        GameObject RoomInstance = Instantiate(CurrentRoomPrefab, PlayerSpawnPosition, Quaternion.identity);

        // Doesn't need offset as if it is the first room
        if (PlayerSpawnPosition != Vector3.zero)
        {
            Vector3 OffsetPosition = GetPlayerOffset(PlayerSpawnPosition, RoomCell, AdjacentCellIndex);
        }

        // Player location will be based on the door they enter from
        Player.transform.position = PlayerSpawnPosition;
    }

    public Vector3 GetPlayerOffset(Vector3 DoorPosition, Cell RoomCell, int AdjacentCellIndex) {
       
        Door CurrentDoor = Door.GetDoor();

        int RoomAdjacentIndex;
        foreach(int index in RoomCell.OccupiedIndexes){
            if(index == CurrentDoor.AdjacentIndex){
                RoomAdjacentIndex = index;
                break;
            }
        }
        // asdsad 
        Door NextRoomDoor = RoomAdjacentIndex;
        
        Vector3 PlayerOffsetPosition = NextRoomDoor.GetPositionDoor();

        // Flip Coordinates of door on that segment
        if(NextRoomDoor.DoorType == North){
            PlayerOffsetPosition.y *= -1;
        }
        if(NextRoomDoor.DoorType == East){
            PlayerOffsetPosition.x *= -1;
        }
        if(NextRoomDoor.DoorType == South){
            PlayerOffsetPosition.y *= -1;
        }
        if(NextRoomDoor.DoorType == West){
            PlayerOffsetPosition.x *= -1;
        }

        int offset = 10;

        return PlayerOffsetPosition;
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