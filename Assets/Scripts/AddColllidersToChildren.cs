using UnityEngine;

public class AddCollidersToChildren_Improved : MonoBehaviour
{
    [Header("Preferencias")]
    public bool useBoxColliderFallback = true;   // si no hay mesh, usa BoxCollider con bounds
    public bool preferBoxForLargeSurfaces = true; // útil para suelos grandes
    public float largeSurfaceArea = 25f;         // umbral (aprox) para usar BoxCollider (X*Z)

    [ContextMenu("Add Colliders To Children (Improved)")]
    public void AddColliders()
    {
        int meshAdded = 0, boxAdded = 0, skipped = 0;

        var renderers = GetComponentsInChildren<Renderer>(true);

        foreach (var r in renderers)
        {
            var go = r.gameObject;

            // Si ya tiene algún collider, no lo tocamos
            if (go.GetComponent<Collider>() != null)
            {
                skipped++;
                continue;
            }

            // Intento 1: MeshFilter
            Mesh mesh = null;
            var mf = go.GetComponent<MeshFilter>();
            if (mf != null) mesh = mf.sharedMesh;

            // Intento 2: SkinnedMeshRenderer (por si acaso)
            if (mesh == null && r is SkinnedMeshRenderer smr)
                mesh = smr.sharedMesh;

            // Si hay mesh, ponemos MeshCollider (o Box si es superficie grande)
            if (mesh != null)
            {
                if (preferBoxForLargeSurfaces)
                {
                    // si es muy grande/planito, BoxCollider suele ser mejor y más estable
                    var b = r.bounds.size;
                    float area = Mathf.Abs(b.x * b.z);
                    if (area >= largeSurfaceArea)
                    {
                        AddBoxFromRendererBounds(go, r);
                        boxAdded++;
                        continue;
                    }
                }

                var mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh;
                mc.convex = false;     // escenario
                mc.isTrigger = false;
                meshAdded++;
                continue;
            }

            // Fallback: si no hay mesh pero sí renderer, usar BoxCollider por bounds
            if (useBoxColliderFallback)
            {
                AddBoxFromRendererBounds(go, r);
                boxAdded++;
            }
            else
            {
                skipped++;
            }
        }

        Debug.Log($"[Colliders] Mesh: {meshAdded} | Box: {boxAdded} | Skipped: {skipped}");
    }

    void AddBoxFromRendererBounds(GameObject go, Renderer r)
    {
        var bc = go.AddComponent<BoxCollider>();
        var bounds = r.bounds;

        // bounds está en mundo; convertimos a local del objeto
        var centerWorld = bounds.center;
        var sizeWorld = bounds.size;

        bc.center = go.transform.InverseTransformPoint(centerWorld);

        // Convertir tamaño mundo a local (aprox)
        Vector3 lossy = go.transform.lossyScale;
        bc.size = new Vector3(
            SafeDiv(sizeWorld.x, lossy.x),
            SafeDiv(sizeWorld.y, lossy.y),
            SafeDiv(sizeWorld.z, lossy.z)
        );

        bc.isTrigger = false;
    }

    float SafeDiv(float a, float b) => Mathf.Abs(b) < 1e-6f ? a : a / b;
}