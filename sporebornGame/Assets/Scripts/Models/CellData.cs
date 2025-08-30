[System.Serializable]
public class CellData
{
    public int FloorPlanIndex;  // Origin point in the FloorPlan   
    public List<int> CellList;  // All the indexes that make up that room      
    public RoomType RoomType;   // Shop, Item, Regular, Boss
    public RoomShape RoomShape; // OnebyOne, LShaped, TwoByTwo, etc  
}

[System.Serializable]
public class RoomData
{
    public int CellIndex;               // Cell Index of Room
    public bool IsWall;                 // Is tile a wall/barrier
    public bool IsDoor;                 // Is tile a door
    public List<int> PathfindableTiles; // Potentially could have for pathfinding
}