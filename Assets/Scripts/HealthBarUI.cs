using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HealthManager health;
    [SerializeField] private Slider playerSlider;
    [SerializeField] private Slider enemySlider;
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI enemyText;

    private void OnEnable()
    {
        if (health != null)
            health.OnHpChanged += Refresh;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnHpChanged -= Refresh;
    }

    private void Start()
    {
        // Inicializa UI aunque el evento no haya disparado aún
        if (health != null)
            Refresh(health.PlayerHP, health.EnemyHP);
    }

    private void Refresh(int playerHP, int enemyHP)
    {
        if (playerSlider != null) playerSlider.value = playerHP;
        if (enemySlider != null) enemySlider.value = enemyHP;

        if (playerText != null) playerText.text = $"{playerHP}/40";
        if (enemyText != null) enemyText.text = $"{enemyHP}/40";
    }
}