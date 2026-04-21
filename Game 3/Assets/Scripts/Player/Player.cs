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
}
