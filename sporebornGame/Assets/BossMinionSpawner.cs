using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Spawns Phase 2 helpers for the Final Boss:
/// - 2 Chargers + 2 Melee (configurable).
/// - Uses MapPresenter.GetSpawnLocations() for legal floor tiles in the current room.
/// - Tracks alive minions so the boss can advance to Phase 3 when all are gone.
/// Attach this to the Boss GameObject and wire it in FinalBossController.minionSpawner.
/// </summary>
public class BossMinionSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject chargerPrefab;
    public GameObject meleePrefab;

    [Header("Counts")]
    public int chargerCount = 2;
    public int meleeCount = 2;

    [Header("Placement Rules")]
    [Tooltip("Minimum distance from the boss to place a minion.")]
    public float minDistanceFromBoss = 2.0f;
    [Tooltip("Minimum distance from the player to place a minion.")]
    public float minDistanceFromPlayer = 2.0f;

    [Tooltip("If true, new minions become children of this spawner for easy cleanup.")]
    public bool parentMinionsToSpawner = true;

    // Internals
    private readonly List<GameObject> _activeMinions = new List<GameObject>();
    private MapPresenter _map;           // auto-found
    private Transform _player;           // auto-found by tag
    private Transform _boss;             // this.transform

    public bool HasActiveMinions
    {
        get
        {
            CleanupMinionList();
            return _activeMinions.Count > 0;
        }
    }

    private void Awake()
    {
        _map = FindFirstObjectByType<MapPresenter>();
        var playerGo = GameObject.FindGameObjectWithTag("Player");
        _player = playerGo ? playerGo.transform : null;
        _boss = transform;
    }

    /// <summary>
    /// Called by FinalBossController during Phase 2.
    /// Spawns the configured mix of minions on valid tiles.
    /// Safe to call once per Phase 2 entry.
    /// </summary>
    public void SpawnWave()
    {
        if (!_map)
        {
            Debug.LogWarning("[BossMinionSpawner] No MapPresenter found in scene.");
            return;
        }

        // Get all valid floor tiles from the active room (centered world positions).
        // This comes from MapPresenter.GetSpawnLocations()
        var spawnTiles = _map.GetSpawnLocations() ?? new List<Vector3>(); // returns world-centers of valid tiles
        if (spawnTiles.Count == 0)
        {
            Debug.LogWarning("[BossMinionSpawner] No spawnable tiles found.");
            return;
        }

        // Shuffle tiles for randomness
        spawnTiles = spawnTiles.OrderBy(_ => Random.value).ToList();

        int needed = chargerCount + meleeCount;
        var chosen = PickSpawnPositions(spawnTiles, needed);

        int idx = 0;
        // Spawn Chargers
        for (int i = 0; i < chargerCount && idx < chosen.Count; i++, idx++)
        {
            TrySpawn(chargerPrefab, chosen[idx]);
        }
        // Spawn Melee
        for (int i = 0; i < meleeCount && idx < chosen.Count; i++, idx++)
        {
            TrySpawn(meleePrefab, chosen[idx]);
        }

        CleanupMinionList(); // in case some failed instantly
        Debug.Log($"[BossMinionSpawner] Spawned {_activeMinions.Count} minions.");
    }

    private List<Vector3> PickSpawnPositions(List<Vector3> tiles, int needed)
    {
        var result = new List<Vector3>(needed);
        foreach (var pos in tiles)
        {
            if (result.Count >= needed) break;

            // Distance checks
            if (_boss && Vector2.Distance(_boss.position, pos) < minDistanceFromBoss) continue;
            if (_player && Vector2.Distance(_player.position, pos) < minDistanceFromPlayer) continue;

            result.Add(pos);
        }
        return result;
    }

    private void TrySpawn(GameObject prefab, Vector3 pos)
    {
        if (!prefab) return;

        var go = Instantiate(prefab, pos, Quaternion.identity);
        if (parentMinionsToSpawner) go.transform.SetParent(transform);

        _activeMinions.Add(go);

        // Optional: if minions have HealthModel, drop a tiny watcher to auto-clean on death.
        var hm = go.GetComponent<HealthModel>();
        if (hm != null)
        {
            // Local function to detach when dead
            void OnHealthChanged()
            {
                if (hm.currHealth <= 0f) CleanupMinionList();
            }
            hm.OnHealthChanged += OnHealthChanged;

            // Cleanly remove the handler when the minion is destroyed
            var remover = go.AddComponent<_MinionCleanupHook>();
            remover.Init(hm, OnHealthChanged, () => _activeMinions.Remove(go));
        }
    }

    private void CleanupMinionList()
    {
        for (int i = _activeMinions.Count - 1; i >= 0; i--)
        {
            var go = _activeMinions[i];
            if (go == null)
            {
                _activeMinions.RemoveAt(i);
                continue;
            }

            var hm = go.GetComponent<HealthModel>();
            if (hm != null && hm.currHealth <= 0f)
            {
                _activeMinions.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Small helper component to detach event handlers and prune list on destroy.
    /// </summary>
    private class _MinionCleanupHook : MonoBehaviour
    {
        private HealthModel _hm;
        private System.Action _onHealthChangedDetach;
        private System.Action _onDestroyCallback;

        public void Init(HealthModel hm, System.Action onHealthChangedDetach, System.Action onDestroyCallback)
        {
            _hm = hm;
            _onHealthChangedDetach = onHealthChangedDetach;
            _onDestroyCallback = onDestroyCallback;
        }

        private void OnDestroy()
        {
            if (_hm != null && _onHealthChangedDetach != null)
                _hm.OnHealthChanged -= _onHealthChangedDetach;
            _onDestroyCallback?.Invoke();
        }
    }
}

