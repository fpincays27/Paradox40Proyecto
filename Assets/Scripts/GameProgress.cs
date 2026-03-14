using System;
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    public bool HasWeapon { get; private set; }

    // Token genérico (sirve para cualquier sistema que requiera tokens)
    public int Tokens { get; private set; }

    public event Action<int> OnTokensChanged;

    [Header("Token Visual")]
    [SerializeField] private Sprite genericTokenSprite;
    public Sprite GenericTokenSprite => genericTokenSprite;

    // Persistencia de ojos
    public bool LeftEyeDestroyed { get; set; }
    public bool RightEyeDestroyed { get; set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GainWeapon() => HasWeapon = true;

    // GameProgress.GainToken()
    public void GainToken(int amount = 1)
    {
        int add = Mathf.Max(0, amount);
        if (add <= 0) return;
        
        Tokens += add;
        Debug.Log($"[GameProgress] GainToken +{add} => {Tokens}");
        OnTokensChanged?.Invoke(Tokens);
    }

    public bool TryConsumeToken(int amount = 1)
    {
        int consume = Mathf.Max(1, amount);
        if (Tokens < consume) return false;

        Tokens -= consume;
        OnTokensChanged?.Invoke(Tokens);
        return true;
    }

    public void ResetAll()
    {
        HasWeapon = false;
        Tokens = 0;
        LeftEyeDestroyed = false;
        RightEyeDestroyed = false;

        OnTokensChanged?.Invoke(Tokens);
    }
}