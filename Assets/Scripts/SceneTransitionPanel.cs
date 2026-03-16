using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionPanel : MonoBehaviour
{
    public static SceneTransitionPanel Instance { get; private set; }

    [Header("UI (opcional si lo asignas manualmente)")]
    [SerializeField] private CanvasGroup blackoutGroup;

    [Header("Timing")]
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float fadeInDuration = 0.35f;

    [Header("Startup")]
    [SerializeField] private bool fadeInOnFirstScene = true;

    private bool isTransitioning;
    private bool didInitialFade;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (blackoutGroup == null)
            CreateRuntimeOverlay();

        ForceOnTop();
        SetHiddenInstant();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (!didInitialFade && fadeInOnFirstScene && blackoutGroup != null)
        {
            didInitialFade = true;
            blackoutGroup.alpha = 1f;
            StartCoroutine(FadeInRoutine());
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ForceOnTop();

        // Al entrar a escena, liberar pantalla + clicks
        if (isTransitioning)
            StartCoroutine(FadeInRoutine());
    }

    public static void LoadSceneWithTransition(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        if (Instance == null)
        {
            GameObject go = new GameObject("SceneTransitionPanel(Runtime)");
            Instance = go.AddComponent<SceneTransitionPanel>();
        }

        Instance.BeginTransition(sceneName);
    }

    private void BeginTransition(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        ForceOnTop();

        if (blackoutGroup != null)
        {
            blackoutGroup.DOKill();
            blackoutGroup.blocksRaycasts = true;
            blackoutGroup.interactable = true;

            Tween t = blackoutGroup.DOFade(1f, fadeOutDuration).SetEase(Ease.Linear);
            yield return t.WaitForCompletion();
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;
    }

    private IEnumerator FadeInRoutine()
    {
        if (blackoutGroup == null)
        {
            isTransitioning = false;
            yield break;
        }

        ForceOnTop();

        blackoutGroup.DOKill();
        blackoutGroup.blocksRaycasts = true;
        blackoutGroup.interactable = true;

        Tween t = blackoutGroup.DOFade(0f, fadeInDuration).SetEase(Ease.Linear);
        yield return t.WaitForCompletion();

        // CLAVE: devolver interacción a UI
        blackoutGroup.blocksRaycasts = false;
        blackoutGroup.interactable = false;
        isTransitioning = false;
    }

    private void CreateRuntimeOverlay()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("BlackoutPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(transform, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.GetComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = true;

        blackoutGroup = panel.GetComponent<CanvasGroup>();
    }

    private void ForceOnTop()
    {
        if (blackoutGroup == null) return;

        blackoutGroup.transform.SetAsLastSibling();

        Canvas c = blackoutGroup.GetComponentInParent<Canvas>();
        if (c != null)
        {
            c.overrideSorting = true;
            c.sortingOrder = short.MaxValue;
        }
    }

    private void SetHiddenInstant()
    {
        if (blackoutGroup == null) return;

        blackoutGroup.alpha = 0f;
        blackoutGroup.blocksRaycasts = false;
        blackoutGroup.interactable = false;
    }
}