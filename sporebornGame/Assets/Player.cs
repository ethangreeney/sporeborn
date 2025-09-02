using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Rigidbody2D rb;
    private float mx;
    private float my;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        mx = Input.GetAxis("Horizontal");
        my = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(mx, my).normalized * speed;
    }
}
