using UnityEngine;


public class HatCarryClick : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CharacterController playerController; // capsule del jugador (opcional)


    [Header("Carry Settings")]
    [SerializeField] private float carryDistance = 1.8f;
    [SerializeField] private float carryHeightOffset = -0.15f;
    [SerializeField] private float followSmooth = 14f;


    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask = ~0;


    [Header("Drop")]
    [SerializeField] private LayerMask groundMask = ~0;


    [Header("Physics")]
    [SerializeField] private bool ignoreCollisionWithPlayerWhileCarried = true;


    private Rigidbody rb;
    private Collider[] hatColliders;
    private Collider playerCollider;


    private bool isCarried;
    private Quaternion carriedRotation; // mantiene la rotación que tenía al agarrarlo
    private Vector3 velocityRef;        // smoothing manual


    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        hatColliders = GetComponentsInChildren<Collider>(true);


        if (playerController == null)
            playerController = FindObjectOfType<CharacterController>();


        if (playerController != null)
            playerCollider = playerController.GetComponent<Collider>();


        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isCarried) TryPick();
            else Drop();
        }
    }


    private void FixedUpdate()
    {
        if (!isCarried || rb == null || playerCamera == null) return;


        Vector3 targetPos = playerCamera.transform.position
                            + playerCamera.transform.forward * carryDistance
                            + playerCamera.transform.up * carryHeightOffset;


        // Mover con Rigidbody para respetar colisiones
        Vector3 newPos = Vector3.SmoothDamp(rb.position, targetPos, ref velocityRef, 1f / Mathf.Max(1f, followSmooth));
        rb.MovePosition(newPos);


        // Mantener la misma rotación que tenía al agarrarlo (sin girarlo)
        rb.MoveRotation(carriedRotation);
    }


    private void TryPick()
    {
        if (playerCamera == null) return;


        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Ignore))
            return;


        HatCarryClick target = hit.transform.GetComponentInParent<HatCarryClick>();
        if (target != this) return;


        isCarried = true;
        carriedRotation = transform.rotation; // conserva orientación actual
        velocityRef = Vector3.zero;


        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;       // mientras lo cargas
            rb.isKinematic = false;      // mantener físicas y colisiones
            rb.freezeRotation = true;    // no girar accidentalmente
        }


        SetPlayerCollisionIgnored(true);
    }


    private void Drop()
    {
        isCarried = false;


        if (rb != null)
        {
            rb.freezeRotation = false;
            rb.useGravity = true;        // al soltar, cae al suelo
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }


        SetPlayerCollisionIgnored(false);


        // Opcional: si miras al suelo cerca, lo acomoda un poco
        if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance * 2f, groundMask, QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point + hit.normal * 0.03f;
            }
        }
    }


    private void SetPlayerCollisionIgnored(bool ignored)
    {
        if (!ignoreCollisionWithPlayerWhileCarried) return;
        if (playerCollider == null || hatColliders == null) return;


        for (int i = 0; i < hatColliders.Length; i++)
        {
            if (hatColliders[i] != null)
                Physics.IgnoreCollision(hatColliders[i], playerCollider, ignored);
        }
    }
}
