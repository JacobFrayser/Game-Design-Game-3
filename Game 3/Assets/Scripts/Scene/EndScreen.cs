using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    private void Start()
    {
        // Enable cursor for navigation
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ReturnToMenu()
    {
        ScreenFader.Instance.FadeToScene("MainMenu");
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        #else
        {
            Application.Quit();
        }
        #endif
    }
}