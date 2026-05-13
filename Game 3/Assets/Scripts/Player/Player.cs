using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    // SFX to play on death
    public AudioClip deathSFX;

    // Player's sprite
    public SpriteRenderer spriteRenderer;

    // isDead bool fixes a bug where touching multiple hitboxes for spikes at once would cause multiple instances of the death sfx
    private bool isDead = false;

    public void Die()
    {
        // Since RespawnManagers are per-scene, always check if one is present before respawning
        if (RespawnManager.Instance == null)
        {
            Debug.LogError("<Player> No RespawnManager found in scene!");
            return;
        }

        if (isDead) return;

        isDead = true;

        StartCoroutine(DeathRoutine());
    }

    public IEnumerator DeathRoutine()
    {
        // Disable movement for a short time
        SetMovementEnabled(false);
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        // Play sfx
        SoundManager.Instance?.PlaySound(deathSFX);

        // Wait for sound to play some before beginning the fade (sit with what you've done)
        yield return new WaitForSeconds(0.5f);

        // Fade to black, respawn when fading to black is complete, fade back to transparent
        ScreenFader.Instance?.FadeToBlack(0.3f, () =>
        {
            RespawnManager.Instance.Respawn(this);
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            SetMovementEnabled(true);
            isDead = false;
            ScreenFader.Instance?.FadeFromBlack(0.3f);
        });
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
