using UnityEngine;

public class PulsatingPortal : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;       // How fast it pulses
    public float pulseStrength = 0.1f;  // How much it grows/shrinks
    public float baseScale = 1f;        // Starting size

    private Vector3 initialScale;
    private MapPresenter mapPresenter;

    private float spawnTime;
    public float activationDelay = 4f; // Delay before the portal becomes active
    void Start()
    {
        initialScale = Vector3.one * baseScale;
        // Find the MapPresenter in the scene
        mapPresenter = FindFirstObjectByType<MapPresenter>();
        spawnTime = Time.time;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseStrength;
        transform.localScale = initialScale * scale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - spawnTime < activationDelay)
            return; // Ignore collisions during the activation delay

        if (other.CompareTag("Player") && mapPresenter != null)
        {
            mapPresenter.ResetMap();
        }
    }
}
