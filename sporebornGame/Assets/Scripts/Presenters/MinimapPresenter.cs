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
    public void SetupMiniMap(MapModel model, MapPresenter mapPresenter)
    {
        if(ActiveMinimapRooms != null && ActiveMinimapRooms.Count > 0)
        {
            ResetMiniMap();
        }
        
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
                RoomSize = new Vector2(CellSize, CellSize);
            else if (room.RoomShape == RoomShape.OneByTwo)
                RoomSize = new Vector2(CellSize, CellSize * 2);
            else if (room.RoomShape == RoomShape.TwoByOne)
                RoomSize = new Vector2(CellSize * 2, CellSize);
            else // 2x2 or L shapes
                RoomSize = new Vector2(CellSize * 2, CellSize * 2);
            
            RoomPosition = new Vector2(col * CellSize, -row * CellSize);
            

            // Sets the position of the room prefab
            CellTransform.anchoredPosition = RoomPosition;
            // Sets the size for the room prefab
            CellTransform.sizeDelta = RoomSize;

        }
    }

    // Updates each new room
    public void UpdateMinimap(MapPresenter mapPresenter)
    {
        // Map hasn't been created
        if (ActiveMinimapRooms == null)
        {
            return;
        }
        
        // Apply each style to room
        foreach (GameObject MiniRoom in ActiveMinimapRooms)
        {
            ApplyRoomStyling(MiniRoom, mapPresenter);
        }
    }

    public void ApplyRoomStyling(GameObject RoomObject, MapPresenter mapPresenter)
    {
        MiniRoomModel CurrentRoomData = RoomObject.GetComponent<MiniRoomModel>();
        Room RoomData = CurrentRoomData.GetRoomData;
        Image img = RoomObject.GetComponentInChildren<Image>();

        // Default to no colour overlay
        Color BaseColour = Color.white;

        // Highlights the current room player is in
        if (RoomData == mapPresenter.CurrentPlayerRoom)
        {
            BaseColour = Color.green;
        }

        // Makes all room that are not visited semi-transparent
        else if (!RoomData.HasBeenVisited && RoomData != mapPresenter.GetStarterRoom)
        {
            BaseColour = new Color(BaseColour.r, BaseColour.g, BaseColour.b, 0.6f);
        }

        img.color = BaseColour;
    }

    public void ResetMiniMap()
    {
        foreach (GameObject MiniRoom in ActiveMinimapRooms)
        {
            Destroy(MiniRoom);
        }
    }
            //     if (room.RoomShape == RoomShape.OneByOne)
            // {
            //     RoomSize = new Vector2(CellSize, CellSize);
            //     RoomPosition = new Vector2(col * CellSize, -row * CellSize);
            // }
            // else if (room.RoomShape == RoomShape.OneByTwo)
            // {
            //     RoomSize = new Vector2(CellSize, CellSize * 2);
            //     RoomPosition = new Vector2(col * CellSize, -row * CellSize + (CellSize / 2f));
            // }
            // else if (room.RoomShape == RoomShape.TwoByOne)
            // {
            //     RoomSize = new Vector2(CellSize * 2, CellSize);
            //     RoomPosition = new Vector2(col * CellSize + (CellSize / 2f), -row * CellSize);

            // }

    // if (room.RoomShape == RoomShape.LShape_270)
                // {
                //     RoomPosition = new Vector2(col * CellSize + (CellSize / 2f), row);
                // }
                // else if (room.RoomShape == RoomShape.LShape_180)
                // {
                //     RoomPosition = new Vector2(col * CellSize - (CellSize / 2f), -row * CellSize + (CellSize / 2f));
                // }
                // else if (room.RoomShape == RoomShape.LShape_90)
                // {
                //     RoomPosition = new Vector2(col * CellSize + (CellSize / 2f), -row * CellSize + (CellSize / 2f));
                // }
                // else if (room.RoomShape == RoomShape.LShape_0)
                // {
                //     RoomPosition = new Vector2(col * CellSize + (CellSize / 2f), -row * CellSize + (CellSize / 2f));
                // }
                // else // 2x2
                // {
                //     RoomPosition = new Vector2(col * CellSize + (CellSize / 2f), -row * (CellSize*2) + (CellSize/2f));
                // }
}
