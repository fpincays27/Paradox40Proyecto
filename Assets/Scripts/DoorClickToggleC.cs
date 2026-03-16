using UnityEngine;
using DG.Tweening;

/// <summary>
/// Puerta_C:
/// - Click izquierdo sobre la puerta para abrir/cerrar.
/// </summary>
public class DoorClickToggleC : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform hinge; // si null, usa este transform

    [Header("Angles (local Y)")]
    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float openAngle = -90f;

    [Header("Animation")]
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private bool isOpen = false;
    private Tween rotateTween;

    private void Awake()
    {
        if (hinge == null)
            hinge = transform;

        SetAngleInstant(closedAngle);
        isOpen = false;
    }

    private void OnMouseDown()
    {
        ToggleDoor();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        float target = isOpen ? openAngle : closedAngle;

        rotateTween?.Kill();
        rotateTween = hinge.DOLocalRotate(
            new Vector3(0f, target, 0f),
            duration
        ).SetEase(ease);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
        float target = isOpen ? openAngle : closedAngle;

        rotateTween?.Kill();
        rotateTween = hinge.DOLocalRotate(
            new Vector3(0f, target, 0f),
            duration
        ).SetEase(ease);
    }

    private void SetAngleInstant(float y)
    {
        Vector3 e = hinge.localEulerAngles;
        hinge.localEulerAngles = new Vector3(e.x, y, e.z);
    }
}