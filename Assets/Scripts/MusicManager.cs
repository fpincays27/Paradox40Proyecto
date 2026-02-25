using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips")]
    public AudioClip portadaMusic;
    public AudioClip salonPrincipalMusic;
    public AudioClip ronda1Music;
    public AudioClip ronda2Music;
    public AudioClip gameOverMusic;
    public AudioClip winMusic;

    [Header("Fade Settings")]
    [SerializeField] private float fadeOutTime = 0.6f;
    [SerializeField] private float fadeInTime = 0.6f;
    [SerializeField] private float targetVolume = 1f;

    private AudioSource source;
    private Coroutine fadeRoutine;

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
        source.loop = true;
        source.playOnAwake = false;

        // Si quieres que siempre arranque con este volumen
        source.volume = targetVolume;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Evita leaks si destruyen este objeto
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Portada":
                PlayWithFade(portadaMusic);
                break;

            case "SalonPrincipal":
                PlayWithFade(salonPrincipalMusic);
                break;

            case "Ronda1":
                PlayWithFade(ronda1Music);
                break;

            case "Ronda2":
                PlayWithFade(ronda2Music);
                break;

            case "GameOver":
                PlayWithFade(gameOverMusic, loop: false);
                break;

            case "Win":
                PlayWithFade(winMusic, loop: false);
                break;
        }
    }

    public void PlayWithFade(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        // Si ya está sonando ese mismo clip, no hagas nada
        if (source.clip == clip && source.isPlaying) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeToClip(clip, loop));
    }

    private IEnumerator FadeToClip(AudioClip newClip, bool loop)
    {
        // Fade Out
        float startVol = source.volume;

        if (source.isPlaying && fadeOutTime > 0f)
        {
            float t = 0f;
            while (t < fadeOutTime)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVol, 0f, t / fadeOutTime);
                yield return null;
            }
        }

        source.Stop();
        source.clip = newClip;
        source.loop = loop;
        source.Play();

        // Fade In
        if (fadeInTime > 0f)
        {
            float t = 0f;
            while (t < fadeInTime)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, t / fadeInTime);
                yield return null;
            }
        }

        source.volume = targetVolume;
        fadeRoutine = null;
    }
}