using System;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    public Cell ConnectingCell;
    public int AdjacentIndex;

    public enum DoorType
    {
        North,
        East,
        South,
        West
    }

    void Start()
    {
        ConnectingCell = FindAdjacentRoom();
        InitaliseDoor();
    }

    public Door GetDoor() {
        return this;
    }

    public void InitaliseDoor()
    {
        // Toggles/Door on or off
        if (ConnectingCell == null) {
            gameObject.SetActive(false);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Get Player here
        PlayerModel player = collision.GetComponent<Player>();

        // If player collides with door then Build the next room 
        // Pass through the cell and this doors position
        if (player)
        {
            BuildRoom(ConnectingCell, this.transform.position, this);
        }
    }

    public Cell FindAdjacentRoom(int index)
    {
        // Neightbour count 
        List<Cell> CellList = GetCellList();

        int row = index / 10;
        int col = index % 10;

        int up = index - 10;
        int down = index + 10;
        int left = index - 1;
        int right = index + 1;

        // Only adds to adjacent rooms if they are not part of the same room

        // Up Check
        if (!currentRoom.OccupiedIndexes.Contains(up) && (row > 0))
        {
            AdjacentIndex = FloorPlan[up];
            DoorType = North;
        }
        
        // Down Check
        if (!currentRoom.OccupiedIndexes.Contains(down) && (row < 9))
        {
            AdjacentIndex = FloorPlan[down];
            DoorType = South;
        }

        // Left Check
        if (!currentRoom.OccupiedIndexes.Contains(left) && (col > 0))
        {
            AdjacentIndex = FloorPlan[left];
            DoorType = West;
    
        }

        // Right Check
        if (!currentRoom.OccupiedIndexes.Contains(Right) && (col < 9))
        {
            AdjacentIndex = FloorPlan[right];
            DoorType = East;
        }

        foreach(Cell room in CellList){
            for(int index in room.OccupiedIndexes){
                if(index == AdjacentIndex){
                    return room;
                }
            }
        }

        return default;
    }

    public Vector3 GetPositionDoor(){
        return this.transform.position;
    }


}