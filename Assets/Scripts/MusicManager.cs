using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips")]
    public AudioClip portadaMusic;
    public AudioClip rondaMusic;
    public AudioClip gameOverMusic;
    public AudioClip winMusic;

    private AudioSource source;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Portada":
                Play(portadaMusic);
                break;

            case "Ronda1":
                Play(rondaMusic);
                break;

            case "GameOver":
                Play(gameOverMusic);
                break;

            case "Win":
                Play(winMusic);
                break;
        }
    }

    void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (source.clip == clip) return;

        source.Stop();
        source.clip = clip;
        source.Play();
    }
}