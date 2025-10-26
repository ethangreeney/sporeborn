using UnityEngine;
using System.Collections;
using Pathfinding;

public class RoomPathfindingScan : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ScanOnRoomEntered()
    {
        StartCoroutine(RescanAfterPhysicsSettles());
    }

    // Update is called once per frame
    private IEnumerator RescanAfterPhysicsSettles()
    {
        // Make sure newly spawned colliders are enabled before scanning
        yield return null;                     // wait 1 frame
        yield return new WaitForFixedUpdate(); // let physics update
        if (AstarPath.active != null)
        {
            AstarPath.active.Scan();          // full rescan of all graphs
        }
    }
}
