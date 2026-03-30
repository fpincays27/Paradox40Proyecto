using System.Collections;
using UnityEngine;
using TMPro;

public class SimpleObjectiveUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("Objetivos (1 o 2)")]
    [TextArea(2, 4)]
    [SerializeField] private string[] objectives = new string[]
    {
        "Encuentra la llave roja.",
        "Abre la puerta principal."
    };

    [Header("Animación")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float holdAfterChange = 1.0f;

    private int currentIndex = 0;
    private Coroutine transitionRoutine;

    private void Awake()
    {
        if (panelGroup == null)
            panelGroup = GetComponentInChildren<CanvasGroup>();

        if (objectiveText == null)
            objectiveText = GetComponentInChildren<TextMeshProUGUI>();

        if (panelGroup != null)
            panelGroup.alpha = 1f;

        ShowCurrentObjectiveInstant();
    }

    private void Start()
    {
        // Opcional: mostrar panel al arrancar
        ShowPanel(true);
    }

    public void ShowPanel(bool show)
    {
        if (panelGroup == null) return;
        panelGroup.gameObject.SetActive(show);
    }

    public void SetObjectiveIndex(int index)
    {
        if (objectives == null || objectives.Length == 0) return;
        if (index < 0 || index >= objectives.Length) return;
        if (index == currentIndex) return;

        currentIndex = index;
        StartTextTransition();
    }

    public void NextObjective()
    {
        if (objectives == null || objectives.Length == 0) return;

        int next = currentIndex + 1;
        if (next >= objectives.Length)
            next = objectives.Length - 1; // se queda en el último

        if (next != currentIndex)
        {
            currentIndex = next;
            StartTextTransition();
        }
    }

    public void SetObjectiveTextDirect(string text)
    {
        if (objectiveText == null) return;
        objectiveText.text = text;
    }

    public string GetCurrentObjective()
    {
        if (objectives == null || objectives.Length == 0) return string.Empty;
        return objectives[currentIndex];
    }

    private void ShowCurrentObjectiveInstant()
    {
        if (objectiveText == null || objectives == null || objectives.Length == 0) return;
        objectiveText.text = objectives[currentIndex];
    }

    private void StartTextTransition()
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        if (panelGroup == null || objectiveText == null || objectives == null || objectives.Length == 0)
        {
            ShowCurrentObjectiveInstant();
            yield break;
        }

        // Fade out
        yield return Fade(1f, 0f, fadeDuration);

        // Cambia texto
        objectiveText.text = objectives[currentIndex];

        // Pequeña pausa opcional
        if (holdAfterChange > 0f)
            yield return new WaitForSeconds(holdAfterChange * 0.2f);

        // Fade in
        yield return Fade(0f, 1f, fadeDuration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            panelGroup.alpha = to;
            yield break;
        }

        float t = 0f;
        panelGroup.alpha = from;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            panelGroup.alpha = Mathf.Lerp(from, to, p);
            yield return null;
        }

        panelGroup.alpha = to;
    }
}