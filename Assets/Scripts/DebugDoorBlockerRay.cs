using UnityEngine;

public class DebugDoorBlockerRay : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float distance = 4f;
    [SerializeField] private LayerMask mask = ~0;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (cam == null) return;

        if (Input.GetMouseButtonDown(1)) // click derecho
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Ignore))
            {
                Debug.Log($"[BLOCKER] Hit: {hit.collider.name} | GO: {hit.collider.gameObject.name} | Path: {GetPath(hit.collider.transform)}");
            }
            else
            {
                Debug.Log("[BLOCKER] No hit");
            }
        }
    }

    private string GetPath(Transform t)
    {
        string p = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            p = t.name + "/" + p;
        }
        return p;
    }
}