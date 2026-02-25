using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager I;

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip match;
    public AudioClip noMatch;
    public AudioClip dragCard;
    public AudioClip shuffleDeck;
    public AudioClip weaponShot;
    public AudioClip weaponSpawn;
    public AudioClip keyReveal;
    public AudioClip eyeImpact;

    // ✅ NUEVOS (si ya los tienes, déjalos)
    public AudioClip drawCard;   // Spacebar -> robar carta
    public AudioClip cardFlip;   // Flip de carta

    // ✅ NUEVO: cofre
    public AudioClip chestSpawn; // Spawn del cofre

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // Helpers opcionales (más cómodo)
    public void PlayMatch()        => Play(match);
    public void PlayNoMatch()      => Play(noMatch);
    public void PlayDragCard()     => Play(dragCard, 0.8f);
    public void PlayShuffle()      => Play(shuffleDeck);
    public void PlayShot()         => Play(weaponShot);
    public void PlayWeaponSpawn()  => Play(weaponSpawn);
    public void PlayKeyReveal()    => Play(keyReveal);
    public void PlayEyeImpact()    => Play(eyeImpact);

    // ✅ Extras
    public void PlayDrawCard()     => Play(drawCard, 0.9f);
    public void PlayCardFlip()     => Play(cardFlip, 0.85f);

    // ✅ NUEVO: cofre
    public void PlayChestSpawn()   => Play(chestSpawn, 1f);
}