using System.Collections.Generic;

[System.Serializable]
public class RoomData
{
    public int CellIndex;               // Origin point of room
    public RoomShape Shape;             // The RoomShape of this tilset
    public RoomType Type;               // The RoomType of this tilset
    public List<TileData> RoomTiles;    // All tiles that make up the room
}

[System.Serializable]
public class TileData
{
    // If not a wall,door or chest it is a enemy spawnable tile
    public int Index;       
    public bool isWall;     // Is tile a wall/barrier
    public bool isDoor;     // Is tile a door
    public bool isChest;    // Is tile a chest
}
