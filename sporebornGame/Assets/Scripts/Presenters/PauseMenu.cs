using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; 

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenuUi;
    public GameObject settingsMenuUI;


    // Update is called once per frame
    void Update()
    {
        // Listen for the Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // CASE 1: The settings menu is currently open
            if (settingsMenuUI.activeSelf)
            {
                // Pressing Escape in settings should act like a "Back" button
                CloseSettings();
            }
            // CASE 2: The game is already paused (and settings is not open)
            else if (isPaused)
            {
                // Pressing Escape should resume the game
                Resume();
            }
            // CASE 3: The game is currently playing
            else
            {
                // Pressing Escape should pause the game
                Pause();
            }
        }
    }

    public void OpenSettings()
    {
        pauseMenuUi.SetActive(false);
        settingsMenuUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUi.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Resume()
    {
        pauseMenuUi.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

    }

    public void Pause()
    {
        pauseMenuUi.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(0);

    }

    
}
