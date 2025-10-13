using UnityEngine;

public class CurrencyModel : MonoBehaviour
{
    [SerializeField]
    private int CurrentNectar;

    public CurrencyModel()
    {
        CurrentNectar = 0;
    }

    public int GetCurrentNectar()
    {
        return CurrentNectar;
    }

    public void AddCurrency(int amount)
    {
        CurrentNectar += amount;
    }

    public void RemoveCurrency(int amount)
    {
        CurrentNectar -= amount;
    }
}