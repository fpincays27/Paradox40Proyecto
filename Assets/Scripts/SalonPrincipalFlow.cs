using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class SalonPrincipalFlow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;          // Panel que se muestra tras delay
    [SerializeField] private TextMeshProUGUI timerText; // Texto opcional para mostrar el conteo

    [Header("Timing")]
    [SerializeField] private float showPanelAfter = 5f;
    [SerializeField] private float countdownSeconds = 15f;

    [Header("Scenes")]
    [SerializeField] private string nextScene = "Ronda2";

    [Header("Special Case")]
    [Tooltip("Si llegas desde esta escena, SalonPrincipal ignorará el retorno y usará nextScene.")]
    [SerializeField] private string forceFallbackWhenComingFrom = "Ronda1";

    [Header("Input (opcional)")]
    [SerializeField] private bool allowSpaceToContinue = true;

    private Coroutine countdownRoutine;
    private bool panelClosed = false;

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        StartCoroutine(ShowPanelDelayed());
    }

    private void Update()
    {
        if (!allowSpaceToContinue) return;
        if (panel == null) return;
        if (!panel.activeSelf) return;
        if (panelClosed) return;

        // Cerrar panel + iniciar contador con SPACE
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ClosePanelAndStartTimer();
        }
    }

    private IEnumerator ShowPanelDelayed()
    {
        yield return new WaitForSeconds(showPanelAfter);

        if (panel != null)
        {
            panel.SetActive(true);
            panelClosed = false;
        }
    }

    // Puedes llamarlo desde botón o desde SPACE
    public void ClosePanelAndStartTimer()
    {
        if (panelClosed) return;
        panelClosed = true;

        if (panel != null)
            panel.SetActive(false);

        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        countdownRoutine = StartCoroutine(CountdownThenLoad());
    }

    private IEnumerator CountdownThenLoad()
    {
        float t = countdownSeconds;

        while (t > 0f)
        {
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(t).ToString();

            yield return new WaitForSeconds(1f);
            t -= 1f;
        }

        if (timerText != null)
            timerText.text = "0";

        string sceneToLoad = nextScene;

        if (GameProgress.Instance != null)
        {
            string pending = GameProgress.Instance.PendingSalonReturnScene;

            if (!string.IsNullOrEmpty(pending))
            {
                // Regla especial:
                // Si venimos de Ronda1, NO retornar a Ronda1; usar nextScene (Ronda2).
                if (pending == forceFallbackWhenComingFrom)
                {
                    sceneToLoad = nextScene;
                }
                else
                {
                    // En todos los demás casos, sí volvemos a la escena de origen.
                    sceneToLoad = pending;
                }

                // Consumir siempre el pending para no arrastrarlo al siguiente ciclo.
                GameProgress.Instance.PendingSalonReturnScene = string.Empty;
            }
        }

        SceneTransitionPanel.LoadSceneWithTransition(sceneToLoad);
    }
}