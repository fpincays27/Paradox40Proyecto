using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HallwayHatEventController : MonoBehaviour
{
    [Header("Hallway Config")]
    [SerializeField] private int hallwayNumber = 1;
    [SerializeField] private int requiredHatsOverride = 0; // 0 = hallwayNumber
    [SerializeField] private string requiredHatTypeId = "Negro";

    [Header("Scene on suit collision")]
    [SerializeField] private string ronda3SceneName = "Ronda3";

    [Header("Objects to disable on success")]
    [SerializeField] private GameObject suitWithHatRoot;
    [SerializeField] private GameObject blockedDoorRoot;
    [SerializeField] private GameObject[] extraObjectsToDisable;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private int deliveredCount;
    private bool solved;

    // ✅ Importante:
    // cuenta UNA sola vez cada sombrero en ESTE hallway,
    // pero permite reutilizar ese mismo sombrero en OTROS hallways.
    private readonly HashSet<int> countedHatIdsThisHallway = new HashSet<int>();

    private int RequiredCount => requiredHatsOverride > 0 ? requiredHatsOverride : Mathf.Max(1, hallwayNumber);

    public bool IsSolved => solved;

    public void RegisterHatInZone(HatIdentity hat)
    {
        if (solved || hat == null) return;

        // Debe ser del tipo correcto
        if (!string.Equals(hat.HatTypeId, requiredHatTypeId, System.StringComparison.OrdinalIgnoreCase))
        {
            if (debugLogs) Debug.Log($"[HallwayEvent] Tipo inválido: {hat.HatTypeId} (requerido: {requiredHatTypeId})");
            return;
        }

        int id = hat.GetInstanceID();

        // Si ya se contó este sombrero en ESTE hallway, no sumes otra vez
        if (countedHatIdsThisHallway.Contains(id))
            return;

        countedHatIdsThisHallway.Add(id);
        deliveredCount++;

        if (debugLogs) Debug.Log($"[HallwayEvent] Entregados {deliveredCount}/{RequiredCount} en {name}");

        // ❌ Antes: Destroy(hat.gameObject);
        // ✅ Ahora: NO destruir, se queda en la zona y puedes volver a agarrarlo.

        if (deliveredCount >= RequiredCount)
            SolveHallway();
    }

    public void TriggerSuitPenaltyImmediate()
    {
        if (solved) return;
        SceneManager.LoadScene(ronda3SceneName);
    }

    private void SolveHallway()
    {
        if (solved) return;
        solved = true;

        if (suitWithHatRoot != null) suitWithHatRoot.SetActive(false);
        if (blockedDoorRoot != null) blockedDoorRoot.SetActive(false);

        if (extraObjectsToDisable != null)
        {
            for (int i = 0; i < extraObjectsToDisable.Length; i++)
                if (extraObjectsToDisable[i] != null) extraObjectsToDisable[i].SetActive(false);
        }

        if (debugLogs) Debug.Log($"[HallwayEvent] {name} completado.");
    }
}