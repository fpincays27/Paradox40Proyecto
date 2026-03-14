using UnityEngine;

public class ChestChoiceUI : MonoBehaviour
{
    private RoundFlowController flow;

    private void Awake()
    {
        Debug.Log("[ChestChoiceUI] Awake");
    }

    private void OnEnable()
    {
        Debug.Log("[ChestChoiceUI] OnEnable -> PANEL ACTIVO");
    }

    private void OnDisable()
    {
        Debug.Log("[ChestChoiceUI] OnDisable -> PANEL INACTIVO");
    }

    public void Show(RoundFlowController flowController)
    {
        Debug.Log("[ChestChoiceUI] Show() llamado");

        flow = flowController;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        Debug.Log("[ChestChoiceUI] Hide() llamado");
        gameObject.SetActive(false);
    }

    public void ChooseWeapon()
    {
        Debug.Log("[ChestChoiceUI] ChooseWeapon()");
        Hide();
        flow?.OnChooseWeapon();
    }

    // Nuevo nombre genérico
    public void ChooseToken()
    {
        Debug.Log("[ChestChoiceUI] ChooseToken()");
        Hide();
        flow?.OnChooseToken();
    }

    // Compatibilidad con bindings antiguos en el Inspector
    public void ChooseLifeToken()
    {
        ChooseToken();
    }
}