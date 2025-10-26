using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    public EnemyModel model;  // optional: drag the parent here in Inspector

    void Awake()
    {
        if (model == null) model = GetComponentInParent<EnemyModel>();
    }

    // Called by an Animation Event at the very end of the Death clip
    public void OnDeathAnimationComplete()
    {
        if (model != null) model.OnDeathAnimationComplete();
    }
}
