using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }
    public Difficulty Current { get; private set; } = Difficulty.Medium;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Current = (Difficulty)PlayerPrefs.GetInt("difficulty", (int)Difficulty.Medium);
    }

    public void SetDifficulty(Difficulty d)
    {
        Current = d;
        PlayerPrefs.SetInt("difficulty", (int)d);
    }

    public float PlayerMaxHealth => Current switch
    {
        Difficulty.Easy => 12f,
        Difficulty.Medium => 10f,
        Difficulty.Hard => 8f,
        Difficulty.Impossible => 6f,
        _ => 10f
    };
    public float EnemyFireIntervalMult => Current switch
    {
        Difficulty.Easy => 1.25f,
        Difficulty.Medium => 1f,
        Difficulty.Hard => 0.85f,
        Difficulty.Impossible => 0.65f,
        _ => 1f
    };
    public float PlayerFireIntervalMult => Current switch
    {
        Difficulty.Easy => 0.85f,
        Difficulty.Medium => 1f,
        Difficulty.Hard => 1.1f,
        Difficulty.Impossible => 1.25f,
        _ => 1f
    };
}