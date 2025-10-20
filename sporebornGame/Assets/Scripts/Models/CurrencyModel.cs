using System;
using UnityEngine;

public class CurrencyModel : MonoBehaviour
{

    public static event Action<int> OnCurrencyChanged;

    [SerializeField]
    private int _currentNectar;
    public int CurrentNectar
    {
        get { return _currentNectar; }
        private set
        {
            _currentNectar = value;
            // Fire the event whenever the value is changed!
            // The '?' checks if any script is currently listening before trying to fire.
            OnCurrencyChanged?.Invoke(_currentNectar);
        }
    }

    public CurrencyModel()
    {
        CurrentNectar = 0;
    }

    private void Awake()
    {
        // When the game starts, set the initial value.
        // We use the property so the initial event fires and UI updates itself.
        CurrentNectar = _currentNectar;
    }

    public int GetCurrentNectar()
    {
        return CurrentNectar;
    }

    public void AddCurrency(int amount)
    {
        // Use the property, not the private field, to ensure the event fires.
        CurrentNectar += amount;
    }

    public void RemoveCurrency(int amount)
    {
        CurrentNectar -= amount;
    }
}