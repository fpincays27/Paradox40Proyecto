using UnityEngine;

public class InventoryMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    public void Toggle()
    {
        if (panel == null) return;
        panel.SetActive(!panel.activeSelf);
    }

    public void Open()
    {
        if (panel == null) return;
        panel.SetActive(true);
    }

    public void Close()
    {
        if (panel == null) return;
        panel.SetActive(false);
    }
}