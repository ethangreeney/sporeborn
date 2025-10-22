using UnityEngine;

public class MeleeContactDamageScript : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] int damage = 1;
    [SerializeField] string playerTag = "Player";
    [SerializeField] float hitInterval = 0.5f;
    [SerializeField] LayerMask environmentMask; // set to Environment (optional)

    [Header("Animation")]
    [SerializeField] Animator animator;                  // drag Enemy GFX Animator here (optional)
    [SerializeField] string attackTriggerName = "Attack";
    [SerializeField] string attackXParam = "AttackX";    // must exist in Animator
    [SerializeField] string attackYParam = "AttackY";    // must exist in Animator

    float nextTimeCanHit;

    void Awake()
    {
        // Auto-find the Animator if not assigned in Inspector
        if (animator == null)
        {
            // Try via EnemyModel first (it already caches the child Animator)
            var model = GetComponentInParent<EnemyModel>();
            if (model != null) animator = model.animator;

            // Fallback: search from root down to the Enemy GFX child
            if (animator == null) animator = transform.root.GetComponentInChildren<Animator>(true);
        }
    }

    void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    void OnTriggerStay2D(Collider2D other)  => TryHit(other);

    void TryHit(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time < nextTimeCanHit)    return;

        var pp = other.GetComponent<PlayerPresenter>() ?? other.GetComponentInParent<PlayerPresenter>();
        if (!pp) return;

        // Optional: prevent through-wall hits
        if (environmentMask.value != 0 && Physics2D.Linecast(transform.position, pp.transform.position, environmentMask))
            return;

        // --- Decide attack direction relative to player (dominant-axis quantized) ---
        if (animator != null)
        {
            // Use enemy root as the origin so direction feels correct even if hitbox is offset
            Vector2 toPlayer = ((Vector2)pp.transform.position - (Vector2)transform.root.position).normalized;

            Vector2 attackDir;
            if (Mathf.Abs(toPlayer.x) >= Mathf.Abs(toPlayer.y))
            {
                // Left / Right wins when |x| >= |y|
                attackDir = new Vector2(Mathf.Sign(toPlayer.x), 0f);
            }
            else
            {
                // Up / Down otherwise
                attackDir = new Vector2(0f, Mathf.Sign(toPlayer.y));
            }

            // Push direction into Animator params used by the Attack blend tree
            animator.SetFloat(attackXParam, attackDir.x);
            animator.SetFloat(attackYParam, attackDir.y);

            // Fire the attack animation
            animator.SetTrigger(attackTriggerName);
        }

        // Apply contact damage + cooldown
        pp.TakeDamage(damage);
        nextTimeCanHit = Time.time + hitInterval;
    }
}
