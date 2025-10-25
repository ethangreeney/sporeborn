using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapModel
{
    private int[] FloorPlan;
    public int[] GetFloorPlan => FloorPlan;

    private int MinRooms;
    private int MaxRooms;

    // Locations of each of the special type rooms
    public int BossRoomIndex, ItemRoomIndex, ShopRoomIndex;

    // Location in floor plan the room generation starts from
    private const int StartingRoomIndex = 45;
    public int GetStartingRoomIndex => StartingRoomIndex;

    // One side of grid length
    private int GRID_SIDE = 10;
    public int GET_GRID_SIDE => GRID_SIDE;

    // Generates Random numbers
    System.Random rng;

    // Tracking Parts of the map
    private Queue<int> CellQueue;
    private List<int> EndRooms;
    private List<int> LargeRoomIndexes;
    // temp remove later
    public List<int> GetLargeRoomIndexes => LargeRoomIndexes;

    private List<Room> RoomList;
    public List<Room> GetRoomList => RoomList;

    private static readonly Dictionary<RoomShape, List<int[]>> RoomShapesOffsets = new()
    {
        // Configurations for the large room types and variants of that room
        // Values to add to get the surrounding cells for room type

        {
            RoomShape.OneByTwo,
            new List<int[]>
            {
                new int[]{ -10 }   // 1x2 - Top
            }
        },
        {
            RoomShape.TwoByOne,
            new List<int[]>
            {
                new int[]{ 1 },    // 2x1 - Origin on left 
            }
            
        },
        {
            RoomShape.TwoByTwo,
            new List<int[]>
            {
                new int[]{ 1, -9, -10 }
            }
        },
        {
            RoomShape.LShape_0,
            new List<int[]>
            {
                new int[]{ 1, -10 },       // L shape - 0 degrees
            }
        },
        {
            RoomShape.LShape_90,
            new List<int[]>
            {
                new int[]{ -9, -10 },        // L shape - 90 degrees
            }
        },

        {
            RoomShape.LShape_180,
            new List<int[]>
            {
                new int[]{ -11, -10 },       // L shape - 180 degrees
            }
        },
        {
            RoomShape.LShape_270,
            new List<int[]>
            {
                new int[]{ 1, -9 },      // L shape - 270 degrees
            }
        },

    };

    public MapModel(int minRooms, int maxRooms)
    {
        this.MinRooms = minRooms;
        this.MaxRooms = maxRooms;
        rng = new System.Random();

        // Automatically will create new Dungeon Layout
        SetupDungeon();
    }

    public void SetupDungeon()
    {
        // Initalises new/clears previous map
        ResetMapState(); 
        // Generation will start from this point (centre)
        CheckValidCell(StartingRoomIndex);
        // Sets up new dungeon
        GenerateDungeon();
        // Check if Floorplan has been generated
        // PrintFloorPlan();
    }

    private void ResetMapState()
    {
        RoomList = new List<Room>();
        CellQueue = new Queue<int>();
        EndRooms = new List<int>();
        LargeRoomIndexes = new List<int>();
        FloorPlan = new int[100];
    }

    private void GenerateDungeon()
    {
        while (CellQueue.Count > 0)
        {
            int index = CellQueue.Dequeue();
            // Calculates x coordinate of index
            int x = index % GRID_SIDE;

            if (x > 0) CheckValidCell(index - 1); // Checks left space
            if (x < 9) CheckValidCell(index + 1); // Checks right space
            if (index >= GRID_SIDE) CheckValidCell(index - GRID_SIDE); // Checks upwards space 
            if (index < 90) CheckValidCell(index + GRID_SIDE); // Checks downwards space

            // If no neighbours were created then it is an end room
            if (GetNeighbourCellCount(index) == 1 && index != StartingRoomIndex)
            {
                EndRooms.Add(index);
            }

        }
        
        // After Generation Complete:
        // Loops through floor plan to find number of rooms that equal 1
        int RoomCount = FloorPlan.Count(c => c == 1);

        // Retry the generation if we don't meet the MinRoom count
        if (RoomCount < MinRooms)
        {
            SetupDungeon();
            return;
        }

        // Regenerate if not enough end rooms to setup special room
        if (EndRooms.Count <= 3)
        {
            SetupDungeon(); 
            return;
        }

        // Removes all end rooms that are large rooms from the list 
        CleanEndRoomsList();
        // Sets up Special Rooms
        SetupSpecialRooms();
    }

    private void CleanEndRoomsList()
    {
        EndRooms.RemoveAll(item => LargeRoomIndexes.Contains(item) || GetNeighbourCellCount(item) > 1);
    }

    private int RandomEndRoom()
    {
        // Picks a random End Room returns -1 if there are no end rooms
        return EndRooms.Count > 0 ? EndRooms[rng.Next(0, EndRooms.Count)] : -1;
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
        EndRooms.Remove(ItemRoomIndex);
        ShopRoomIndex = RandomEndRoom();
        EndRooms.Remove(ShopRoomIndex);

        // If any of the rooms fail then restart dungeon setup
        if (ItemRoomIndex == -1 || ShopRoomIndex == -1 || BossRoomIndex == -1)
            SetupDungeon();

        // Set the correct room types
        Room BossRoom = RoomList.First(room => room.OriginIndex == BossRoomIndex);
        BossRoom.RoomType = RoomType.Boss;
        
        Room ItemRoom = RoomList.First(room => room.OriginIndex == ItemRoomIndex);
        ItemRoom.RoomType = RoomType.Item;
        
        Room ShopRoom = RoomList.First(room => room.OriginIndex == ShopRoomIndex);
        ShopRoom.RoomType = RoomType.Shop;
        
    }

    private bool CheckValidCell(int index)
    {
        // Out of bounds of the map array
        if (index >= FloorPlan.Length || index < 0)
        {
            return false;
        }

        // Greater than max room num - so can't create any more rooms
        if (FloorPlan.Count(c => c == 1) >= MaxRooms)
        {
            return false;
        }

        // Fails if - Aleady a room at this cell || If neightbour has room || Fails to create a room 50% of the time
        if (FloorPlan[index] == 1 || GetNeighbourCellCount(index) > 1 || rng.NextDouble() < 0.5f)
        {
            return false;
        }

        // Gurantees the starting room will generate
        if (index == StartingRoomIndex)
        {
            AddNewRoom(index, new int[] { }, RoomShape.OneByOne);
            CellQueue.Enqueue(index);
            return true;
        }

        // 30% chance to try and place a large room
        if (rng.NextDouble() < 0.3f)
        {
            
            // Randomly chooses the order that a shape will try and be placed
            foreach (RoomShape shape in RoomShapesOffsets.Keys.OrderBy(_ => rng.NextDouble()))
            {
                // Checks each possible rotation of the large room
                foreach (int[] shapeOffsets in RoomShapesOffsets[shape])
                {
                    if (CanPlaceLargeRoom(index, shapeOffsets)) // If can place a large room variant
                    {
                        // New large rooom
                        AddNewRoom(index, shapeOffsets, shape);
                        CellQueue.Enqueue(index);
                        LargeRoomIndexes.Add(index);
                        return true;
                    }
                }
            }
        }

        // If it can't generate make large room it generates a OneByOne
        AddNewRoom(index, new int[] { }, RoomShape.OneByOne);
        CellQueue.Enqueue(index);

        return true;
    }

    private bool CanPlaceLargeRoom(int index, int[] offsetsForShape)
    {
        List<int> currentRoomIndexes = new List<int>() { index };
        int roomOriginIndex;

        // Goes through and checks if that specifc shape can be placed based on its offsets
        foreach (var offset in offsetsForShape)
        {
            roomOriginIndex = index + offset; // Holds each cell offset

            // Prevents a large room from being created with the starting index
            if (roomOriginIndex == StartingRoomIndex || index == StartingRoomIndex)
            return false;

            //  If Offset is outside of bounds - fail to place room
            if ((roomOriginIndex < 0) || roomOriginIndex  >= FloorPlan.Length)
            {
                return false;
            }

            // Prevent horizontal out of bounds
            int indexRow = index / GRID_SIDE;
            int targetRow = roomOriginIndex / GRID_SIDE;
            if (Math.Abs(offset) == 1 && targetRow != indexRow)
            {
                return false; // Can't place this room
            }

            // If current cell is occupied
            if (FloorPlan[roomOriginIndex] == 1) { return false; }

            if (roomOriginIndex == index) continue; // Skips if same as origin

            // Because grid 0-9 -> 10 is left side - so prevents a prevents 
            // wrapping around horizontally (e.g., from one row to another)
            if (roomOriginIndex % GRID_SIDE == 0 || roomOriginIndex % GRID_SIDE == 9) return false;

            currentRoomIndexes.Add(roomOriginIndex);
        }

        // Room is OneByOne and not large
        if (currentRoomIndexes.Count == 1) { return false; }

        return true;
    }

    private void AddNewRoom(int index, int[] roomIndexes, RoomShape shape)
    {   
        // Origin is marked as occupied
        FloorPlan[index] = 1;
        
        // Each room will store all the indexes it uses on the floor plan
        List<int> AllOccupiedIndexes = new List<int>{index}; // First value is origin index

        // Mark surrounding spacing as occupied on FloorPlan- based on room shape offsets
        foreach (int offset in roomIndexes)
        {
            FloorPlan[index + offset] = 1;
            
            AllOccupiedIndexes.Add(index + offset); // adds offsets to index
        }

        // By default every room is regular type
        Room newRoom = new Room(new RoomData(index, AllOccupiedIndexes, RoomType.Regular, shape));
        RoomList.Add(newRoom);
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
    
}