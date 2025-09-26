using UnityEngine;

public class Spin : MonoBehaviour
{
    public float rotationSpeed = 1500f; // degrees per second

    void Update()
    {
        
        // Rotate around Z-axis (for 2D top-down)
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
