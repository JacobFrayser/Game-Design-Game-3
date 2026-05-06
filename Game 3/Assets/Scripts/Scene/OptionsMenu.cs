using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Sliders")]
    public Slider sfxSlider;
    public Slider musicSlider;

    [Header("Movement Style")]
    public TMPro.TextMeshProUGUI movementStyleLabel;
    public TMPro.TextMeshProUGUI movementStyleDesc;

    private string defaultStyleText = "Default Movement Style:\nThis style allows you to control\nwhere your Pulse Gun fires\nusing WASD. You can only\nfire your Pulse Gun while\nairborne.";
    private string preciseStyleText = "Precise Movement Style:\nThis style allows you to control\nwhere your Pulse Gun fires\nusing your mouse. You can\nfire your Pulse Gun at any\ntime and also gain the\nability to slightly maneuver yourself\nwhile airborne using WASD.";

    private void OnEnable()
    {
        // fires every time panel is shown, allows for sliders to sync to current configuration
        if (GameSettings.Instance == null)
        {
            Debug.LogWarning("<Options Panel> Game Settings not ready!");
            return;
        }

        sfxSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();

        sfxSlider.value = GameSettings.Instance.sfxVolume;
        musicSlider.value = GameSettings.Instance.musicVolume;

        sfxSlider.onValueChanged.AddListener(GameSettings.Instance.SetSFXVolume);
        musicSlider.onValueChanged.AddListener(GameSettings.Instance.SetMusicVolume);

        RefreshMovementStyleLabel();
    }

    public void ToggleMovementStyle()
    {
        if (GameSettings.Instance == null)
        {
            return;
        }

        var next = GameSettings.Instance.CurrentMovementStyle == GameSettings.MovementStyle.DEFAULT
            ? GameSettings.MovementStyle.PRECISE
            : GameSettings.MovementStyle.DEFAULT;

        GameSettings.Instance.SetMovementStyle(next);
        RefreshMovementStyleLabel();
    }

    private void RefreshMovementStyleLabel()
    {
        if (movementStyleLabel == null || GameSettings.Instance == null)
        {
            return;
        }

        movementStyleLabel.text = GameSettings.Instance.CurrentMovementStyle == GameSettings.MovementStyle.DEFAULT
            ? "Movement Style: Default"
            : "Movement Style: Precise";

        movementStyleDesc.text = GameSettings.Instance.CurrentMovementStyle == GameSettings.MovementStyle.DEFAULT
            ? defaultStyleText
            : preciseStyleText;
    }
}
