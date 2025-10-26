using UnityEngine;
using System.Collections;
using Pathfinding;

public class RoomPathfindingScan : MonoBehaviour
{
    private bool isScanning = false;

    public void ScanOnRoomEntered(BoundsInt roomBounds)
    {
        if (!isScanning)
            StartCoroutine(RescanAfterPhysicsSettles(roomBounds));
    }

    private IEnumerator RescanAfterPhysicsSettles(BoundsInt roomBounds)
    {
        isScanning = true;

        yield return null;
        yield return new WaitForFixedUpdate();

        if (AstarPath.active != null)
        {
            // Partial scan — only updates affected area
            // Convert BoundsInt (cell-space) → Bounds (world-space)
            Bounds bounds = new Bounds();
            bounds.center = roomBounds.center;
            bounds.size = roomBounds.size;

            AstarPath.active.UpdateGraphs(bounds);
        }

        isScanning = false;
    }
}
