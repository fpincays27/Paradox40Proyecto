using UnityEngine;

/// <summary>
/// Puerta_A:
/// Mantén E y mueve el mouse X para abrir/cerrar.
/// - Modo original: pivote local desde la puerta (si frameReference == null)
/// - Modo marco: pivote basado en el marco (si frameReference asignado)
/// </summary>
public class DoorHoldRotateA : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode holdKey = KeyCode.E;
    [SerializeField] private string mouseAxis = "Mouse X";
    [SerializeField] private float mouseSensitivity = 140f;
    [SerializeField] private bool invertMouse = false;

    [Header("Arc / Hinge (local space door - modo original)")]
    [Tooltip("Offset LOCAL desde el centro de la puerta hasta la bisagra (modo original).")]
    [SerializeField] private Vector3 hingeLocalOffset = new Vector3(-0.45f, 0f, 0f);

    [Tooltip("Ángulo cerrado (grados) relativo al estado inicial.")]
    [SerializeField] private float closedAngle = 0f;

    [Tooltip("Ángulo mínimo relativo (ej: -100).")]
    [SerializeField] private float minAngle = -100f;

    [Tooltip("Ángulo máximo relativo (ej: 0 o 100).")]
    [SerializeField] private float maxAngle = 0f;

    [Header("Frame pivot (opcional)")]
    [Tooltip("Si lo asignas, la puerta pivotará respecto al marco.")]
    [SerializeField] private Transform frameReference;

    [Tooltip("Posición LOCAL de la bisagra en el espacio del marco.")]
    [SerializeField] private Vector3 hingeLocalInFrame = new Vector3(-0.45f, 0f, 0f);

    [Tooltip("Posición LOCAL cerrada de la puerta respecto al marco.")]
    [SerializeField] private Vector3 closedPosLocalInFrame = Vector3.zero;

    [Tooltip("Rotación LOCAL cerrada de la puerta respecto al marco.")]
    [SerializeField] private Vector3 closedEulerLocalInFrame = Vector3.zero;

    [Tooltip("Bloquea altura para evitar hundimiento visual.")]
    [SerializeField] private bool lockWorldHeight = true;

    [Header("Optional constraints")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxUseDistance = 3f;
    [SerializeField] private bool requireLookingAtDoor = true;

    private Vector3 basePos;
    private Quaternion baseRot;
    private float currentAngle;

    private void Awake()
    {
        basePos = transform.position;
        baseRot = transform.rotation;
        currentAngle = closedAngle;

        ApplyAngle(currentAngle);
    }

    private void Update()
    {
        if (!Input.GetKey(holdKey))
            return;

        if (!CanUse())
            return;

        float mx = Input.GetAxis(mouseAxis);
        if (Mathf.Abs(mx) < 0.0001f)
            return;

        float delta = mx * mouseSensitivity * Time.deltaTime;
        if (invertMouse) delta = -delta;

        currentAngle = Mathf.Clamp(currentAngle + delta, minAngle, maxAngle);
        ApplyAngle(currentAngle);
    }

    private bool CanUse()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            return true;

        float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
        if (dist > maxUseDistance)
            return false;

        if (!requireLookingAtDoor)
            return true;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxUseDistance))
        {
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }

        return false;
    }

    private void ApplyAngle(float angleDeg)
    {
        // --- MODO MARCO (nuevo) ---
        if (frameReference != null)
        {
            Vector3 framePos = frameReference.position;
            Quaternion frameRot = frameReference.rotation;

            // Pose cerrada de la puerta en mundo (desde marco)
            Vector3 closedWorldPos = framePos + frameRot * closedPosLocalInFrame;
            Quaternion closedWorldRot = frameRot * Quaternion.Euler(closedEulerLocalInFrame);

            // Bisagra en mundo (desde marco)
            Vector3 hingeWorld = framePos + frameRot * hingeLocalInFrame;

            // Reponer pose base en cada frame para evitar deriva
            transform.SetPositionAndRotation(closedWorldPos, closedWorldRot);

            // Giro horizontal real sobre eje Y del marco
            transform.RotateAround(hingeWorld, frameReference.up, angleDeg);

            if (lockWorldHeight)
            {
                Vector3 p = transform.position;
                p.y = closedWorldPos.y;
                transform.position = p;
            }

            return;
        }

        // --- MODO ORIGINAL (sin marco) ---
        transform.SetPositionAndRotation(basePos, baseRot);
        Vector3 hingeWorldOriginal = basePos + (baseRot * hingeLocalOffset);
        transform.RotateAround(hingeWorldOriginal, Vector3.up, angleDeg);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        if (frameReference != null)
        {
            Vector3 hingeWorld = frameReference.position + frameReference.rotation * hingeLocalInFrame;
            Gizmos.DrawSphere(hingeWorld, 0.03f);
            Gizmos.DrawLine(hingeWorld, hingeWorld + frameReference.up * 0.35f);
        }
        else
        {
            Quaternion rot = Application.isPlaying ? baseRot : transform.rotation;
            Vector3 pos = Application.isPlaying ? basePos : transform.position;
            Vector3 hingeWorld = pos + rot * hingeLocalOffset;
            Gizmos.DrawSphere(hingeWorld, 0.03f);
            Gizmos.DrawLine(hingeWorld, hingeWorld + Vector3.up * 0.35f);
        }
    }
#endif
}