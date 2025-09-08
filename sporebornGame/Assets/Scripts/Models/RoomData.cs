using System.Collections.Generic;

public class RoomData
{
    public int FloorPlanIndex;  // Origin point in the FloorPlan   
    public int[] RoomIndexes;  // All the indexes that make up that room      
    public RoomType RoomType;   // Shop, Item, Regular, Boss
    public RoomShape RoomShape; // OnebyOne, LShaped, TwoByTwo, etc 

    public RoomData(int FloorPlanIndex, int[] RoomIndexes, RoomType RoomType, RoomShape RoomShape)
    {
        this.FloorPlanIndex = FloorPlanIndex;
        this.RoomIndexes = RoomIndexes;
        this.RoomType = RoomType;
        this.RoomShape = RoomShape;
    }
}