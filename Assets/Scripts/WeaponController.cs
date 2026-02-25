using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EscapeQTE escapeQTE;

    [Header("Aim")]
    [SerializeField] private float rotateSpeed = 25f;
    [SerializeField] private float angleOffset = 0f;

    [Header("Shoot")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask shootMask = ~0;

    private Camera cam;
    private bool canShoot = true;

    private void Start()
    {
        cam = Camera.main;
        canShoot = true;
    }

    private void Update()
    {
        if (cam == null) return;

        Aim2D();

        if (canShoot && Input.GetMouseButtonDown(0))
            Shoot2D();
    }

    public void DisableShooting()
    {
        canShoot = false;
    }

    private void Aim2D()
    {
        Vector3 mouse = Input.mousePosition;
        float zToCamera = Mathf.Abs(cam.transform.position.z - transform.position.z);
        mouse.z = zToCamera;

        Vector3 world = cam.ScreenToWorldPoint(mouse);
        Vector2 dir = (Vector2)(world - transform.position);

        if (dir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle += angleOffset;

        Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
    }

    private void Shoot2D()
    {
        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;

        Vector3 mouse = Input.mousePosition;
        float zToCamera = Mathf.Abs(cam.transform.position.z - transform.position.z);
        mouse.z = zToCamera;

        Vector2 target = (Vector2)cam.ScreenToWorldPoint(mouse);
        Vector2 dir = (target - origin).normalized;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxDistance, shootMask);
        if (!hit.collider) return;

        EyeTarget eye = hit.collider.GetComponentInParent<EyeTarget>();
        if (eye == null) return;

        // ✅ SFX: disparo (solo si realmente pegaste a un ojo)
        SFXManager.I?.PlayShot();

        // ✅ Acertaste: desactiva disparos para siempre
        canShoot = false;

        // ✅ Lógica del ojo (herida, desaparición, UI, QTE…)
        eye.OnShot();

        // (NO iniciamos QTE aquí; lo iniciará el panel de "CORRE!")
    }
}