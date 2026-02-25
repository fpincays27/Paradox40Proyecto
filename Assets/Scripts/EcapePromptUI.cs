using UnityEngine;

public class EscapePromptUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EscapeQTE escapeQTE;
    [SerializeField] private RoundFlowController flow; // opcional (para ocultar arma, etc.)

    private GameObject panelRoot;

    // ✅ NUEVO: arma instanciada que el panel debe destruir al correr
    private GameObject weaponToDestroy;

    private void Awake()
    {
        // Si este script está en un hijo, el panel real suele ser el padre
        panelRoot = transform.parent != null ? transform.parent.gameObject : gameObject;

        // Apagar al inicio
        panelRoot.SetActive(false);
    }

    // ✅ NUEVO: para que RoundFlowController pueda inyectar la referencia
    public void SetFlow(RoundFlowController f)
    {
        flow = f;
    }

    // ✅ NUEVO: RoundFlowController le pasa la instancia del arma
    public void SetWeapon(GameObject weaponInstance)
    {
        weaponToDestroy = weaponInstance;
    }

    public void Show()
    {
        if (panelRoot == null) return;

        panelRoot.SetActive(true);
        panelRoot.transform.SetAsLastSibling(); // encima de todo
    }

    public void Hide()
    {
        if (panelRoot == null) return;
        panelRoot.SetActive(false);
    }

    // ✅ Esta es la función que debes poner en el botón OnClick()
    public void OnClickRun()
    {
        Hide();

        // ✅ destruir el arma spawneada (la que nos pasaron)
        if (weaponToDestroy != null)
        {
            Destroy(weaponToDestroy);
            weaponToDestroy = null;
        }

        // (Opcional) Si aún quieres destruir "cualquier arma" por flow
        // flow?.HideWeaponIfAny(); Queda pendiente el desarrollo de mecánica de escape por presion 

        // Inicia el QTE
        if (escapeQTE != null)
            escapeQTE.StartEscape();
    }
}