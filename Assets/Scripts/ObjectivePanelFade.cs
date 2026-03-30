using System.Collections;
using UnityEngine;

public class ObjectivePanelFade : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float visibleTime = 10f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeRoutine;

    private void Start()
    {
        ShowPanel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ShowPanel();
        }
    }

    public void ShowPanel()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(ShowThenFade());
    }

    IEnumerator ShowThenFade()
    {
        yield return StartCoroutine(Fade(1f));

        yield return new WaitForSeconds(visibleTime);

        yield return StartCoroutine(Fade(0f));
    }

    IEnumerator Fade(float target)
    {
        float start = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }
}