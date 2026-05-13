using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    // Sprites
    public Sprite canisterCharged;
    public Sprite canisterDepleted;
    public Image canisterImage;

    private PlayerMotor motor;

    void Start()
    {
        // Find motor on player
        motor = FindFirstObjectByType<PlayerMotor>();

        if (motor == null)
        {
            Debug.LogWarning("<Player HUD> No PlayerMotor found!");
        }
    }

    void Update()
    {
        if (motor == null || canisterImage == null)
        {
            return;
        }

        canisterImage.sprite = motor.hasPulseGunCharge
            ? canisterCharged
            : canisterDepleted;
    }
}
