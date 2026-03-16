using UnityEngine;

public class TriggerRelayHallway : MonoBehaviour
{
    public enum RelayMode
    {
        ZoneS,
        SuitBlocker
    }

    [Header("Mode")]
    [SerializeField] private RelayMode mode = RelayMode.ZoneS;

    [Header("Refs")]
    [SerializeField] private HallwayHatEventController hallwayController;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        if (hallwayController == null)
            hallwayController = GetComponentInParent<HallwayHatEventController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hallwayController == null) return;

        if (mode == RelayMode.ZoneS)
        {
            HatIdentity hat = other.GetComponentInParent<HatIdentity>();
            if (hat != null)
                hallwayController.RegisterHatInZone(hat);
        }
        else if (mode == RelayMode.SuitBlocker)
        {
            // inmediato, sin tecla
            if (other.CompareTag(playerTag))
                hallwayController.TriggerSuitPenaltyImmediate();
        }
    }
}