using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Sliders")]
    public Slider sfxSlider;
    public Slider musicSlider;
    public Slider sensSlider;

    [Header("Descriptions and Labels")]
    public TMPro.TextMeshProUGUI movementStyleLabel;
    public TMPro.TextMeshProUGUI movementStyleDesc;
    public TMPro.TextMeshProUGUI sensLabel;

    private string defaultStyleText = "Default Movement Style:\nThis style allows you to control\nwhere your Pulse Gun fires\nusing WASD. You can only\nfire your Pulse Gun while\nairborne.";
    private string preciseStyleText = "Precise Movement Style:\nThis style allows you to control\nwhere your Pulse Gun fires\nusing your mouse. You can\nadditionally fire your\nPulse Gun while on a surface.";

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
        sensSlider.onValueChanged.RemoveAllListeners();

        sfxSlider.value = GameSettings.Instance.sfxVolume;
        musicSlider.value = GameSettings.Instance.musicVolume;
        musicSlider.value = GameSettings.Instance.mouseSens;

        sfxSlider.onValueChanged.AddListener(GameSettings.Instance.SetSFXVolume);
        musicSlider.onValueChanged.AddListener(GameSettings.Instance.SetMusicVolume);
        sensSlider.onValueChanged.AddListener(OnSensitivityChanged);

        RefreshMovementStyleLabel();
        RefreshSensLabel(GameSettings.Instance.mouseSens);
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

    private void OnSensitivityChanged(float value)
    {
        GameSettings.Instance?.SetMouseSens(value);
        RefreshSensLabel(value);
    }

    private void RefreshSensLabel(float value)
    {
        if (sensLabel != null)
        {
            sensLabel.text = value.ToString("F2");
        }
    }
}
