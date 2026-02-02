using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private int maxHP = 40;
    [SerializeField] private int damagePerResult = 10;
    [SerializeField] private int ownDamagePerResult = 5;

    [Header("Scenes")]
    [SerializeField] private string winSceneName = "Win";
    [SerializeField] private string gameOverSceneName = "GameOver";

private bool gameEnded = false;

    public int PlayerHP { get; private set; }
    public int EnemyHP { get; private set; }

    public System.Action<int, int> OnHpChanged; // playerHP, enemyHP

    private void Awake()
    {
        PlayerHP = maxHP;
        EnemyHP = maxHP;
        OnHpChanged?.Invoke(PlayerHP, EnemyHP);
    }

    public void ApplyMatchResult(bool isMatch)
    {
        if (isMatch)
        {
            EnemyHP = Mathf.Max(0, EnemyHP - damagePerResult);
        }
        else
        {
            PlayerHP = Mathf.Max(0, PlayerHP - ownDamagePerResult);
        }

        OnHpChanged?.Invoke(PlayerHP, EnemyHP);
        
        CheckEndGame();
    }
    private void CheckEndGame()
    {
        if (gameEnded) return;

        if (EnemyHP <= 0)
        {
            gameEnded = true;
            SceneManager.LoadScene(winSceneName);
        }
        else if (PlayerHP <= 0)
        {
            gameEnded = true;
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}