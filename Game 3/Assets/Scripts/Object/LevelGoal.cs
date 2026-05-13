using UnityEngine;
using System.Collections;

public class LevelGoal : MonoBehaviour
{
    public string nextScene;

    public AudioClip triggerSound;
    public AudioClip finalTriggerSound;

    // Length of time that the small white flash pops up when triggering the goal
    public float flashDuration = 0.2f;

    // Length of time that the game will wait before fading to black and transitioning to the next scene
    public float pauseDuration = 0.4f;
    public float finalPauseDuration = 1.0f;

    // Used to run different sfx and coroutines if the goal is in the final level
    public bool isFinalLevel = false;

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

        // Select and play the correct trigger sound
        if (isFinalLevel)
        {
            SoundManager.Instance?.StopMusic();
            SoundManager.Instance?.PlaySound(finalTriggerSound);
        }
        else
        {
            SoundManager.Instance?.PlaySound(triggerSound);
        }

        // Short white flash, handled in ScreenFader
        ScreenFader.Instance.Flash(Color.white, flashDuration);
        yield return new WaitForSeconds(flashDuration);

        // Small pause before transition
        if (isFinalLevel)
        {
            yield return new WaitForSeconds(finalPauseDuration);
        }
        else
        {
            yield return new WaitForSeconds(pauseDuration);
        }

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
