using UnityEngine;
using TMPro;

public class WelcomeScreen : MonoBehaviour
{
    public GameObject welcomePanel;
    public TextMeshProUGUI pressKeyText;
    private static bool hasShown = false;
    private float blinkTimer;

    void Start()
    {
        if (!hasShown)
        {
            welcomePanel.SetActive(true);
            Time.timeScale = 0f;
            hasShown = true;
        }
        else
        {
            welcomePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (welcomePanel.activeSelf)
        {
            // Blink effect for "Press any key" text
            if (pressKeyText != null)
            {
                blinkTimer += Time.unscaledDeltaTime;
                Color color = pressKeyText.color;
                color.a = Mathf.PingPong(blinkTimer * 0.8f, 1f);
                pressKeyText.color = color;
            }

            if (Input.anyKeyDown)
            {
                welcomePanel.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }
}

