using UnityEngine;
using System.Collections;

public class LevelGoal : MonoBehaviour
{
    public string nextScene;

    public AudioClip triggerSound;

    public float flashDuration = 0.2f;

    public float pauseDuration = 0.4f;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        Player player = other.GetComponentInParent<Player>();
        if (player == null) return;

        triggered = true;
        StartCoroutine(GoalSequence(player));
    }

    private IEnumerator GoalSequence (Player player)
    {
        // Disable player movement altogether while running the sequence
        player.SetMovementEnabled(false);

        // Play the trigger sound
        SoundManager.Instance?.PlaySound(triggerSound);

        // Short white flash, handled in ScreenFader
        ScreenFader.Instance.Flash(Color.white, flashDuration);
        yield return new WaitForSeconds(flashDuration);

        // Small pause before level transition
        yield return new WaitForSeconds(pauseDuration);

        // Fade to black, load next scene
        if (!string.IsNullOrEmpty(nextScene))
        {
            ScreenFader.Instance.FadeToScene(nextScene);
        }
        else
        {
            Debug.LogWarning("<Level Goal> No next scene assigned!");
        }
    }
}
