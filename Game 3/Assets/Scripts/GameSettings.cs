using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    [Range(0.05f, 1.25f)] public float mouseSens = 1.0f;

    public enum MovementStyle
    {
        DEFAULT, // WASD used to aim cursor, no movement influence midair
        PRECISE // Mouse aims cursor, WASD influences aerial movement slightly
    }

    public MovementStyle CurrentMovementStyle { get; private set; } = MovementStyle.DEFAULT;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        SoundManager.Instance?.SetSFXVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        SoundManager.Instance?.SetMusicVolume(volume);
    }

    public void SetMovementStyle(MovementStyle style)
    {
        CurrentMovementStyle = style;
        Debug.Log($"<Game Settings> Movement style set to {style}");
    }

    public void SetMouseSens(float sens)
    {
        // Mouse Sensitivity to be used under the PRECISE style
        mouseSens = sens;
    }
}
