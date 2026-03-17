using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private int maxHP = 40;

    [Header("Damage To Enemy (por match)")]
    [SerializeField] private int damageToEnemyRonda1 = 10;
    [SerializeField] private int damageToEnemyRonda2 = 5;
    [SerializeField] private int damageToEnemyRonda3 = 5;
    [SerializeField] private int damageToEnemyRonda4 = 3;
    [SerializeField] private int damageToEnemyFallback = 10;

    [Header("Damage To Player (por no-match)")]
    [SerializeField] private int damageToPlayerRonda1 = 5;
    [SerializeField] private int damageToPlayerRonda2 = 5;
    [SerializeField] private int damageToPlayerRonda3 = 5;
    [SerializeField] private int damageToPlayerRonda4 = 5;
    [SerializeField] private int damageToPlayerFallback = 5;

    [Header("Scenes")]
    [SerializeField] private string winSceneName = "Win";
    [SerializeField] private string gameOverSceneName = "GameOver";

    private bool gameEnded = false;
    private int damageToEnemyPerMatch;
    private int damageToPlayerPerMiss;

    public int PlayerHP { get; private set; }
    public int EnemyHP { get; private set; }

    public System.Action<int, int> OnHpChanged;
    public System.Action OnEnemyDefeated;

    private void Awake()
    {
        PlayerHP = maxHP;
        EnemyHP = maxHP;

        SetupDamageByScene();
        OnHpChanged?.Invoke(PlayerHP, EnemyHP);
    }

    private void SetupDamageByScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Ronda1":
                damageToEnemyPerMatch = damageToEnemyRonda1;
                damageToPlayerPerMiss = damageToPlayerRonda1;
                break;
            case "Ronda2":
                damageToEnemyPerMatch = damageToEnemyRonda2;
                damageToPlayerPerMiss = damageToPlayerRonda2;
                break;
            case "Ronda3":
                damageToEnemyPerMatch = damageToEnemyRonda3;
                damageToPlayerPerMiss = damageToPlayerRonda3;
                break;
            case "Ronda4":
                damageToEnemyPerMatch = damageToEnemyRonda4;
                damageToPlayerPerMiss = damageToPlayerRonda4;
                break;
            default:
                damageToEnemyPerMatch = damageToEnemyFallback;
                damageToPlayerPerMiss = damageToPlayerFallback;
                break;
        }
    }

    public void ApplyMatchResult(bool isMatch)
    {
        if (gameEnded) return;

        if (isMatch)
            EnemyHP = Mathf.Max(0, EnemyHP - damageToEnemyPerMatch);
        else
            PlayerHP = Mathf.Max(0, PlayerHP - damageToPlayerPerMiss);

        OnHpChanged?.Invoke(PlayerHP, EnemyHP);
        CheckEndGame();
    }

    public bool RecoverPlayerHP(int amount)
    {
        if (gameEnded) return false;

        int heal = Mathf.Max(0, amount);
        if (heal <= 0) return false;

        int before = PlayerHP;
        PlayerHP = Mathf.Min(maxHP, PlayerHP + heal);

        bool changed = PlayerHP != before;
        if (changed)
            OnHpChanged?.Invoke(PlayerHP, EnemyHP);

        return changed;
    }

    public void ResetEnemyHPToMax()
    {
        if (gameEnded) return;

        EnemyHP = maxHP;
        OnHpChanged?.Invoke(PlayerHP, EnemyHP);
    }

    private void CheckEndGame()
    {
        if (gameEnded) return;

        if (EnemyHP <= 0)
        {
            gameEnded = true;
            OnEnemyDefeated?.Invoke();
            return;
        }

        if (PlayerHP <= 0)
        {
            gameEnded = true;
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    public void ForceLose()
    {
        if (gameEnded) return;

        PlayerHP = 0;
        OnHpChanged?.Invoke(PlayerHP, EnemyHP);
        CheckEndGame();
    }

    public void GoToWin()
    {
        SceneManager.LoadScene(winSceneName);
    }
}