using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(AIPath))]
public class RandomAccelerationBoss : MonoBehaviour
{

    private AIDestinationSetter destinationSetter;
    [Header("Speed Settings")]
    public float baseSpeed = 4f;          
    public float burstSpeed = 10f;        
    public float acceleration = 20f;      
    public float minBurstInterval = 2f;   
    public float maxBurstInterval = 5f;   
    public float burstDuration = 0.5f;    

    [Header("Visual Color")]
    public Color dashColor = Color.red;  

    private AIPath ai;
    private float targetSpeed;
    private float burstTimer = 0f;
    private bool bursting = false;

    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();

        // Automatically find the player in the scene
        if (destinationSetter.target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                destinationSetter.target = playerObj.transform;
        }

        ai = GetComponent<AIPath>();
        ai.canMove = true;
        ai.maxSpeed = baseSpeed;
        targetSpeed = baseSpeed;
        ScheduleNextBurst();

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
    }

    void Update()
    {
        // Gradually adjust AIPath.maxSpeed toward targetSpeed
        ai.maxSpeed = Mathf.MoveTowards(ai.maxSpeed, targetSpeed, acceleration * Time.deltaTime);

        // Handle burst timing
        burstTimer -= Time.deltaTime;
        if (burstTimer <= 0f)
        {
            if (!bursting)
            {
                // Start the dash
                if (sr != null)
                    sr.color = dashColor;  // turn red

                targetSpeed = burstSpeed;
                bursting = true;
                burstTimer = burstDuration;
            }
            else
            {
                // End the dash
                targetSpeed = baseSpeed;
                bursting = false;
                ScheduleNextBurst();

                if (sr != null)
                    sr.color = originalColor; // reset color
            }
        }
    }

    void ScheduleNextBurst()
    {
        burstTimer = Random.Range(minBurstInterval, maxBurstInterval);
    }
}
