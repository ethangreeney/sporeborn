using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Door : MonoBehaviour
{
    [HideInInspector]
    public Room ConnectingRoom;
    [HideInInspector]
    public int AdjacentCellIndex;
    [HideInInspector]
    public MapPresenter map;

    public enum DoorType
    {
        North,
        East,
        South,
        West
    }

    [HideInInspector]
    public int[][] RelPosFromOrigin = {
        new int[] {0, -6}, // Coming from North, entering from South
        new int[] {-6, 0}, // Coming from East, entering from West
        new int[] {0, 6}, // Coming from South, entering from North
        new int[] {6, 0} // Coming from West, entering from East
    };

    // Manually Set Per Door
    public DoorType CurrentDoorType;
    public int[] RelativeDoorPosition = { 0, 0 };

    void Start()
    {
        ConnectingRoom = FindAdjacentRoom();
        InitaliseDoor();
    }

    public Door GetDoor()
    {
        return this;
    }

    public void InitaliseDoor()
    {
        // Toggles/Door on or off
        if (ConnectingRoom == null)
        {
            gameObject.SetActive(false);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If player collides with door then Build the next room 
        // Pass through the cell and this doors position
        if (collision.gameObject != map.Player)
        {
            return;
        }
        Debug.Log("Connecting room is"+ConnectingRoom.RoomShape+", index is"+AdjacentCellIndex);
        // Build next room
        map.BuildRoom(ConnectingRoom, this);
    }

    public Room FindAdjacentRoom()
    {
        // Only adds to adjacent rooms if they are not part of the same room
        Room CurrentRoom = map.CurrentPlayerRoom;

        int relativeDoorX = RelativeDoorPosition[0];
        int relativeDoorY = RelativeDoorPosition[1];
        int DoorCellIndex = map.RelIndexToRoomIndex(CurrentRoom, relativeDoorX, relativeDoorY);

        if (DoorCellIndex == -1)
        {
            // Debug.Log($"Door Cell Index {DoorCellIndex} at {relativeDoorX}, {relativeDoorY} is invalid");
            return null;
        }

        int adjacentIndex = -1;
        switch (CurrentDoorType)
        {
            // Up Check
            case DoorType.North:
                adjacentIndex = DoorCellIndex - map.GRID_SIDE;
                break;

            // Down Check
            case DoorType.South:
                adjacentIndex = DoorCellIndex + map.GRID_SIDE;
                break;

            // Left Check
            case DoorType.West:
                adjacentIndex = DoorCellIndex - 1;
                break;

            // Right Check
            case DoorType.East:
                adjacentIndex = DoorCellIndex + 1;
                break;
        }

        AdjacentCellIndex = adjacentIndex;
        Room AdjacentRoom = map.FindRoom(AdjacentCellIndex);

        if (AdjacentRoom == null)
        {
            // Debug.Log("Can't find adjacent room");
            return null;
        }


        return AdjacentRoom;
    }

    public Vector3 GetPositionDoor()
    {
        return this.transform.position;
    }
}