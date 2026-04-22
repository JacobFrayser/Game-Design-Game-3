using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("SoundManager");
                _instance = obj.AddComponent<SoundManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private static SoundManager _instance;

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 0.33f;
    [Range(0f, 1f)] public float musicVolume = 0.33f;

    // Controls whether the next music track to play will fade in or not
    // False by default so main menu music doesn't fade in
    private bool musicFadeIn = false;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private void Awake()
    {
        // If instance of sound manager exists already, remove it since there should only ever be one
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        SetupSources();
    }

    private void SetupSources()
    {
        // SFX source added by RequireComponent
        sfxSource = GetComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        // Music source added at runtime
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
    }
    
    // SFX-specific PlaySound method
    public void PlaySound(AudioClip clip, float volumeScale = 1f)
    {
        // Calling a null clip won't brick the system
        if (clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    // Music-specific PlayMusic method (separate to allow for checking current clip)
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        // Don't repeatedly start track if current track matches new track
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;

        // If music should fade in, start the coroutine, otherwise simply set music volume to setting
        if (musicFadeIn)
        {
            StartCoroutine(FadeInRoutine());
            musicFadeIn = false;
        }
        else
        {
            musicSource.volume = musicVolume;
        }
        
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void FadeOutMusic()
    {
        if (!musicSource.isPlaying)
        {
            return;
        }

        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume; // Set musicFadedOut to true so next track will fade in
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }
}
