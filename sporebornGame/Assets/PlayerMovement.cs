using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;   // movement speed
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        // Get Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get input (WASD / Arrow Keys)
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right arrows
        float vertical = Input.GetAxisRaw("Vertical");     // W/S or Up/Down arrows

        // Store movement as a Vector2
        moveInput = new Vector2(horizontal, vertical).normalized;
    }

    void FixedUpdate()
    {
        // Apply movement in FixedUpdate (for physics)
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
