using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float PixelsPerUnit = 16f; 

    void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }

    // Called at the end of the frame
    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Target has not been assigned in inspector");
            return;
        }



        Vector3 PlayerPositon = target.position;

        // Snap to pixel grid
        float snappedX = Mathf.Round(PlayerPositon.x * PixelsPerUnit) / PixelsPerUnit;
        float snappedY = Mathf.Round(PlayerPositon.y * PixelsPerUnit) / PixelsPerUnit;
        
        transform.position = new Vector3(snappedX, snappedY, transform.position.z);
    }
}
