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


    public void CreateMinimap()
    {
        DrawMiniMap();
    }

    private void DrawMiniMap()
    {
        //if (mapPresenter == null || mapPresenter.GetSpawnedRooms == null) return;

        foreach (var room in mapPresenter.GetSpawnedRooms)
        {
            Color Colour = GetRoomStyle(room);

            foreach (var idx in room.OccupiedIndexes)
            {
                int row = idx / 10;
                int col = idx % 10;

                // Creates the RoomCell within the Minimap Container

                GameObject RoomCell = Instantiate(MiniroomPrefab, transform, false);
                RectTransform CellTransform = RoomCell.GetComponent<RectTransform>();

                Image img = RoomCell.GetComponent<Image>();

                img.color = Colour;

                // Positions each part of the Room
                CellTransform.anchoredPosition = new Vector2(col * CellSize, -row * CellSize);
                // Sets the size of each segment of the room
                CellTransform.sizeDelta = new Vector2(CellSize, CellSize);
            }
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
