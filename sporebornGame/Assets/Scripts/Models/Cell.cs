
using System.Collections.Generic;

public enum RoomShape
{
    OneByOne,
    OneByTwo,
    TwoByOne,
    TwoByTwo,
    LShape,
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
    public int Index { get; private set; }
    public RoomType RoomType { get; private set; }
    public RoomShape RoomShape { get; private set; }
    public List<int> OccupiedIndexes { get; private set; }

    public Cell(RoomData data)
    {
        Index = data.FloorPlanIndex;
        RoomType = data.RoomType;
        RoomShape = data.RoomShape;
        OccupiedIndexes = new List<int>(data.RoomIndexes);
    }
}
