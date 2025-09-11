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

    // Generates Random numbers
    System.Random rng;

    // Tracking Parts of the map
    private Queue<int> CellQueue;
    private List<int> EndRooms;
    private List<int> LargeRoomIndexes;

    private List<Room> RoomList;
    public List<Room> GetRoomList => RoomList;

    private static readonly Dictionary<RoomShape, List<int[]>> RoomShapes = new()
    {
        // Configurations for the large room types and variants of that room
        // Values to add/subtract to get the surrounding cells for room type

        // The origin -> (0), is excluded from the indexes
        {
            RoomShape.OneByOne,
            new List<int[]>
            {
                new int[] { }  // Empty because no offsets, origin is handled in AddNewRoom
            }
        },
        {
            RoomShape.OneByTwo,
            new List<int[]>
            {
                new int[]{ 1 },    // 1x2 - Origin on left 
                new int[]{ -1 }    // 1x2 - Origin on right 
            }
        },
        {
            RoomShape.TwoByOne,
            new List<int[]>
            {
                new int[]{ 10 },   // 2x1 - Bottom  
                new int[]{ -10 }   // 2x1 - Top
            }
        },
        {
            RoomShape.TwoByTwo,
            new List<int[]>
            {
                new int[]{ 1, 10, 11 },    // 2x2 - Origin top-left side
                new int[]{ -1, 9, 10 },    // 2x2 - Origin top-right side
            }
        },
        {
            RoomShape.LShape_0,
            new List<int[]>
            {
                new int[]{ 1, -10 },       // L shape - 0°
            }
        },

        {
            RoomShape.LShape_90,
            new List<int[]>
            {
                new int[]{ 1, 10 },        // L shape - 90°
            }
        },

        {
            RoomShape.LShape_180,
            new List<int[]>
            {
                new int[]{ -1, 10 },       // L shape - 180°
            }
        },

        {
            RoomShape.LShape_270,
            new List<int[]>
            {
                new int[]{ -1, -10 },      // L shape - 270°
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
        PrintFloorPlan();
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
            int x = index % 10;

            if (x > 0) CheckValidCell(index - 1); // Checks left space
            if (x < 9) CheckValidCell(index + 1); // Checks right space
            if (index >= 10) CheckValidCell(index - 10); // Checks upwards space 
            if (index < 90) CheckValidCell(index + 10); // Checks downwards space

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
        Room BossRoom = RoomList.First(room => room.Index == BossRoomIndex);
        BossRoom.RoomType = RoomType.Boss;
        
        Room ItemRoom = RoomList.First(room => room.Index == ItemRoomIndex);
        ItemRoom.RoomType = RoomType.Item;
        
        Room ShopRoom = RoomList.First(room => room.Index == ShopRoomIndex);
        ShopRoom.RoomType = RoomType.Shop;
        
    }

    private bool CheckValidCell(int index)
    {
        // Gurantees the starting room will generate
        if (index == StartingRoomIndex)
        {
            AddNewRoom(index, RoomShapes[RoomShape.OneByOne][0], RoomShape.OneByOne);
            CellQueue.Enqueue(index);
            return true;
        }
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

        // 30% chance to try and place a large room
        if (rng.NextDouble() < 0.3f)
        {
            // Randomly chooses the order that a shape will try and be placed
            foreach (RoomShape shape in RoomShapes.Keys.OrderBy(_ => rng.NextDouble()))
            {
                // Checks each possible rotation of the large room
                foreach (int[] shapeOffsets in RoomShapes[shape])
                {
                    if (CanPlaceLargeRoom(index, shapeOffsets)) // If can place a large room variant
                    {
                        // New large rooom
                        AddNewRoom(index, shapeOffsets, shape);
                        CellQueue.Enqueue(index);
                        return true;
                    }
                }
            }
        }

        // If it can't generate make large room it generates a OneByOne
        int[] onebyoneConfig = RoomShapes[RoomShape.OneByOne][0];
        AddNewRoom(index, onebyoneConfig, RoomShape.OneByOne);
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

            //  If top or bottom are outside of bounds - fail to place room
            if (((roomOriginIndex - 10) < 0) || ((roomOriginIndex + 10) >= FloorPlan.Length))
            {
                return false;
            }

            // Prevent horizontal out of bounds
            int indexRow = index / 10;
            int targetRow = roomOriginIndex / 10;
            if (Math.Abs(offset) == 1 && targetRow != indexRow)
            {
                return false; // Can't place this room
            }

            // If current cell is occupied
            if (FloorPlan[roomOriginIndex] == 1) { return false; }

            if (roomOriginIndex == index) continue; // Skips if same as origin

            // Because grid 0-9 -> 10 is left side - so prevents a prevents 
            // wrapping around horizontally (e.g., from one row to another)
            if (roomOriginIndex % 10 == 0 || roomOriginIndex % 10 == 9) continue;

            currentRoomIndexes.Add(roomOriginIndex);
        }

        return true;
    }

    private void AddNewRoom(int index, int[] roomIndexes, RoomShape shape)
    {
        // Origin is marked as occupied
        FloorPlan[index] = 1;
        
        // Each room will store all the indexes it uses on the floor plan
        List<int> AllOccupiedIndexes = new List<int>{index}; // First value is origin index

        // Mark surrounding spacing as occupied on FloorPlan- based on room shape offsets
        foreach (int idx in roomIndexes)
        {
            FloorPlan[index + idx] = 1;
            // 
            AllOccupiedIndexes.Add(index + idx);
        }
        // Figure out what shape the room is and assign initial type
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
    
    private void PrintFloorPlan()
    {
        string output = "";
        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                int index = row * 10 + col;
                output += FloorPlan[index] == 1 ? "1 " : "0 ";
            }
            output += "\n";
        }
        Debug.Log(output);
    }
}