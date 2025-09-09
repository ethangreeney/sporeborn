using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Door : MonoBehaviour
{
    [HideInInspector]
    public Room ConnectingRoom;
    [HideInInspector]
    public int AdjacentIndex;
    [HideInInspector]
    public MapPresenter map;

    public enum DoorType
    {
        North,
        East,
        South,
        West
    }

    // Manually Set Per Door
    public DoorType CurrentDoorType;
    public int[] RelativeDoorPosition = {0, 0};

    void Start()
    {
        ConnectingRoom = FindAdjacentRoom();
        InitaliseDoor();
    }

    public Door GetDoor() {
        return this;
    }

    public void InitaliseDoor()
    {
        // Toggles/Door on or off
        if (ConnectingRoom == null) {
            gameObject.SetActive(false);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Get Player here
        GameObject Player = GameObject.FindGameObjectWithTag("Player");

        // If player collides with door then Build the next room 
        // Pass through the cell and this doors position
        if (!Player) {
            return;
        }

        map.BuildRoom(ConnectingRoom, this.transform.position, this);
    }

    public Room FindAdjacentRoom()
    {
        // Only adds to adjacent rooms if they are not part of the same room
        Room CurrentRoom = map.CurrentPlayerRoom;
        Room ConnectingRoom = null;

        int relativeDoorX = RelativeDoorPosition[0];
        int relativeDoorY = RelativeDoorPosition[1];

        // Stop the code if the door doesn't exist
        switch (CurrentDoorType)
        {
            // Up Check
            case DoorType.North:
                if (map.ValidCellNeighbour(
                    CurrentRoom, relativeDoorX, relativeDoorY - 1
                ) == false)
                {
                    return null;
                }
                ConnectingRoom = map.FindRoom(CurrentRoom.Index - 10);
                break;
            // Down Check
            case DoorType.South:
                if (map.ValidCellNeighbour(
                    CurrentRoom, relativeDoorX, relativeDoorY + 1
                ) == false)
                {
                    return null;
                }
                ConnectingRoom = map.FindRoom(CurrentRoom.Index + 10);
                break;
            // Left Check
            case DoorType.East:
                if (map.ValidCellNeighbour(
                    CurrentRoom, relativeDoorX - 1, relativeDoorY
                ) == false)
                {
                    return null;
                }
                ConnectingRoom = map.FindRoom(CurrentRoom.Index - 1);
                break;

            // Right Check
            case DoorType.West:
                if (map.ValidCellNeighbour(
                    CurrentRoom, relativeDoorX, relativeDoorY + 1
                ) == false)
                {
                    return null;
                }
                ConnectingRoom = map.FindRoom(CurrentRoom.Index + 1);
                break;
        }

        return ConnectingRoom;
    }

    public Vector3 GetPositionDoor(){
        return this.transform.position;
    }


}