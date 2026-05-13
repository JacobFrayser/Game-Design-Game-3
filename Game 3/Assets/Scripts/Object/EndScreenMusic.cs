using UnityEngine;

public class EndScreenMusic : MonoBehaviour
{
    public AudioClip endMusic;

    void Start()
    {
        SoundManager.Instance?.PlayMusic(endMusic);
    }
}
