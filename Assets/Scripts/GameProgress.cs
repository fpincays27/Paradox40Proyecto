using System;
using System.Collections.Generic;
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    public bool HasWeapon { get; private set; }

    // Token genérico (sistemas como shuffle, etc.)
    public int Tokens { get; private set; }
    public event Action<int> OnTokensChanged;

    // Nuevo token de curación
    public int HealTokens { get; private set; }
    public event Action<int> OnHealTokensChanged;

    [Header("Token Visual")]
    [SerializeField] private Sprite genericTokenSprite;
    public Sprite GenericTokenSprite => genericTokenSprite;

    [Serializable]
    public struct EyeRoundState
    {
        public bool LeftDestroyed;
        public bool RightDestroyed;
    }

    private readonly Dictionary<int, EyeRoundState> eyeStatesByRound = new();

    // Si viene de una ronda al Salón, define a qué ronda debe volver.
    public string PendingSalonReturnScene { get; set; } = string.Empty;

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

    public EyeRoundState GetEyeState(int roundId)
    {
        if (eyeStatesByRound.TryGetValue(roundId, out var state))
            return state;

        return default;
    }

    public void SetEyeDestroyed(int roundId, bool isLeftEye)
    {
        EyeRoundState state = GetEyeState(roundId);

        if (isLeftEye) state.LeftDestroyed = true;
        else state.RightDestroyed = true;

        eyeStatesByRound[roundId] = state;
    }

    public void ClearEyeState(int roundId)
    {
        eyeStatesByRound.Remove(roundId);
    }

    // Token genérico
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

    // Token de curación
    public void GainHealToken(int amount = 1)
    {
        int add = Mathf.Max(0, amount);
        if (add <= 0) return;

        HealTokens += add;
        Debug.Log($"[GameProgress] GainHealToken +{add} => {HealTokens}");
        OnHealTokensChanged?.Invoke(HealTokens);
    }

    public bool TryConsumeHealToken(int amount = 1)
    {
        int consume = Mathf.Max(1, amount);
        if (HealTokens < consume) return false;

        HealTokens -= consume;
        OnHealTokensChanged?.Invoke(HealTokens);
        return true;
    }

    public void ResetAll()
    {
        HasWeapon = false;
        Tokens = 0;
        HealTokens = 0;
        PendingSalonReturnScene = string.Empty;
        eyeStatesByRound.Clear();

        OnTokensChanged?.Invoke(Tokens);
        OnHealTokensChanged?.Invoke(HealTokens);
    }
}