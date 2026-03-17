using UnityEngine;

public class HealTokenPickup : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private int healTokensToGive = 1;

    [Header("Behavior")]
    [SerializeField] private bool oneTimeOnly = true;
    [SerializeField] private bool disableAfterClaim = true;
    [SerializeField] private bool verboseLogs = true;

    private bool claimed;

    private void OnMouseDown()
    {
        if (oneTimeOnly && claimed) return;

        if (GameProgress.Instance == null)
        {
            Debug.LogWarning($"[{name}] No se encontró GameProgress.Instance.", this);
            return;
        }

        int amount = Mathf.Max(0, healTokensToGive);
        if (amount <= 0) return;

        GameProgress.Instance.GainHealToken(amount);

        if (verboseLogs)
            Debug.Log($"[{name}] HealToken +{amount}. Total heal tokens: {GameProgress.Instance.HealTokens}", this);

        claimed = true;

        if (disableAfterClaim)
            gameObject.SetActive(false);
    }
}