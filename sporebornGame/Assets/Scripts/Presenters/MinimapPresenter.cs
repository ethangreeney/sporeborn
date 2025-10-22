using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MinimapPresenter : MonoBehaviour
{
    public GameObject MiniroomPrefab;
    public float CellSize = 50f; // Change in inspector

    [SerializeField]
    private List<GameObject> MinimapPrefabs;

    private List<GameObject> ActiveMinimapRooms;


    // Handles the initial position and instatiation of each room prefab
    public void DrawMiniMap(MapModel model, MapPresenter mapPresenter)
    {
        ActiveMinimapRooms = new();
        List<Room> ActiveRooms = mapPresenter.GetSpawnedRooms;
        foreach (var room in ActiveRooms)
        {
            // Converts index to row and col position
            int row = room.OriginIndex / 10;
            int col = room.OriginIndex % 10;

            // Gets file of minimap prefab based on shape, type and current level
            string RoomName = "mini" + "_" + room.RoomShape + "_" + room.RoomType;

            // Will hide any special rooms that haven't been found
            // if (room.RoomShape == RoomShape.OneByOne && !room.HasBeenVisited)
            // {
            //     RoomName += "Regular";
            // }
            // else
            // {
            //     RoomName += room.RoomType;
            // }

            // Find the correct minmap prefab
            GameObject CurrentRoomPrefab = null;
            foreach (GameObject prefab in MinimapPrefabs)
            {
                if (prefab.name == RoomName)
                {
                    CurrentRoomPrefab = prefab;
                    break;
                }
            }

            if (CurrentRoomPrefab == null)
            {
                Debug.Log("Can't find minimap prefab, file naming may be incorrect: " + RoomName);
            }

            // Creates the room within the Minimap Container
            GameObject Room = Instantiate(CurrentRoomPrefab, transform, false);

            // Attaches the rooms data to the current minimap room
            Room.GetComponent<MiniRoomModel>().SetupMiniRoom(room);
            ActiveMinimapRooms.Add(Room);

            // Apply styling e.g. transparency
            ApplyRoomStyling(Room, mapPresenter);

            // Get Room GameObject UI transform
            RectTransform CellTransform = Room.GetComponent<RectTransform>();

            // Calculated position and size
            Vector2 RoomSize;
            Vector2 RoomPosition;
            if (room.RoomShape == RoomShape.OneByOne)
            {
                RoomSize = new Vector2(CellSize, CellSize);
                RoomPosition = new Vector2(col * CellSize, -row * CellSize);
            }
            else if (room.RoomShape == RoomShape.OneByTwo)
            {
                RoomSize = new Vector2(CellSize, CellSize * 2);
                RoomPosition = new Vector2(col * CellSize, -row * CellSize - (CellSize / 2f));
            }
            else if (room.RoomShape == RoomShape.TwoByOne)
            {
                RoomSize = new Vector2(CellSize * 2, CellSize);
                RoomPosition = new Vector2(col * CellSize + (CellSize / 2f), -row * CellSize);
            }
            else // 2x2 or Lshaped room
            {
                RoomSize = new Vector2(CellSize * 2, CellSize * 2);
                RoomPosition = new Vector2(col * CellSize + (CellSize / 2f), -row * CellSize - (CellSize / 2f));
            }

            // Sets the position of the room prefab
            CellTransform.anchoredPosition = RoomPosition;
            // Sets the size for the room prefab
            CellTransform.sizeDelta = RoomSize;

        }
    }

    // Updates each new room
    public void UpdateMinimap(MapPresenter mapPresenter, Room CurrentPlayerRoom)
    {
        // Don't update on new level build
        if(CurrentPlayerRoom == mapPresenter.GetStarterRoom && !CurrentPlayerRoom.HasBeenVisited)
        {
            return;
        }
        
        foreach (GameObject MiniRoom in ActiveMinimapRooms)
        {
            ApplyRoomStyling(MiniRoom, mapPresenter);
        }
    }
    
    public void ApplyRoomStyling(GameObject RoomObject, MapPresenter mapPresenter)
    {
        MiniRoomModel CurrentRoomData = RoomObject.GetComponent<MiniRoomModel>();
        Room RoomData = CurrentRoomData.GetRoomData;

        // Highlights the current room player is in
        if (RoomData == mapPresenter.CurrentPlayerRoom)
        {
            Image img = RoomObject.GetComponentInChildren<Image>();
            img.color = Color.yellow;
            //new Color(1f, 1f, 0f, 0.5f);
        }

        // Makes all room that are not visited semi-transparent
        else if (!RoomData.HasBeenVisited)
        {
            Image img = RoomObject.GetComponentInChildren<Image>();
            Color baseColor = img.color; // whatever the prefab’s color is
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.6f);
        }
    }

    // public Color GetRoomStyle(Room CurrentRoom)
    // {
    //     Color Colour;

    //     // Pick a color based on RoomType or if it's the starter room
    //     if (CurrentRoom.OriginIndex == mapPresenter.GetStarterRoom.OriginIndex)
    //     {
    //         return Color.grey;
    //     }
    //     else
    //     {
    //         // Highlights Special Rooms
    //         Colour = CurrentRoom.RoomType switch
    //         {
    //             RoomType.Boss => Color.red,
    //             RoomType.Shop => Color.yellow,
    //             RoomType.Item => Color.green,
    //             _ => new Color32(71, 72, 77, 255)
    //         };
    //         // Highlights large rooms
    //         if (CurrentRoom.RoomShape != RoomShape.OneByOne)
    //         {
    //             return Color.grey;
    //         }

    //         // Highlights room the player is in
    //         if (CurrentRoom == mapPresenter.CurrentPlayerRoom)
    //         {
    //             return Color.magenta;
    //         }
    //     }

    //     return Colour;
    // }
    
    // public Vector2 OffsetCalc(MapModel model, MapPresenter mapPresenter, Vector2 RoomPosition){
    //     int[] FloorPlan = model.GetFloorPlan;
    //         float onepixel = CellSize * 0.02f;

    //         // Prevents the room from being moved more than once
    //         bool ShiftedX = false;
    //         bool ShiftedY = false;
    //     // Move room to overlap up & right rooms to avoid a double outline look
    //     foreach (int RoomIndex in room.OccupiedIndexes)
    //     {
    //         // Prevents out of bounds
    //         if ((RoomIndex - 10) < 0) continue;
    //         if (RoomIndex % model.GET_GRID_SIDE == 0 || RoomIndex % model.GET_GRID_SIDE == 9) continue;

    //         // Room above and not within the same room
    //         if (!ShiftedY && FloorPlan[RoomIndex - 10] == 1 && !room.OccupiedIndexes.Contains(RoomIndex - 10))
    //         {
    //             RoomPosition.y += onepixel;
    //             ShiftedY = true;
    //         }
    //         // Room to right and not within the same room
    //         if (!ShiftedX && FloorPlan[RoomIndex + 1] == 1 && !room.OccupiedIndexes.Contains(RoomIndex + 1))
    //         {
    //             RoomPosition.x += onepixel;
    //             ShiftedX = true;
    //         }
    //     }

    //     return RoomPosition;

    // }
}
