using UnityEngine;
using TMPro;

public class HealButton2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HealthManager health;
    [SerializeField] private GameObject buttonVisualRoot;   // opcional
    [SerializeField] private TextMeshProUGUI amountText;    // opcional (ej. "x2")

    [Header("Cost / Effect")]
    [SerializeField] private int healTokenCost = 1;
    [SerializeField] private int healAmount = 5;

    [Header("Behavior")]
    [SerializeField] private bool hideWhenNoTokens = true;
    [SerializeField] private bool verboseLogs = true;

    private void OnEnable()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnHealTokensChanged += HandleHealTokensChanged;

        RefreshButtonState();
    }

    private void OnDisable()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnHealTokensChanged -= HandleHealTokensChanged;
    }

    private void Start()
    {
        RefreshButtonState();
    }

    private void OnMouseDown()
    {
        TryUseHeal();
    }

    public void TryUseHeal()
    {
        if (GameProgress.Instance == null || health == null) return;

        int cost = Mathf.Max(1, healTokenCost);
        if (!GameProgress.Instance.TryConsumeHealToken(cost))
        {
            if (verboseLogs) Debug.Log("[HealButton2D] No hay heal tokens suficientes.");
            RefreshButtonState();
            return;
        }

        bool healed = health.RecoverPlayerHP(healAmount);

        if (verboseLogs)
            Debug.Log($"[HealButton2D] Consumo {cost} heal token(s). Curación aplicada: {healed}.", this);

        RefreshButtonState();
    }

    private void HandleHealTokensChanged(int current)
    {
        RefreshButtonState();
    }

    private void RefreshButtonState()
    {
        int current = (GameProgress.Instance != null) ? GameProgress.Instance.HealTokens : 0;
        bool canUse = current >= Mathf.Max(1, healTokenCost);

        if (buttonVisualRoot != null && hideWhenNoTokens)
            buttonVisualRoot.SetActive(canUse);

        if (amountText != null)
            amountText.text = $"x{current}";
    }
}