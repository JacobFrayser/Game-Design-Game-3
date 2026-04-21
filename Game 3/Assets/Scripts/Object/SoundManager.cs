using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set;  }

    private AudioSource audioSource;

    private void Awake()
    {
        // If instance of sound manager exists already, remove it since there should only ever be one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void PlaySound(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, volumeScale);
    }
}
