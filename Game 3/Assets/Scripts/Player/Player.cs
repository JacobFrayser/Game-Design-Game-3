using UnityEngine;

public class Player : MonoBehaviour
{
    public void Die()
    {
        // Since RespawnManagers are per-scene, always check if one is present before respawning
        if (RespawnManager.Instance == null)
        {
            Debug.LogError("<Player> No RespawnManager found in scene!");
            return;
        }

        RespawnManager.Instance.Respawn(this);
    }

    public void SetMovementEnabled(bool enabled)
    {
        PlayerMotor motor = GetComponentInChildren<PlayerMotor>();
        if (motor == null) return;

        if (enabled)
        {
            motor.enabled = true;
        }
        else
        {
            motor.enabled = false;
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }
}
