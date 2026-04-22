using UnityEngine;

public class LevelMusic : MonoBehaviour
{
    public AudioClip levelMusic;

    void Start()
    {
        SoundManager.Instance.PlayMusic(levelMusic);
    }
}
