using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public Sprite activatedSprite;

    // Prevents re-activation if player stays in trigger zone
    private bool hasBeenActivated = false;

    private SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasBeenActivated) return;

        Player player = collision.GetComponentInParent<Player>();
        if (player == null) return;

        if (RespawnManager.Instance == null)
        {
            Debug.LogWarning("<Respawn Point> No RespawnManager in scene!");
            return;
        }

        RespawnManager.Instance.SetSpawnPoint(transform);
        hasBeenActivated = true;

        if (sprite != null && activatedSprite != null)
        {
            sprite.sprite = activatedSprite;
        }
    }
}
