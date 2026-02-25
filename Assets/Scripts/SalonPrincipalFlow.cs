using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SalonPrincipalFlow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;          // Panel que se muestra a los 5s
    [SerializeField] private TextMeshProUGUI timerText; // Texto opcional para mostrar el conteo (puede ser null)

    [Header("Timing")]
    [SerializeField] private float showPanelAfter = 5f;
    [SerializeField] private float countdownSeconds = 15f;

    [Header("Scenes")]
    [SerializeField] private string nextScene = "Ronda2";

    private Coroutine countdownRoutine;
    private bool panelClosed = false;

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        StartCoroutine(ShowPanelDelayed());
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

    // ✅ Esto lo llamas desde el botón "Cerrar"
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

        SceneManager.LoadScene(nextScene);
    }
}