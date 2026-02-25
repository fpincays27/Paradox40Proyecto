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

    [Header("Round 2 behavior")]
    [SerializeField] private bool isRound2 = false;

    [Header("Victory")]
    [SerializeField] private string winSceneName = "Win";

    private bool shotConsumed = false;
    private bool victoryTriggered = false;

    private void Start()
    {
        ApplyPersistedEyes();
    }

    private void ApplyPersistedEyes()
    {
        if (GameProgress.Instance == null) return;

        if (leftEyeGO != null) leftEyeGO.SetActive(!GameProgress.Instance.LeftEyeDestroyed);
        if (rightEyeGO != null) rightEyeGO.SetActive(!GameProgress.Instance.RightEyeDestroyed);
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

        // 1) Sprite herido (opcional)
        var sr = hit.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (hit.IsLeftEye && injuredLeftSprite != null) sr.sprite = injuredLeftSprite;
            if (!hit.IsLeftEye && injuredRightSprite != null) sr.sprite = injuredRightSprite;
        }

        yield return new WaitForSeconds(injuredTime);

        // ✅ SFX impacto ojo
        SFXManager.I?.PlayEyeImpact();

        // 2) Desaparecer (fade + scale)
        yield return StartCoroutine(DisappearEye(hit.gameObject));

        // Persistencia (por si rondas futuras)
        if (GameProgress.Instance != null)
        {
            if (hit.IsLeftEye) GameProgress.Instance.LeftEyeDestroyed = true;
            else GameProgress.Instance.RightEyeDestroyed = true;
        }

        // 3) ¿Se fueron LOS DOS ojos? => VICTORIA (Win)
        if (AreBothEyesGone())
        {
            victoryTriggered = true;
            SceneManager.LoadScene(winSceneName);
            yield break;
        }

        // 4) Queda solo 1 ojo => esperar y distorsionar (solo Ronda1)
        yield return new WaitForSeconds(delayBeforeDistortion);

        if (!isRound2)
        {
            GameObject remaining = GetRemainingEye();
            if (remaining != null)
                StretchRemainingEye(remaining.transform);
        }

        // 5) ✅ Ahora sí: panel + apagón + siguiente escena
        if (flow != null)
            flow.StartBlackoutAndLoadNextScene();

        // Si por algún motivo NO hay flow, al menos permitir otro disparo
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

        // Restaurar (por si reusan el GO en otra escena)
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