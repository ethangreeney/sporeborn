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
        CellList = new();
        CellQueue = new Queue<int>();
        
        // Automatically will create new Dungeon Layout
        GenerateDungon();
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

            // CheckValidCell will create the room 
            // If valid cell and has one neighbouring cell then it is an EndRoom
            if (CheckValidCell(index) && GetNeighbourCellCount(index) == 1)
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
            foreach(RoomShape shape in RoomShapes.Keys.OrderBy(_ => rng.NextDouble()))
            {
                foreach (int[] shapeOffsets in RoomShapes[shape])
                {
                    if (CanPlaceLargeRoom(index, shapeOffsets)) // If can place a large room variant
                    {
                        // New large rooom
                        AddNewRoom(index, shapeOffsets, shape);
                        return true;
                    }
                }
            }
        }
        
        int[] onebyoneConfig = RoomShapes[RoomShape.OneByOne][0];
        AddNewRoom(index, onebyoneConfig, RoomShape.OneByOne);
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
        // Mark surrounding spacing as occupied - based on room shape
        foreach (int idx in roomIndexes)
            FloorPlan[idx] = 1;

        // Adds to the cell queue
        CellQueue.Enqueue(index);

        // Figure out what shape the room is and assign initial type
        // By default every room is regular type
        Cell newCell = new Cell(new CellData(index, roomIndexes, RoomType.Regular, shape));
        CellList.Add(newCell);
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

        // Set the correct room types
        CellList[BossRoomIndex].RoomType = RoomType.Boss;
        CellList[ItemRoomIndex].RoomType = RoomType.Item;
        CellList[ShopRoomIndex].RoomType = RoomType.Shop;
    }

    private int RandomEndRoom()
    {
        // Picks a random End Room returns -1 if there are no end rooms
        return EndRooms.Count > 0 ? EndRooms[rng.Next(0, EndRooms.Count)] : -1;
    }
}