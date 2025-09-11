using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target;

    [Header("Camera Offset")]
    public Vector3 offset = new Vector3(0, 0, -10f); // keep camera in front

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Target is not assigned in inspector");
            return;
        }

        // Directly follow target with offset
        transform.position = target.position + offset;
    }
}
