using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    public AudioClip menuMusic;

    void Start()
    {
        SoundManager.Instance.PlayMusic(menuMusic);
    }
}
