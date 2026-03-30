using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class EyeBossController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RoundFlowController flow;

    [Header("Eyes")]
    [SerializeField] private GameObject leftEyeGO;
    [SerializeField] private GameObject rightEyeGO;

    [Header("Injured Sprites (optional)")]
    [SerializeField] private Sprite injuredLeftSprite;
    [SerializeField] private Sprite injuredRightSprite;

    [Header("Timing")]
    [SerializeField] private float injuredTime = 0.25f;

    [Header("Disappear Animation")]
    [SerializeField] private float disappearScaleTime = 0.2f;
    [SerializeField] private float disappearFadeTime = 0.22f;

    [Header("Delay before distortion")]
    [SerializeField] private float delayBeforeDistortion = 1f;

    [Header("Stretch (cuando quede 1 ojo)")]
    [SerializeField] private float lonelyEyeScaleX = 1.0f;
    [SerializeField] private float lonelyEyeScaleY = 1.7f;
    [SerializeField] private float stretchTime = 0.15f;

    [Header("Round")]
    [SerializeField] private int roundId = 2;
    [SerializeField] private bool goToIntermissionAfterFirstEye = true;

    [Header("Carry Progress From Previous Round (opcional)")]
    [Tooltip("Si la ronda actual no tiene estado guardado, hereda desde esta ronda (ej: Ronda2 hereda de Ronda1 => 1).")]
    [SerializeField] private int inheritStateFromRoundId = -1;
    [SerializeField] private bool copyInheritedStateToCurrentRound = true;

    [Header("After Second Eye Destroyed")]
    [SerializeField] private string nextSceneAfterSecondEye = "Pasillo";

    [Header("Ronda4 Intro Hand (desde transición)")]
    [SerializeField] private bool playHandIntroOnStart = true;
    [SerializeField] private string requiredPreviousSceneName = "Ronda4Transition";
    [SerializeField] private bool playIfPreviousSceneUnknown = false; // fallback útil para debug
    [SerializeField] private GameObject handToActivate;
    [SerializeField] private Transform handTargetTransform;
    [SerializeField] private float handStartDelay = 0.05f;
    [SerializeField] private float handHoldBeforeMove = 0.15f;
    [SerializeField] private float handMoveUpY = 0.35f;
    [SerializeField] private float handMoveTime = 0.45f;
    [SerializeField] private Ease handMoveEase = Ease.OutSine;
    [SerializeField] private bool verboseLogs = true;

    private bool shotConsumed = false;
    private bool victoryTriggered = false;

    // Tracker estático de escena previa
    private static bool sceneTrackerInstalled = false;
    private static string previousSceneName = string.Empty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InstallSceneTrackerEarly()
    {
        EnsureSceneTracker();
    }

    private static void EnsureSceneTracker()
    {
        if (sceneTrackerInstalled) return;
        sceneTrackerInstalled = true;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        previousSceneName = oldScene.name;
    }

    private void Awake()
    {
        EnsureSceneTracker();
    }

    private void Start()
    {
        ApplyPersistedEyes();
        TryPlayStartHandIntro();
    }

    private void TryPlayStartHandIntro()
    {
        if (!playHandIntroOnStart) return;
        if (roundId != 4) return;
        if (handToActivate == null) return;

        bool previousMatches = !string.IsNullOrEmpty(requiredPreviousSceneName) &&
                               previousSceneName == requiredPreviousSceneName;

        bool previousUnknown = string.IsNullOrEmpty(previousSceneName);
        bool shouldPlay = previousMatches || (previousUnknown && playIfPreviousSceneUnknown);

        if (verboseLogs)
            Debug.Log($"[EyeBossController] Start intro check. prev='{previousSceneName}', required='{requiredPreviousSceneName}', play={shouldPlay}");

        if (!shouldPlay) return;

        StartCoroutine(PlayHandIntroSequence());
    }

    private IEnumerator PlayHandIntroSequence()
    {
        if (handStartDelay > 0f)
            yield return new WaitForSeconds(handStartDelay);

        handToActivate.SetActive(true);

        if (handHoldBeforeMove > 0f)
            yield return new WaitForSeconds(handHoldBeforeMove);

        Transform ht = handToActivate.transform;
        ht.DOKill();

        Vector3 start = ht.position;
        Vector3 target = handTargetTransform != null
            ? handTargetTransform.position
            : start + Vector3.up * handMoveUpY;

        yield return ht.DOMove(target, handMoveTime)
            .SetEase(handMoveEase)
            .WaitForCompletion();
    }

    private void ApplyPersistedEyes()
    {
        if (GameProgress.Instance == null) return;

        GameProgress.EyeRoundState state = GameProgress.Instance.GetEyeState(roundId);
        bool hasOwnProgress = HasAnyProgress(state);

        bool canInherit = inheritStateFromRoundId >= 0 && inheritStateFromRoundId != roundId;
        if (!hasOwnProgress && canInherit)
        {
            GameProgress.EyeRoundState inherited = GameProgress.Instance.GetEyeState(inheritStateFromRoundId);

            if (HasAnyProgress(inherited))
            {
                state = inherited;

                if (copyInheritedStateToCurrentRound)
                {
                    if (inherited.LeftDestroyed) GameProgress.Instance.SetEyeDestroyed(roundId, true);
                    if (inherited.RightDestroyed) GameProgress.Instance.SetEyeDestroyed(roundId, false);
                }
            }
        }

        if (leftEyeGO != null) leftEyeGO.SetActive(!state.LeftDestroyed);
        if (rightEyeGO != null) rightEyeGO.SetActive(!state.RightDestroyed);
    }

    private bool HasAnyProgress(GameProgress.EyeRoundState state)
    {
        return state.LeftDestroyed || state.RightDestroyed;
    }

    public void OnEyeShot(EyeTarget hit)
    {
        if (victoryTriggered) return;
        if (shotConsumed) return;

        shotConsumed = true;
        StartCoroutine(EyeShotSequence(hit));
    }

    private IEnumerator EyeShotSequence(EyeTarget hit)
    {
        if (hit == null)
        {
            shotConsumed = false;
            yield break;
        }

        var sr = hit.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (hit.IsLeftEye && injuredLeftSprite != null) sr.sprite = injuredLeftSprite;
            if (!hit.IsLeftEye && injuredRightSprite != null) sr.sprite = injuredRightSprite;
        }

        yield return new WaitForSeconds(injuredTime);

        SFXManager.I?.PlayEyeImpact();

        yield return StartCoroutine(DisappearEye(hit.gameObject));

        if (GameProgress.Instance != null)
            GameProgress.Instance.SetEyeDestroyed(roundId, hit.IsLeftEye);

        if (AreBothEyesGone())
        {
            victoryTriggered = true;
            GameProgress.Instance?.ClearEyeState(roundId);

            if (GameProgress.Instance != null)
                GameProgress.Instance.PendingSalonReturnScene = string.Empty;

            SceneTransitionPanel.LoadSceneWithTransition(nextSceneAfterSecondEye);
            yield break;
        }

        yield return new WaitForSeconds(delayBeforeDistortion);

        GameObject remaining = GetRemainingEye();
        if (remaining != null)
            StretchRemainingEye(remaining.transform);

        if (goToIntermissionAfterFirstEye && flow != null)
        {
            if (GameProgress.Instance != null)
                GameProgress.Instance.PendingSalonReturnScene = SceneManager.GetActiveScene().name;

            flow.StartBlackoutAndLoadNextScene();
            shotConsumed = false;
            yield break;
        }

        shotConsumed = false;
    }

    private bool AreBothEyesGone()
    {
        bool leftGone = (leftEyeGO == null) || !leftEyeGO.activeSelf;
        bool rightGone = (rightEyeGO == null) || !rightEyeGO.activeSelf;
        return leftGone && rightGone;
    }

    private IEnumerator DisappearEye(GameObject eyeGO)
    {
        if (eyeGO == null) yield break;

        Transform t = eyeGO.transform;
        SpriteRenderer sr = eyeGO.GetComponent<SpriteRenderer>();

        t.DOKill();
        if (sr != null) sr.DOKill();

        float startAlpha = (sr != null) ? sr.color.a : 1f;
        Vector3 startScale = t.localScale;

        t.DOScale(startScale * 0.6f, disappearScaleTime).SetEase(Ease.InBack);

        if (sr != null)
            sr.DOFade(0f, disappearFadeTime).SetEase(Ease.Linear);

        float wait = Mathf.Max(disappearScaleTime, disappearFadeTime);
        yield return new WaitForSeconds(wait);

        eyeGO.SetActive(false);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = startAlpha;
            sr.color = c;
        }

        t.localScale = startScale;
    }

    private GameObject GetRemainingEye()
    {
        bool leftActive = leftEyeGO != null && leftEyeGO.activeSelf;
        bool rightActive = rightEyeGO != null && rightEyeGO.activeSelf;

        if (leftActive && !rightActive) return leftEyeGO;
        if (!leftActive && rightActive) return rightEyeGO;
        return null;
    }

    private void StretchRemainingEye(Transform eye)
    {
        if (eye == null) return;

        Vector3 target = new Vector3(lonelyEyeScaleX, lonelyEyeScaleY, 1f);

        eye.DOKill();
        eye.DOScale(target, stretchTime).SetEase(Ease.OutBack);
    }
}