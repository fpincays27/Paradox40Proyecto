using UnityEngine;
using DG.Tweening;

public class EyesAnimator : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;

    [Header("Blink")]
    [SerializeField] private float blinkMinDelay = 1.2f;
    [SerializeField] private float blinkMaxDelay = 3.5f;
    [SerializeField] private float blinkCloseTime = 0.06f;
    [SerializeField] private float blinkOpenTime = 0.08f;
    [SerializeField] private float blinkScaleY = 0.1f;

    [Header("Look")]
    [SerializeField] private float lookRadius = 0.08f;
    [SerializeField] private float lookMoveTime = 0.35f;
    [SerializeField] private float lookWaitMin = 0.2f;
    [SerializeField] private float lookWaitMax = 0.8f;

    [Header("Stress")]
    [SerializeField] private bool stressShake = false;
    [SerializeField] private float shakeStrength = 0.04f;
    [SerializeField] private float shakeDuration = 0.25f;

    private Vector3 leftStart, rightStart;
    private Tween lookTween, shakeTween;

    // ✅ Guardar valores “normales” (tus defaults del inspector)
    private float d_blinkMinDelay, d_blinkMaxDelay, d_blinkCloseTime, d_blinkOpenTime, d_blinkScaleY;
    private float d_lookRadius, d_lookMoveTime, d_lookWaitMin, d_lookWaitMax;
    private bool d_stressShake;
    private float d_shakeStrength, d_shakeDuration;

    private void Awake()
    {
        if (leftEye == null || rightEye == null)
        {
            Debug.LogWarning("Asigna LeftEye y RightEye en el Inspector.");
            enabled = false;
            return;
        }

        leftStart = leftEye.localPosition;
        rightStart = rightEye.localPosition;

        CacheDefaults();
    }

    private void OnEnable()
    {
        StartBlinkLoop();
        StartLookLoop();

        if (stressShake)
            StartShakeLoop();
    }

    private void OnDisable()
    {
        lookTween?.Kill();
        shakeTween?.Kill();
        DOTween.Kill(leftEye);
        DOTween.Kill(rightEye);
        DOTween.Kill(this);
    }

    private void CacheDefaults()
    {
        d_blinkMinDelay = blinkMinDelay;
        d_blinkMaxDelay = blinkMaxDelay;
        d_blinkCloseTime = blinkCloseTime;
        d_blinkOpenTime = blinkOpenTime;
        d_blinkScaleY = blinkScaleY;

        d_lookRadius = lookRadius;
        d_lookMoveTime = lookMoveTime;
        d_lookWaitMin = lookWaitMin;
        d_lookWaitMax = lookWaitMax;

        d_stressShake = stressShake;
        d_shakeStrength = shakeStrength;
        d_shakeDuration = shakeDuration;
    }

    // ✅ NORMAL: vuelve exactamente a tus valores originales del Inspector
    public void ApplyNormalPreset()
    {
        blinkMinDelay = d_blinkMinDelay;
        blinkMaxDelay = d_blinkMaxDelay;
        blinkCloseTime = d_blinkCloseTime;
        blinkOpenTime = d_blinkOpenTime;
        blinkScaleY = d_blinkScaleY;

        lookRadius = d_lookRadius;
        lookMoveTime = d_lookMoveTime;
        lookWaitMin = d_lookWaitMin;
        lookWaitMax = d_lookWaitMax;

        stressShake = d_stressShake;
        shakeStrength = d_shakeStrength;
        shakeDuration = d_shakeDuration;

        RestartLoops();
    }

    // ✅ DISTORSIÓN: los valores raros que me mostraste (parpadeo “infinito”)
    public void ApplyLonelyEyeDistortionPreset()
    {
        blinkMinDelay = 1e26f;
        blinkMaxDelay = 1000000f;
        blinkCloseTime = 1000000f;
        blinkOpenTime = 1000000f;
        // Mantén tu escala Y de blink (o déjalo en 0.5 si quieres)
        // blinkScaleY = 0.5f;
        // Yo lo dejo usando el tuyo, para no romper estilo:
        blinkScaleY = d_blinkScaleY;

        // Puedes dejar look igual o más lento, lo dejo igual a tus defaults:
        lookRadius = d_lookRadius;
        lookMoveTime = d_lookMoveTime;
        lookWaitMin = d_lookWaitMin;
        lookWaitMax = d_lookWaitMax;

        // Stress igual que tenías por defecto
        stressShake = d_stressShake;
        shakeStrength = d_shakeStrength;
        shakeDuration = d_shakeDuration;

        RestartLoops();
    }

    private void RestartLoops()
    {
        // matar loops/tweens y arrancar de nuevo con parámetros nuevos
        lookTween?.Kill();
        shakeTween?.Kill();
        DOTween.Kill(leftEye);
        DOTween.Kill(rightEye);
        DOTween.Kill(this);

        StartBlinkLoop();
        StartLookLoop();

        if (stressShake)
            StartShakeLoop();
    }

    void StartBlinkLoop()
    {
        float delay = Random.Range(blinkMinDelay, blinkMaxDelay);

        DOVirtual.DelayedCall(delay, () =>
        {
            BlinkOnce();
            StartBlinkLoop();
        }).SetTarget(this);
    }

    void BlinkOnce()
    {
        // ✅ Si un ojo está desactivado, no lo animes
        bool leftActive = leftEye != null && leftEye.gameObject.activeInHierarchy;
        bool rightActive = rightEye != null && rightEye.gameObject.activeInHierarchy;

        // Cierra
        if (leftActive) leftEye.DOScaleY(blinkScaleY, blinkCloseTime).SetEase(Ease.InQuad);
        if (rightActive) rightEye.DOScaleY(blinkScaleY, blinkCloseTime).SetEase(Ease.InQuad);

        // Abre
        DOVirtual.DelayedCall(blinkCloseTime, () =>
        {
            if (leftActive) leftEye.DOScaleY(1f, blinkOpenTime).SetEase(Ease.OutQuad);
            if (rightActive) rightEye.DOScaleY(1f, blinkOpenTime).SetEase(Ease.OutQuad);
        }).SetTarget(this);
    }

    void StartLookLoop()
    {
        float wait = Random.Range(lookWaitMin, lookWaitMax);

        DOVirtual.DelayedCall(wait, () =>
        {
            Vector2 rnd = Random.insideUnitCircle * lookRadius;

            Vector3 lTarget = leftStart + new Vector3(rnd.x, rnd.y, 0f);
            Vector3 rTarget = rightStart + new Vector3(rnd.x, rnd.y, 0f);

            lookTween?.Kill();

            bool leftActive = leftEye != null && leftEye.gameObject.activeInHierarchy;
            bool rightActive = rightEye != null && rightEye.gameObject.activeInHierarchy;

            Sequence seq = DOTween.Sequence();
            if (leftActive) seq.Append(leftEye.DOLocalMove(lTarget, lookMoveTime).SetEase(Ease.OutSine));
            if (rightActive)
            {
                if (leftActive) seq.Join(rightEye.DOLocalMove(rTarget, lookMoveTime).SetEase(Ease.OutSine));
                else seq.Append(rightEye.DOLocalMove(rTarget, lookMoveTime).SetEase(Ease.OutSine));
            }

            lookTween = seq;

            StartLookLoop();
        }).SetTarget(this);
    }

    void StartShakeLoop()
    {
        shakeTween?.Kill();
        shakeTween = transform
            .DOShakePosition(shakeDuration, shakeStrength, 20, 90, false, true)
            .SetLoops(-1, LoopType.Restart);
    }

    // Llamalo desde otros scripts si quieres activar “estres” en runtime
    public void SetStress(bool on)
    {
        stressShake = on;
        if (on) StartShakeLoop();
        else shakeTween?.Kill();
    }
}