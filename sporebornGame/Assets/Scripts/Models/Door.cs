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
        Debug.Log(AdjacentCellIndex);
        Debug.Log(ConnectingRoom);
        // Build next room
        map.BuildRoom(ConnectingRoom, this);
    }

    public Room FindAdjacentRoom()
    {
        // Only adds to adjacent rooms if they are not part of the same room
        Room CurrentRoom = map.CurrentPlayerRoom;

        int relativeDoorX = RelativeDoorPosition[0];
        int relativeDoorY = RelativeDoorPosition[1];

        switch (CurrentDoorType)
        {
            // Up Checka
            case DoorType.North:
                if (map.ValidCellNeighbour(
                    CurrentRoom, relativeDoorX, relativeDoorY - 1
                ) == false)
                {
                    return null;
                }
                AdjacentCellIndex = CurrentRoom.Index - 10;
                return map.FindRoom(AdjacentCellIndex);

            // Down Check
            case DoorType.South:
                if (map.ValidCellNeighbour(
                    CurrentRoom, relativeDoorX, relativeDoorY + 1
                ) == false)
                {
                    return null;
                }
                AdjacentCellIndex = CurrentRoom.Index + 10;
                return map.FindRoom(AdjacentCellIndex);

            // Left Check
            case DoorType.West:
                if (map.ValidCellNeighbour(
                    CurrentRoom, relativeDoorX - 1, relativeDoorY 
                ) == false)
                {
                    return null;
                }
                AdjacentCellIndex = CurrentRoom.Index - 1;
                return map.FindRoom(AdjacentCellIndex);

            // Right Check
            case DoorType.East:
                if (map.ValidCellNeighbour(
                    CurrentRoom, relativeDoorX + 1, relativeDoorY
                ) == false)
                {
                    return null;
                }
                AdjacentCellIndex = CurrentRoom.Index + 1;
                return map.FindRoom(AdjacentCellIndex);
        }

        return null;
    }

    public Vector3 GetPositionDoor()
    {
        return this.transform.position;
    }
}