using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    // Respawn position left public so it can be set per-screen once that's implemented
    // Set via a RespawnPoint game object
    public Transform spawnPoint;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    public void Respawn(Player player)
    {
        // Catch missing spawn point transform
        if (spawnPoint == null)
        {
            Debug.LogError("<Respawn Manager> No spawn point assigned!", this);
            return;
        }

        // Set player position to spawn point and cancel all momentum
        player.transform.position = spawnPoint.position;
        player.GetComponentInChildren<PlayerMotor>().SetVelocity(Vector2.zero);

        // Re-enable all Blue Orbs within this level that were previously collected
        BlueOrb[] orbs = FindObjectsByType<BlueOrb>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (BlueOrb orb in orbs)
        {
            orb.gameObject.SetActive(true);
        }

        Debug.Log($"<Respawn Manager> Respawned player at {spawnPoint.position} and " + $"re-enabled {orbs.Length} orb(s).");
    }

    public void SetSpawnPoint(Transform newSpawn)
    {
        spawnPoint = newSpawn;
        Debug.Log($"<Respawn Manager> Spawn Point updated to {newSpawn.position}");
    }
}
