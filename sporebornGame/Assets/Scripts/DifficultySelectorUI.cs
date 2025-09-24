using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DifficultySelectorUI : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;

    void Awake()
    {
        if (!dropdown) dropdown = GetComponent<TMP_Dropdown>();
        if (!dropdown) return;

        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string> { "Easy", "Medium", "Hard", "Impossible" });

        var current = DifficultyManager.Instance ? DifficultyManager.Instance.Current : Difficulty.Medium;
        dropdown.value = (int)current;
        dropdown.onValueChanged.AddListener(OnChanged);
    }

    void OnChanged(int index)
    {
        if (!DifficultyManager.Instance) return;
        DifficultyManager.Instance.SetDifficulty((Difficulty)index);
    }
}