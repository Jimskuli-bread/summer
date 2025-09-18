using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;

    private static MusicManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(musicSource);
            if (musicSource != null && !musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }
}