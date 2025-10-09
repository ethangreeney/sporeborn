using Unity.VisualScripting;

public class CurrencyModel
{
    public int CurrentNectar;

    public CurrencyModel()
    {
        CurrentNectar = 0;
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