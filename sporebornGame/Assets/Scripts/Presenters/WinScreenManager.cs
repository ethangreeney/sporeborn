using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField]
    private GameObject winScreenPanel; // Victory panel to show

    [SerializeField]
    private GameObject mainHudPanel; // Your main UI canvas/HUD

    [Header("Optional: Boss Reference")]
    [Tooltip("Leave empty to auto-find boss in scene")]
    [SerializeField]
    private FinalBossController bossController;

    public void SetupWinScreen()
    {
        // Hide win screen initially
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(false);
        }

        // Auto-find boss if not assigned
        if (bossController == null)
        {
            bossController = FindFirstObjectByType<FinalBossController>();
        }

        // Subscribe to boss defeated event
        if (bossController != null)
        {
            bossController.onBossDefeated.AddListener(ShowWinScreen);
        }
        else
        {
            Debug.LogWarning("[WinScreenManager] No FinalBossController found in scene. Win screen will not activate.");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe when destroyed
        if (bossController != null)
        {
            bossController.onBossDefeated.RemoveListener(ShowWinScreen);
        }
    }

    private void ShowWinScreen()
    {
        // Hide the main game HUD
        if (mainHudPanel != null)
        {
            mainHudPanel.SetActive(false);
        }

        // Show the win screen panel
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
        }

        // Freeze the game
        Time.timeScale = 0f;

        Debug.Log("[WinScreenManager] Victory! Boss defeated.");
    }

    // Public method for button's OnClick() event
    public void GoToMainMenu()
    {
        // Unfreeze the game before changing scenes
        Time.timeScale = 1f;

        if (winScreenPanel != null)
            winScreenPanel.SetActive(false);

        if (mainHudPanel != null)
            mainHudPanel.SetActive(true);

        SceneManager.LoadScene(0); // Main menu scene index
    }

    // Public method for "Next Level" button if needed
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;

        // Load next scene (adjust index as needed)
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next level available");
            GoToMainMenu();
        }
    }
}