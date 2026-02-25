using UnityEngine;

public class ChestInteract : MonoBehaviour
{
    [SerializeField] private RoundFlowController flow;

    private void OnMouseDown()
    {
        flow?.OpenChestUI();
    }
}