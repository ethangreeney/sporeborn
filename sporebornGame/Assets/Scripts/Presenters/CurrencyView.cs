using UnityEngine;
using TMPro; // Use this if you are using TextMeshPro

public class CurrencyView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI currencyText; // Assign your TextMeshProUGUI component in the Inspector

    // OnEnable is called when the object becomes active.
    // It's the perfect place to subscribe to events.
    private void OnEnable()
    {
        // Subscribe our UpdateCurrencyText method to the event.
        // Now, whenever OnCurrencyChanged is fired, UpdateCurrencyText will be called.
        CurrencyModel.OnCurrencyChanged += UpdateCurrencyText;
    }

    // OnDisable is called when the object is disabled or destroyed.
    // It's CRITICAL to unsubscribe here to prevent memory leaks.
    private void OnDisable()
    {
        // Unsubscribe from the event.
        CurrencyModel.OnCurrencyChanged -= UpdateCurrencyText;
    }

    // This method has the same "signature" (an int parameter) as our Action<int>.
    private void UpdateCurrencyText(int newAmount)
    {
        // Update the text with the new value from the event.
        currencyText.text = "Nectar: " + newAmount.ToString();
    }
}