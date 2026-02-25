using UnityEngine;

public class ChestChoiceUI : MonoBehaviour
{
    private RoundFlowController flow;

    // ❌ NO lo apagues en Awake, porque puede pisar un Show()
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

    // Mostrar panel
    public void Show(RoundFlowController flowController)
    {
        Debug.Log("[ChestChoiceUI] Show() llamado");

        flow = flowController;

        // Activa panel
        gameObject.SetActive(true);

        // Súbelo encima de todo en la UI
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        Debug.Log("[ChestChoiceUI] Hide() llamado");
        gameObject.SetActive(false);
    }

    // Botón: “Tomar arma”
    public void ChooseWeapon()
    {
        Debug.Log("[ChestChoiceUI] ChooseWeapon()");
        Hide();
        flow?.OnChooseWeapon();
    }

    // Botón: “Tomar token”
    public void ChooseLifeToken()
    {
        Debug.Log("[ChestChoiceUI] ChooseLifeToken()");
        Hide();
        flow?.OnChooseToken();
    }
}