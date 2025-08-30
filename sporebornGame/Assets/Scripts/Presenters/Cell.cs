using System.Collections.Generic;
using UnityEngine;


public enum RoomType
{
    Regular,
    Item,
    Shop,
    Boss,
    Secret
}

public enum RoomShape
{
    OneByOne,
    OneByTwo,
    TwoByOne,
    TwoByTwo,
    LShape
}

// Cell is a representation of one room - either 1x1 or muti-celled
public class Cell : Monobehaviour
{
    // Cell Data fields
    private int Index { get; private set; }
    private RoomType RoomType { get; private set; }
    private RoomShape RoomShape { get; private set; }
    private List<int> OccupiedIndexes { get; private set; }

    public Cell(CellData cellData)
    {
        Index = cellData.FloorPlanIndex;
        RoomType = cellData.RoomType;
        RoomShape = cellData.RoomShape;
        OccupiedIndexes = cellData.CellList;
    }
}
