using UnityEngine;
using System.Linq;

public class MiniRoomModel : MonoBehaviour
{

    private Room RoomData;
    public Room GetRoomData => RoomData;

    public void SetupMiniRoom(Room RoomData)
    {
        this.RoomData = RoomData;
    }
}