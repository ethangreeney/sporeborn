using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class StatIcon : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI arrowText;

    public void Setup(Sprite sprite, bool modified, bool increased)
    {
        icon.sprite = sprite;

        if (!modified)
        {
            arrowText.text = "";
            arrowText.gameObject.SetActive(false);
        }
        else
        {
            arrowText.gameObject.SetActive(true);
            arrowText.text = increased ? "↑" : "↓";
            arrowText.color = increased ? Color.green : Color.red;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}