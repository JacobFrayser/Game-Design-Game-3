using UnityEngine;

public class DeathZone : MonoBehaviour
{
    // Simple class that allows specific geometry to be considered a "death zone"
    // Think spikes, lava, black hole, etc.
    // Geometry with this property must also have a Collider2D of some sort set to a Trigger type
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Player player = collider.GetComponent<Player>();
        if (player == null)
        {
            return;
        }

        player.Die();
    }
}
