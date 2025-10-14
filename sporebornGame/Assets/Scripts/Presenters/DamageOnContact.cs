using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
	public int damage = 1;
	public float cooldown = 0.5f;

	float lastHitTime;

	void OnTriggerEnter2D(Collider2D other) { TryDamage(other); }
	void OnTriggerStay2D(Collider2D other) { TryDamage(other); }

	void TryDamage(Collider2D other)
	{
		if (Time.time < lastHitTime + cooldown) return;
		var player = other.GetComponentInParent<PlayerPresenter>();
		if (player != null)
		{
			player.TakeDamage(damage);
			lastHitTime = Time.time;
		}
	}
}