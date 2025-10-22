using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapPresenter : MonoBehaviour
{
    public MapPresenter mapPresenter;
    public GameObject MiniroomPrefab;
    public float CellSize = 50f; // Change in inspector

    [SerializeField]
    private List<GameObject> MinimapPrefabs;

    private List<GameObject> ActiveMinimapRooms;

    // Called for each new level
    public void CreateMinimap()
    {
        ActiveMinimapRooms = new();
        DrawMiniMap();
    }

    // Handles the position and instatiation of each room prefab
    private void DrawMiniMap()
    {
        foreach (var room in mapPresenter.GetSpawnedRooms)
        {
            // Converts index to row and col position
            int row = room.OriginIndex / 10;
            int col = room.OriginIndex % 10;

            // Gets file of minimap prefab based on shape, type and current level
            string RoomName = "mini" + "_" + room.RoomShape + "_";

            // Will hide any special rooms that haven't been found
            if (room.RoomShape == RoomShape.OneByOne && !room.RoomCompleted)
            {
                RoomName += "Regular";
            }
            else
            {
                RoomName += room.RoomType;
            }

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

            if(CurrentRoomPrefab == null)
            {
                Debug.Log("Can't find minimap prefab, file naming may be incorrect: " + RoomName);
            }

            // Creates the RoomCell within the Minimap Container
            GameObject Room = Instantiate(CurrentRoomPrefab, transform, false);
            ActiveMinimapRooms.Add(Room);

            RectTransform CellTransform = Room.GetComponent<RectTransform>();


            // Calculated based on the size of the room and current cell size
            Vector2 RoomSize;
            Vector2 RoomPosition;
            if (room.RoomShape == RoomShape.OneByOne)
            {
                RoomSize = new Vector2(CellSize, CellSize);
                RoomPosition = new Vector2(col * CellSize, -row * CellSize);
            }
            else if (room.RoomShape == RoomShape.OneByTwo)
            {
                RoomSize = new Vector2(CellSize * 2, CellSize);
                RoomPosition = new Vector2(col * CellSize + (CellSize / 2f), -row * CellSize);
            }
            else if (room.RoomShape == RoomShape.TwoByOne)
            {
                RoomSize = new Vector2(CellSize, CellSize * 2);
                RoomPosition = new Vector2(col * CellSize, -row * CellSize - (CellSize / 2f));
            }
            else // 2x2 or Lshaped room
            {
                RoomSize = new Vector2(CellSize * 2, CellSize * 2);
                RoomPosition = new Vector2(col * CellSize + (CellSize / 2f), -row * CellSize - (CellSize / 2f));
            }

            // Positions each part of the Room
            CellTransform.anchoredPosition = RoomPosition;
            // Sets the size for the room prefab
            CellTransform.sizeDelta = RoomSize;

        }
    }

    public Color GetRoomStyle(Room CurrentRoom)
    {
        Color Colour;

        // Pick a color based on RoomType or if it's the starter room
        if (CurrentRoom.OriginIndex == mapPresenter.GetStarterRoom.OriginIndex)
        {
            return Color.grey;
        }
        else
        {
            // Highlights Special Rooms
            Colour = CurrentRoom.RoomType switch
            {
                RoomType.Boss => Color.red,
                RoomType.Shop => Color.yellow,
                RoomType.Item => Color.green,
                _ => new Color32(71, 72, 77, 255)
            };
            // Highlights large rooms
            if (CurrentRoom.RoomShape != RoomShape.OneByOne)
            {
                return Color.grey;
            }

            // Highlights room the player is in
            if (CurrentRoom == mapPresenter.CurrentPlayerRoom)
            {
                return Color.magenta;
            }
        }

        return Colour;
    }
}
