using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyProjectilePresenter : MonoBehaviour
{
	public float speed = 10f;
	public int damage = 1;
	public float lifetime = 3f;

	Rigidbody2D rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.gravityScale = 0f;
		var col = GetComponent<Collider2D>();
		col.isTrigger = true;
	}

	void Start()
	{
		rb.linearVelocity = transform.right * speed;
		Destroy(gameObject, lifetime);
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.isTrigger) return;
		var player = other.GetComponentInParent<PlayerPresenter>();
		if (player != null)
		{
			player.TakeDamage(damage);
			Destroy(gameObject);
		}
	}
}