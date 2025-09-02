
using System.Collections.Generic;

public enum RoomShape
{
    OneByOne,
    OneByTwo,
    TwoByOne,
    TwoByTwo,
    LShape_0,
    LShape_90,
    LShape_180,
    LShape_270,
}
public enum RoomType
{
    Regular,
    Item,
    Shop,
    Boss,
}

// Cell is a representation of one room - either 1x1 or muti-celled
public class Cell
{
    // Cell Data fields
    public int Index { get; set; }
    public RoomType RoomType { get; set; }
    public RoomShape RoomShape { get;  set; }
    public List<int> OccupiedIndexes { get; private set; }

    public Cell(CellData data)
    {
        Index = data.FloorPlanIndex;
        RoomType = data.RoomType;
        RoomShape = data.RoomShape;
        OccupiedIndexes = new List<int>(data.RoomIndexes);
    }
}
