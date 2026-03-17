using System.Collections.Generic;
using UnityEngine;

public class Hallway3HatPuzzleNode : MonoBehaviour
{
    public enum NodeRole
    {
        LeftZone,
        RightZone,
        UnlockTarget
    }

    [Header("Role")]
    [SerializeField] private NodeRole role;

    [Header("Shared Puzzle Nodes")]
    [SerializeField] private Hallway3HatPuzzleNode leftZone;
    [SerializeField] private Hallway3HatPuzzleNode rightZone;
    [SerializeField] private Hallway3HatPuzzleNode unlockTarget;

    [Header("Hat Detection")]
    [SerializeField] private bool requireHatTag = true;
    [SerializeField] private string hatTag = "Hat";

    [Header("Options")]
    [SerializeField] private bool oneShot = true;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = true;

    private readonly HashSet<Collider> _hatsInside = new();
    private bool _alreadyUnlocked;

    private void Awake()
    {
        Collider c = GetComponent<Collider>();

        if (c == null)
        {
            Debug.LogError($"[{name}] Este objeto necesita un Collider.", this);
            return;
        }

        if (role != NodeRole.UnlockTarget && !c.isTrigger)
        {
            Debug.LogWarning($"[{name}] Esta zona debería tener IsTrigger = true.", this);
        }

        if (verboseLogs)
        {
            Debug.Log($"[{name}] Awake role={role}", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (role == NodeRole.UnlockTarget) return;

        if (!PassesFilter(other))
        {
            if (verboseLogs)
                Debug.Log($"[{name}] ENTER ignorado: {other.name}", other);
            return;
        }

        _hatsInside.Add(other);

        if (verboseLogs)
            Debug.Log($"[{name}] ENTER hat: {other.name} | count={_hatsInside.Count}", other);

        EvaluatePuzzle();
    }

    private void OnTriggerExit(Collider other)
    {
        if (role == NodeRole.UnlockTarget) return;

        if (!PassesFilter(other))
            return;

        if (_hatsInside.Remove(other))
        {
            if (verboseLogs)
                Debug.Log($"[{name}] EXIT hat: {other.name} | count={_hatsInside.Count}", other);
        }

        EvaluatePuzzle();
    }

    private bool PassesFilter(Collider other)
    {
        if (other == null) return false;
        if (!requireHatTag) return true;

        return other.CompareTag(hatTag) ||
               (other.transform.root != null && other.transform.root.CompareTag(hatTag));
    }

    public bool IsOccupied()
    {
        return _hatsInside.Count > 0;
    }

    private void EvaluatePuzzle()
    {
        if (leftZone == null || rightZone == null || unlockTarget == null)
        {
            Debug.LogWarning($"[{name}] Faltan referencias del puzzle (left/right/unlockTarget).", this);
            return;
        }

        bool leftOccupied = leftZone.IsOccupied();
        bool rightOccupied = rightZone.IsOccupied();

        if (verboseLogs)
            Debug.Log($"[{name}] EvaluatePuzzle -> left:{leftOccupied} right:{rightOccupied}", this);

        if (leftOccupied && rightOccupied)
        {
            unlockTarget.UnlockNow(oneShot);
        }
    }

    public void UnlockNow(bool useOneShot)
    {
        if (role != NodeRole.UnlockTarget)
        {
            Debug.LogWarning($"[{name}] UnlockNow fue llamado en un nodo que no es UnlockTarget.", this);
            return;
        }

        if (_alreadyUnlocked && useOneShot)
            return;

        _alreadyUnlocked = true;

        Collider c = GetComponent<Collider>();
        if (c != null)
        {
            c.enabled = false;

            if (verboseLogs)
                Debug.Log($"[{name}] >>> UNLOCK TARGET desactivado.", this);
        }
        else
        {
            Debug.LogWarning($"[{name}] No encontré Collider para desactivar.", this);
        }
    }
}