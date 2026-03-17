using UnityEngine;

/// <summary>
/// Hace que el objeto rote hacia la cámara (solo eje Y) cuando el jugador está cerca.
/// Pensado para trajes con tag "Black".
/// No mueve posición, solo rotación en sí mismo.
/// </summary>
public class BlackSuitLookAtCamera : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform targetCamera; // si null, usa Camera.main

    [Header("Behavior")]
    [SerializeField] private float activationDistance = 8f;
    [SerializeField] private float rotationSpeed = 6f;
    [SerializeField] private bool requireTagBlack = true;

    private void Start()
    {
        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main.transform;
    }

    private void Update()
    {
        if (targetCamera == null) return;
        if (requireTagBlack && !CompareTag("Black")) return;

        Vector3 toCam = targetCamera.position - transform.position;
        float dist = toCam.magnitude;
        if (dist > activationDistance) return;

        // Solo rotación horizontal (Y): no inclinar arriba/abajo
        toCam.y = 0f;
        if (toCam.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }
}