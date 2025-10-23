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

    // Dictionary for faster search time for finding room data
    private Dictionary<GameObject, Room> ActiveMinimapRooms;

    // Handles the initial position and instatiation of each room prefab
    public void SetupMiniMap(MapPresenter mapPresenter, MapModel model)
    {
        if (ActiveMinimapRooms != null && ActiveMinimapRooms.Count > 0)
        {
            ResetMiniMap();
        }

        // New Dictionary per level
        ActiveMinimapRooms = new Dictionary<GameObject, Room>();

        List<Room> ActiveRooms = mapPresenter.GetSpawnedRooms;
        foreach (var RoomData in ActiveRooms)
        {
            // Converts index to row and col position
            int row = RoomData.OriginIndex / 10;
            int col = RoomData.OriginIndex % 10;

            // Gets file of minimap prefab based on shape, type and current level
            string RoomName = "mini" + "_" + RoomData.RoomShape + "_" + RoomData.RoomType;

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
            //Room RoomData = Room.GetComponent<MiniRoomModel>().SetupMiniRoom(room);

            // Adds GameObject and RoomData to dictionary
            ActiveMinimapRooms.Add(Room, RoomData);

            // Get Room GameObject UI transform
            RectTransform CellTransform = Room.GetComponent<RectTransform>();

            // Calculated position and size
            Vector2 RoomSize;
            Vector2 RoomPosition;

            if (RoomData.RoomShape == RoomShape.OneByOne)
                RoomSize = new Vector2(CellSize, CellSize);
            else if (RoomData.RoomShape == RoomShape.OneByTwo)
                RoomSize = new Vector2(CellSize, CellSize * 2);
            else if (RoomData.RoomShape == RoomShape.TwoByOne)
                RoomSize = new Vector2(CellSize * 2, CellSize);
            else // 2x2 or L shapes
                RoomSize = new Vector2(CellSize * 2, CellSize * 2);

            RoomPosition = new Vector2(col * CellSize, -row * CellSize);


            // Sets the position of the room prefab
            CellTransform.anchoredPosition = RoomPosition;
            // Sets the size for the room prefab
            CellTransform.sizeDelta = RoomSize;

        }

        // Apply styles to each room
        foreach (var MiniRoom in ActiveMinimapRooms)
        {
            ApplyRoomStyling(MiniRoom.Key, mapPresenter, model);
        }
    }

    // Updates each new room
    public void UpdateMinimap(MapPresenter mapPresenter, MapModel model)
    {
        // Map hasn't been created
        if (ActiveMinimapRooms == null)
            return;
        

        // Apply each style to room
        foreach (var MiniRoom in ActiveMinimapRooms)
        {
            ApplyRoomStyling(MiniRoom.Key, mapPresenter, model);
        }
    }

    public void ApplyRoomStyling(GameObject RoomObject, MapPresenter mapPresenter, MapModel model)
    {
        Room RoomData = ActiveMinimapRooms[RoomObject];
        Image img = RoomObject.GetComponentInChildren<Image>();
        Color currentColor = img.color;

        if(currentColor.a == 0.6f)
        {
            return;
        }

        // Default to no colour overlay
        Color BaseColour = Color.white;

        // Highlights the current room player is in && reveal its surrounding rooms
        if (RoomData == mapPresenter.CurrentPlayerRoom)
        {
            img.color = Color.green;
            RevealSurroundingRooms(RoomObject, model);
        }
        // Makes all room that are not visited hidden (transparent)
        else if (!RoomData.HasBeenVisited && RoomData != mapPresenter.GetStarterRoom)
        {
            img.color = new Color(BaseColour.r, BaseColour.g, BaseColour.b, 0f);
        }
        else
        {
            img.color = BaseColour;
        }

        
    }

    public void ResetMiniMap()
    {
        foreach (var MiniRoom in ActiveMinimapRooms)
        {
            Destroy(MiniRoom.Key);
        }
    }

    // Show the surrounding rooms if they haven't been visited
    public void RevealSurroundingRooms(GameObject RoomObject, MapModel model)
    {
        Room RoomData = ActiveMinimapRooms[RoomObject];
        int[] FloorPlan = model.GetFloorPlan;

        // Adjacent room directions
        int North = RoomData.OriginIndex - 10;
        int South = RoomData.OriginIndex + 10;
        int East = RoomData.OriginIndex + 1;
        int West = RoomData.OriginIndex - 1;

        int[] directions = { North, South, East, West };

        // Check if adjacent room is occupied and has not been visited
        foreach (int direct in directions)
        {
            // Skip if out of bounds
            if (direct < 0 || direct >= FloorPlan.Length)
                continue;

            // Skip if room cell not occupied
            if (FloorPlan[direct] == 0)
                continue;

            // Checks all the active rooms for the adjacent room
            foreach (var MiniRoom in ActiveMinimapRooms)
            {
                // Change image to transparent if it hasn't been visited
                if (MiniRoom.Value.OccupiedIndexes.Contains(direct))
                {
                    Debug.Log("Found");
                }

                if (MiniRoom.Value.OccupiedIndexes.Contains(direct))
                {
                    Debug.Log("we get here");
                    Image img = MiniRoom.Key.GetComponentInChildren<Image>();
                    Color BaseColour = Color.white;
                    img.color = new Color(BaseColour.r, BaseColour.g, BaseColour.b, 0.6f);
                }
            }

        }

    }

    

    
}
