using System;
using System.Collections.Generic;
using System.Linq;

public class MapModel
{
    private int[] FloorPlan;
    public int[] GetFloorPlan => FloorPlan;

    private int MinRooms;
    private int MaxRooms;

    // Locations of each of the special type rooms
    public int BossRoomIndex, ItemRoomIndex, ShopRoomIndex;

    // Generates Random numbers
    System.Random rng;

    // Tracking Parts of the map
    private Queue<int> CellQueue;
    private List<int> EndRooms;
    private List<int> LargeRoomIndexes;

    private List<Cell> CellList;
    public List<Cell> GetCellList => CellList;

    private static readonly List<int[]> RoomShapes = new()
    {
        // Configurations for the large room types
        // Values to add/subtract to get the cells for room type

        // Horiztonal Rooms
        new int[]{-1},
        new int[]{1},

        // Vertical Rooms
        new int[]{10},
        new int[]{-10},

        // L-Shaped Rooms
        new int[]{1, 10},
        new int[]{1, 11},
        new int[]{10, 11},

        new int[]{9, 10},
        new int[]{-1, 9},
        new int[]{-9, -10},

        new int[]{-1, -10},
        new int[]{1, -11},
        new int[]{-10, -11},

        // Large Square Rooms 
        new int[]{1, 10, 11},
        new int[]{1, -9, -10},
        new int[]{-1, 9, 10},
        new int[]{1, -10, -11},

    };

    public MapModel(int minRooms, int maxRooms)
    {
        this.MinRooms = minRooms;
        this.MaxRooms = maxRooms;
        CellList = new();
    }

    public void GenerateDungon()
    {
        FloorPlan = new int[100];
        ResetMapState(); // Clear previous map

        // Generation will start from this point (centre)
        CheckValidCell(45);
        // Sets up new dungeon
        SetupDungeon();
    }

    private void ResetMapState()
    {
        CellList.Clear();
    }

    private void SetupDungeon()
    {
        while (CellQueue.Count > 0)
        {
            int index = CellQueue.Dequeue();

            // Calculates x coordinate of index
            int x = index % 10;

            // If there is only one neighbouring cell then it is an EndRoom
            if (GetNeighbourCellCount(index) == 1)
                EndRooms.Add(index);
        }

        // Loops through floor plan to find number of rooms that equal 1
        int roomCount = FloorPlan.Count(c => c == 1);

        // Retry the generation if we don't meet the minRoom count
        if (roomCount < MinRooms)
        {
            SetupDungeon();
            return;
        }

        // Removes all end rooms that are large rooms from the list 
        CleanEndRoomsList();
        // Sets up Special Rooms
        SetupSpecialRooms();
    }

    private bool CheckValidCell(int index)
    {
        // Out of bounds of the map array
        if (index > FloorPlan.Length || index < 0) { return false; }

        // Greater than max room num - so can't create any more rooms
        if (FloorPlan.Length > MaxRooms) { return false; }

        // Fails if - Aleady a room at this cell || If neightbour has room || Fails to create a room 50% of the time
        if (FloorPlan[index] == 1 || GetNeighbourCellCount(index) > 1 || rng.NextDouble() < 0.5f) { return false; }

        // 30% chance to try and place a large room
        if (rng.NextDouble() < 0.3f)
        {
            // Randomly chooses the order that a shape will try and be placed
            foreach (int[] shapeOffsets in RoomShapes.OrderBy(_ => rng.NextDouble()))
            {
                if (TryPlaceRoom(index, shapeOffsets)) // If can place a large room
                {
                    return true;
                }
            }
        }

        // Is not a large room
        return true;
    }

    private bool TryPlaceRoom(int index, int[] offsetsForShape)
    {
        List<int> currentRoomIndexes = new List<int>() {index};
        int currentIndexChecked;


        // Goes through and checks if that specifc shape can be placed based on its offsets
        foreach (var offset in offsetsForShape)
        {
            currentIndexChecked = index + offset; // Holds each cell offset

            //  If top or bottom are outside of bounds - fail to place room
            if (((currentIndexChecked - 10) < 0) || ((currentIndexChecked + 10) >= FloorPlan.Length))
            {
                return false;
            }

            // Prevent horizontal out of bounds
            int indexRow = index / 10;
            int targetRow = currentIndexChecked / 10;
        
            if (Math.Abs(offset) == 1 && targetRow != indexRow)
            {
                return false; // Can't place this room
            }

            // If current cell is occupied
            if (FloorPlan[currentIndexChecked] == 1) { return false;}

            if (currentIndexChecked == index) continue; // Skips if same as origin

            // Because grid 0-9 -> 10 is left side - so prevents a prevents 
            // wrapping around horizontally (e.g., from one row to another)
            if (currentIndexChecked % 10 == 0 || currentIndexChecked % 10 == 9) continue;

            currentRoomIndexes.Add(currentIndexChecked);
        }

        return false;
    }

    private void SpawnLargeRoomData(List<int> indexes)
    {
        
    }

    private int GetNeighbourCellCount(int index)
    {
        // Neightbour count 
        int count = 0;

        int row = index / 10;
        int col = index % 10;

        // Checks if within bounds, then checks whether neighbour cell has a room

        // Up
        count += (row > 0) ? FloorPlan[index - 10] : 0;
        // Down
        count += (row < 9) ? FloorPlan[index + 10] : 0;
        // Left
        count += (col > 0) ? FloorPlan[index - 1] : 0;
        // Right
        count += (col < 9) ? FloorPlan[index + 1] : 0;

        return count;
    }

    private void CleanEndRoomsList()
    {
        EndRooms.RemoveAll(item => LargeRoomIndexes.Contains(item) || GetNeighbourCellCount(item) > 1);
    }

    private void SetupSpecialRooms()
    {
        // BossRoom is assigned to the last EndRoom in the list
        BossRoomIndex = EndRooms.Count > 0 ? EndRooms[EndRooms.Count - 1] : -1;

        if (BossRoomIndex != -1)
        {
            EndRooms.RemoveAt(EndRooms.Count - 1);
        }

        ItemRoomIndex = RandomEndRoom();
        ShopRoomIndex = RandomEndRoom();

        // If any of the rooms fail then restart dungeon setup
        if (ItemRoomIndex == -1 || ShopRoomIndex == -1 || BossRoomIndex == -1)
            SetupDungeon();

        SpawnRoom(SecretRoomIndex);
        UpdateSpecialRoomVisuals();

        RoomManager.roomInstance.SetupRooms(CellList);
    }

    private int RandomEndRoom()
    {
        // Picks a random End Room returns -1 if there are no end rooms
        return EndRooms.Count > 0 ? EndRooms[rng.Next(0, EndRooms.Count)]: -1;
    }
}