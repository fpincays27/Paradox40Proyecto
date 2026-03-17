using System.Collections.Generic;
using UnityEngine;

public class Hallway2LeftZoneUnlockRight : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private bool detectHats = true;
    [SerializeField] private bool requireHatTag = true;
    [SerializeField] private string hatTag = "Hat";
    [SerializeField] private bool oneShot = true;

    [Header("Unlock right side")]
    [SerializeField] private List<GameObject> rightObjectsToDisable = new();
    [SerializeField] private List<Collider> rightCollidersToDisable = new();
    [SerializeField] private List<MonoBehaviour> rightScriptsToDisable = new();

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = true;
    [SerializeField] private bool logStay = false; // activa solo para debug intensivo

    private bool _alreadyTriggered;

    private void Awake()
    {
        Collider c = GetComponent<Collider>();
        if (c == null)
        {
            Debug.LogError($"[{name}] No collider en este objeto. Debe tener BoxCollider (IsTrigger=true).", this);
            return;
        }

        if (!c.isTrigger)
        {
            Debug.LogWarning($"[{name}] El collider NO está en IsTrigger. OnTriggerEnter no se disparará.", this);
        }

        if (verboseLogs)
        {
            Debug.Log($"[{name}] Awake -> detectHats:{detectHats} requireHatTag:{requireHatTag} hatTag:{hatTag} oneShot:{oneShot}", this);
            Debug.Log($"[{name}] Config -> rightObjects:{rightObjectsToDisable.Count}, rightColliders:{rightCollidersToDisable.Count}, rightScripts:{rightScriptsToDisable.Count}", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (verboseLogs)
            Debug.Log($"[{name}] ENTER con: {other.name} | tag:{other.tag} | layer:{LayerMask.LayerToName(other.gameObject.layer)}", other);

        if (_alreadyTriggered && oneShot)
        {
            if (verboseLogs) Debug.Log($"[{name}] Ignorado por oneShot (ya activado).", this);
            return;
        }

        if (!PassesFilter(other))
        {
            if (verboseLogs) Debug.Log($"[{name}] ENTER filtrado (no cumple condiciones de sombrero/tag).", other);
            return;
        }

        UnlockRight(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!logStay) return;
        Debug.Log($"[{name}] STAY con: {other.name}", other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (verboseLogs)
            Debug.Log($"[{name}] EXIT con: {other.name}", other);
    }

    private bool PassesFilter(Collider other)
    {
        if (!detectHats) return true;

        if (requireHatTag)
        {
            bool okTag = other.CompareTag(hatTag) || (other.transform.root != null && other.transform.root.CompareTag(hatTag));
            return okTag;
        }

        // Si no requiere tag, acepta cualquier cosa que entre.
        return true;
    }

    private void UnlockRight(Collider cause)
    {
        _alreadyTriggered = true;

        Debug.Log($"[{name}] >>> UNLOCK RIGHT disparado por: {cause.name}", cause);

        // Disable GOs
        for (int i = 0; i < rightObjectsToDisable.Count; i++)
        {
            GameObject go = rightObjectsToDisable[i];
            if (go == null)
            {
                Debug.LogWarning($"[{name}] rightObjectsToDisable[{i}] es NULL.", this);
                continue;
            }

            Debug.Log($"[{name}] Disable GO: {go.name}", go);
            go.SetActive(false);
        }

        // Disable Colliders
        for (int i = 0; i < rightCollidersToDisable.Count; i++)
        {
            Collider col = rightCollidersToDisable[i];
            if (col == null)
            {
                Debug.LogWarning($"[{name}] rightCollidersToDisable[{i}] es NULL.", this);
                continue;
            }

            Debug.Log($"[{name}] Disable Collider: {col.name}", col);
            col.enabled = false;
        }

        // Disable Scripts
        for (int i = 0; i < rightScriptsToDisable.Count; i++)
        {
            MonoBehaviour mb = rightScriptsToDisable[i];
            if (mb == null)
            {
                Debug.LogWarning($"[{name}] rightScriptsToDisable[{i}] es NULL.", this);
                continue;
            }

            Debug.Log($"[{name}] Disable Script: {mb.GetType().Name} en {mb.name}", mb);
            mb.enabled = false;
        }
    }
}