using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPresenter : MonoBehaviour
{
    private MapModel model;

    private List<Room> SpawnedCells;

    [Header("Room Prefabs")]
    public List<GameObject> RoomPrefabs;

    [Header("Player")]
    GameObject Player;

    public Room CurrentPlayerRoom;


    void Start()
    {

        // Generates first level map
        model = new MapModel(10, 20);

        // Get the player
        Player = GameObject.FindGameObjectWithTag("Player");

        // Gets the list of rooms
        SpawnedCells = model.GetRoomList;

        // Generates the first room
        Room StarterRoom = SpawnedCells.First(room => room.Index == model.GetStartingRoomIndex);
        BuildRoom(StarterRoom, Vector3.zero, null);
    }

    public bool IsThereARelativeCell(Room CurrentRoom, int dx, int dy) {
        int index = CurrentRoom.Index;

        int x = index % 10;
        int y = index / 10;

        int newX = x + dx;
        int newY = y + dy;
        
        if (newX < 0 || newX >= 10) return false;
        if (newY < 0 || newY >= 10) return false;

        // Figure out the new cell index
        return model.GetFloorPlan[index] == 1 ? true : false;
    }

    // Creates a new room and correctly positions the player
    public void BuildRoom(Room CurrentRoom, Vector3 PlayerSpawnPosition, Door EnterDoor)
    {
        // Place room
        String RoomName = CurrentRoom.RoomShape + "_" + CurrentRoom.RoomType;
        GameObject CurrentRoomPrefab = null;

        foreach (GameObject prefab in RoomPrefabs)
        {
            if (prefab.name == RoomName)
            {
                CurrentRoomPrefab = prefab;
                break;
            }
        }

        // Spawns in room from prefab
        GameObject RoomInstance = Instantiate(CurrentRoomPrefab, PlayerSpawnPosition, Quaternion.identity);

        Door[] theDoorScript = RoomInstance.GetComponentsInChildren<Door>();
        foreach(Door door in theDoorScript) {
            door.map = this;
        }

        // Doesn't need offset as if it is the first room
        if (PlayerSpawnPosition != Vector3.zero)
        {
            // we'll figure this out later
        }

        // Player location will be based on the door they enter from
        Player.transform.position = PlayerSpawnPosition;

        // Update the current room the players in to the one that was just initalised
        CurrentPlayerRoom = CurrentRoom;
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