using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class EyeBossController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RoundFlowController flow;
    [SerializeField] private HealthManager healthManager; // NUEVO

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

    [Header("After Second Eye Destroyed")]
    [SerializeField] private string nextSceneAfterSecondEye = "Pasillo";

    [Header("No-Intermission First Eye Sequence (ej. Ronda4)")]
    [SerializeField] private bool playInlineSequenceWhenNoIntermission = true;

    [Tooltip("Si lo asignas, se usa este GO de arma para ocultar.")]
    [SerializeField] private GameObject weaponVisualToHide;

    [Tooltip("Opcional: componente/script del arma para desactivar control.")]
    [SerializeField] private Behaviour weaponControllerToDisable;

    [Header("Auto Find Weapon (si weaponVisualToHide está vacío)")]
    [SerializeField] private bool autoFindWeaponVisualIfMissing = true;
    [SerializeField] private string weaponTag = "Weapon";
    [SerializeField] private string weaponNameContains = "Arma";

    [Header("Fake Fade")]
    [SerializeField] private CanvasGroup fakeFadeGroup;
    [SerializeField] private float fakeFadeInTime = 0.18f;
    [SerializeField] private float fakeFadeHoldTime = 0.12f;
    [SerializeField] private float fakeFadeOutTime = 0.20f;

    [Header("Hand Reveal")]
    [SerializeField] private GameObject handToActivate;
    [SerializeField] private Transform handTargetTransform;
    [SerializeField] private float handMoveUpY = 0.35f;
    [SerializeField] private float handMoveTime = 0.45f;
    [SerializeField] private Ease handMoveEase = Ease.OutSine;

    [Header("Boss Phases")]
    [Tooltip("Si está activo y estás en Ronda4, al destruir el primer ojo EnemyHP vuelve a 40.")]
    [SerializeField] private bool resetEnemyHpAfterFirstEyeInRonda4 = true; // NUEVO

    private bool shotConsumed = false;
    private bool victoryTriggered = false;
    private bool firstEyeInlineSequencePlayed = false;

    private void Start()
    {
        ApplyPersistedEyes();

        if (fakeFadeGroup != null)
        {
            fakeFadeGroup.alpha = 0f;
            fakeFadeGroup.blocksRaycasts = false;
            fakeFadeGroup.interactable = false;
        }
    }

    private void ApplyPersistedEyes()
    {
        if (GameProgress.Instance == null) return;

        GameProgress.EyeRoundState state = GameProgress.Instance.GetEyeState(roundId);

        if (leftEyeGO != null) leftEyeGO.SetActive(!state.LeftDestroyed);
        if (rightEyeGO != null) rightEyeGO.SetActive(!state.RightDestroyed);
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

        // NUEVO: primer ojo caído => reset HP enemigo a 40 SOLO en Ronda4
        if (resetEnemyHpAfterFirstEyeInRonda4 && roundId == 4 && healthManager != null)
            healthManager.ResetEnemyHPToMax();

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

        if (!goToIntermissionAfterFirstEye && playInlineSequenceWhenNoIntermission && !firstEyeInlineSequencePlayed)
        {
            firstEyeInlineSequencePlayed = true;
            yield return StartCoroutine(PlayInlineNoTransitionSequence());
        }

        shotConsumed = false;
    }

    private IEnumerator PlayInlineNoTransitionSequence()
    {
        ResolveWeaponReferences();

        if (weaponControllerToDisable != null)
            weaponControllerToDisable.enabled = false;

        if (weaponVisualToHide != null)
            weaponVisualToHide.SetActive(false);

        if (fakeFadeGroup != null)
        {
            fakeFadeGroup.blocksRaycasts = true;
            fakeFadeGroup.interactable = true;

            yield return fakeFadeGroup.DOFade(1f, fakeFadeInTime).SetEase(Ease.Linear).WaitForCompletion();

            if (fakeFadeHoldTime > 0f)
                yield return new WaitForSeconds(fakeFadeHoldTime);

            yield return fakeFadeGroup.DOFade(0f, fakeFadeOutTime).SetEase(Ease.Linear).WaitForCompletion();

            fakeFadeGroup.blocksRaycasts = false;
            fakeFadeGroup.interactable = false;
        }

        if (handToActivate != null)
        {
            handToActivate.SetActive(true);

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
    }

    private void ResolveWeaponReferences()
    {
        if (!autoFindWeaponVisualIfMissing) return;

        if (weaponVisualToHide == null)
        {
            if (!string.IsNullOrEmpty(weaponTag))
            {
                try
                {
                    var byTag = GameObject.FindWithTag(weaponTag);
                    if (byTag != null) weaponVisualToHide = byTag;
                }
                catch
                {
                    // Tag no existe
                }
            }

            if (weaponVisualToHide == null && !string.IsNullOrEmpty(weaponNameContains))
            {
                var all = FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] == null) continue;
                    if (all[i].name.ToLower().Contains(weaponNameContains.ToLower()))
                    {
                        weaponVisualToHide = all[i].gameObject;
                        break;
                    }
                }
            }
        }

        if (weaponControllerToDisable == null && weaponVisualToHide != null)
            weaponControllerToDisable = weaponVisualToHide.GetComponent<Behaviour>();
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