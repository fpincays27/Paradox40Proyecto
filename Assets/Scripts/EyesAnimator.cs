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
    }

    void StartBlinkLoop()
    {
        float delay = Random.Range(blinkMinDelay, blinkMaxDelay);
        DOVirtual.DelayedCall(delay, () =>
        {
            BlinkOnce();
            StartBlinkLoop();
        });
    }

    void BlinkOnce()
    {
        // Cierra
        leftEye.DOScaleY(blinkScaleY, blinkCloseTime).SetEase(Ease.InQuad);
        rightEye.DOScaleY(blinkScaleY, blinkCloseTime).SetEase(Ease.InQuad);

        // Abre
        DOVirtual.DelayedCall(blinkCloseTime, () =>
        {
            leftEye.DOScaleY(1f, blinkOpenTime).SetEase(Ease.OutQuad);
            rightEye.DOScaleY(1f, blinkOpenTime).SetEase(Ease.OutQuad);
        });
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
            Sequence seq = DOTween.Sequence();
            seq.Append(leftEye.DOLocalMove(lTarget, lookMoveTime).SetEase(Ease.OutSine));
            seq.Join(rightEye.DOLocalMove(rTarget, lookMoveTime).SetEase(Ease.OutSine));
            lookTween = seq;

            StartLookLoop();
        });
    }

    void StartShakeLoop()
    {
        shakeTween?.Kill();
        shakeTween = transform.DOShakePosition(shakeDuration, shakeStrength, 20, 90, false, true)
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