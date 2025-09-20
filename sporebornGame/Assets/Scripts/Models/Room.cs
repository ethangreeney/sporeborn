using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

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

// Room is - either 1x1 or muti-celled
public class Room
{
    // Cell Data fields
    public int OriginIndex { get; set; }
    public RoomType RoomType { get; set; }
    public RoomShape RoomShape { get; set; }
    public List<int> OccupiedIndexes { get; private set; }
    public bool RoomCompleted;
    public bool itemCollected = false;
     
    public GameObject assignedItemPrefab;

    public Room(RoomData data)
    {
        OriginIndex = data.FloorPlanIndex;
        RoomType = data.RoomType;
        RoomShape = data.RoomShape;
        OccupiedIndexes = new List<int>(data.RoomIndexes);
        RoomCompleted = false;
    }
    
    public override string ToString()
    {
        return $"Room FloorPlan Origin: {OriginIndex}\n" +
           $"RoomType: {RoomType}\n" +
           $"RoomShape: {RoomShape}\n" +
           $"OccupiedIndexes: {string.Join(", ", OccupiedIndexes)}";
    }
        
}
