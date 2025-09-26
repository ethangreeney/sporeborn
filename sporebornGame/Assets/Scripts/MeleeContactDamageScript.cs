using UnityEngine;

public class MeleeContactDamageScript : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] string playerTag = "Player";
    [SerializeField] float hitInterval = 0.5f;
    [SerializeField] LayerMask environmentMask; // set to Environment

    float nextTimeCanHit;

    void OnTriggerEnter2D(Collider2D other)  => TryHit(other);
    void OnTriggerStay2D(Collider2D other)   => TryHit(other);

    void TryHit(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time < nextTimeCanHit)   return;

        var pp = other.GetComponent<PlayerPresenter>() ?? other.GetComponentInParent<PlayerPresenter>();
        if (!pp) return;

        // Optional: prevent through-wall hits
        if (Physics2D.Linecast(transform.position, pp.transform.position, environmentMask))
            return;

        pp.TakeDamage(damage, transform.position);
        nextTimeCanHit = Time.time + hitInterval;
    }
}
