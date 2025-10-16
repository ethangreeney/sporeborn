using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField]
    private GameObject deathScreenPanel; // Assign your "Death screen panel" here

    [SerializeField]
    private GameObject mainHudPanel; // Assign your "UICanvas" here

    // OnEnable is called when this script's GameObject becomes active.
    private void OnEnable()
    {

        PlayerPresenter.OnPlayerDied += ShowDeathScreen;
    }

    // OnDisable is called when the object is disabled or destroyed.
    private void OnDisable()
    {

        PlayerPresenter.OnPlayerDied -= ShowDeathScreen;
    }

    // You can use Start or Awake to make sure the UI is in the correct state initially.
    void Start()
    {
        // Make sure the death screen is hidden when the game starts.
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }
    }

    private void ShowDeathScreen()
    {

        // Hide the main game HUD
        if (mainHudPanel != null)
        {
            mainHudPanel.SetActive(false);
        }

        // Show the death screen panel
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
        }

        // Freeze the game
        Time.timeScale = 0f;
    }

    // --- Public method for your button's OnClick() event ---
    public void GoToMainMenu()
    {
        // IMPORTANT: Unfreeze the game before changing scenes!
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Assuming 0 is your main menu scene index
    }
}