using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseUI;
    public GameObject optionsUI;
    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                // If options panel is opened when escape is pressed, exit just the
                // options menu back to the primary pause menu
                if (optionsUI.activeSelf)
                {
                    CloseOptions();
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        pauseUI.SetActive(true);
        optionsUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        pauseUI.SetActive(false);
        optionsUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenOptions()
    {
        pauseUI.SetActive(false);
        optionsUI.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsUI.SetActive(false);
        pauseUI.SetActive(true);
    }

    public void SetStyleDefault()
    {
        GameSettings.Instance?.SetMovementStyle(GameSettings.MovementStyle.DEFAULT);
    }

    public void SetStylePrecise()
    {
        GameSettings.Instance?.SetMovementStyle(GameSettings.MovementStyle.PRECISE);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}