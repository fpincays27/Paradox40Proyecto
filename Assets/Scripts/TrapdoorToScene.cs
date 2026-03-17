using UnityEngine;

public class TrapdoorToScene : MonoBehaviour
{
    [Header("Scene Load")]
    [SerializeField] private string gameSceneName = "Ronda3";

    [Header("State")]
    [SerializeField] private bool startsUnlocked = false;

    [Header("Click")]
    [SerializeField] private Collider clickableCollider;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = true;

    private bool _isUnlocked;

    private void Awake()
    {
        if (clickableCollider == null)
            clickableCollider = GetComponent<Collider>();

        _isUnlocked = startsUnlocked;
    }

    private void OnMouseDown()
    {
        if (!_isUnlocked)
        {
            if (verboseLogs)
                Debug.Log($"[{name}] Trampilla bloqueada.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogWarning($"[{name}] gameSceneName está vacío.", this);
            return;
        }

        if (verboseLogs)
            Debug.Log($"[{name}] Cargando escena con transición: {gameSceneName}", this);

        // 🔹 Usa tu sistema de transición
        SceneTransitionPanel.LoadSceneWithTransition(gameSceneName);
    }

    public void UnlockTrapdoor()
    {
        _isUnlocked = true;

        if (verboseLogs)
            Debug.Log($"[{name}] Trampilla desbloqueada.", this);
    }
}