using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips")]
    public AudioClip gameOverMusic;
    public AudioClip introMusic;
    public AudioClip pasilloMusic;
    public AudioClip portadaMusic;
    public AudioClip ronda0Music;
    public AudioClip ronda1Music;
    public AudioClip ronda2Music;
    public AudioClip ronda3Music;
    public AudioClip ronda4Music;
    public AudioClip ronda4TransitionMusic;
    public AudioClip salonPrincipalMusic;
    public AudioClip transitionFinalMusic;
    public AudioClip transitionPasilloMusic;
    public AudioClip transitionSalonMusic;
    public AudioClip tutorialMusic;
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
            case "GameOver":
                PlayWithFade(gameOverMusic, loop: false);
                break;

            case "Intro":
                PlayWithFade(introMusic);
                break;

            case "Pasillo":
                PlayWithFade(pasilloMusic);
                break;

            case "Portada":
                PlayWithFade(portadaMusic);
                break;

            case "Ronda0":
                PlayWithFade(ronda0Music);
                break;

            case "Ronda1":
                PlayWithFade(ronda1Music);
                break;

            case "Ronda2":
                PlayWithFade(ronda2Music);
                break;

            case "Ronda3":
                PlayWithFade(ronda3Music);
                break;

            case "Ronda4":
                PlayWithFade(ronda4Music);
                break;

            case "Ronda4Transition":
                PlayWithFade(ronda4TransitionMusic);
                break;

            case "SalonPrincipal":
                PlayWithFade(salonPrincipalMusic);
                break;

            case "TransitionFinal":
                PlayWithFade(transitionFinalMusic);
                break;

            case "TransitionPasillo":
                PlayWithFade(transitionPasilloMusic);
                break;

            case "TransitionSalon":
                PlayWithFade(transitionSalonMusic);
                break;

            case "Tutorial":
                PlayWithFade(tutorialMusic);
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