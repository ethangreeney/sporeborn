using UnityEngine;

public class MapDebugger : MonoBehaviour
{
    public MapPresenter mapPresenter; // Drag the component holding your map presenter here
    public float CellSize = 0.5f; // Size of each cell on minimap
    public float YOffset = 5.0f;

    private void OnDrawGizmos()
    {
        if (mapPresenter == null || mapPresenter.GetSpawnedRooms == null) return;

        foreach (var room in mapPresenter.GetSpawnedRooms)
        {
            // Pick a color based on RoomType or  if it's the starter room
            if (room.Index == mapPresenter.GetStarterRoom.Index)
            {
                Gizmos.color = Color.cyan;
            }
            else
            {
                // Highlights Special Rooms
                Gizmos.color = room.RoomType switch
                {
                    RoomType.Boss => Color.red,
                    RoomType.Shop => Color.yellow,
                    RoomType.Item => Color.green,
                    _ => Color.white
                };

                if (room.RoomShape != RoomShape.OneByOne)
                {
                    Gizmos.color = Color.blue;
                }
            }

            foreach (var idx in room.OccupiedIndexes)
            {
                int row = idx / 10;
                int col = idx % 10;
                Vector3 pos = new Vector3(col * CellSize, (-row * CellSize) - YOffset, 0);

                Gizmos.DrawCube(pos, Vector3.one * CellSize);
            }
        }
    }
}
