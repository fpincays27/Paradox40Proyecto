using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private int maxHP = 40;

    [Header("Damage")]
    [SerializeField] private int damageRonda1 = 10;
    [SerializeField] private int damageRonda2 = 5;
    [SerializeField] private int ownDamagePerResult = 5;

    [Header("Scenes")]
    [SerializeField] private string winSceneName = "Win";
    [SerializeField] private string gameOverSceneName = "GameOver";

    private bool gameEnded = false;
    private int damagePerResult;

    public int PlayerHP { get; private set; }
    public int EnemyHP { get; private set; }

    public System.Action<int, int> OnHpChanged;

    // ✅ para que tu HandView pueda suscribirse
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

        if (sceneName == "Ronda2")
            damagePerResult = damageRonda2;
        else
            damagePerResult = damageRonda1;
    }

    public void ApplyMatchResult(bool isMatch)
    {
        if (gameEnded) return;

        if (isMatch)
            EnemyHP = Mathf.Max(0, EnemyHP - damagePerResult);
        else
            PlayerHP = Mathf.Max(0, PlayerHP - ownDamagePerResult);

        OnHpChanged?.Invoke(PlayerHP, EnemyHP);
        CheckEndGame();
    }

    private void CheckEndGame()
    {
        if (gameEnded) return;

        if (EnemyHP <= 0)
        {
            gameEnded = true;
            OnEnemyDefeated?.Invoke(); // HandView -> HandleEnemyDefeated()
            return; // 👈 no cargues Win aún
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

    // Útil si después de elegir cofre quieres ir a Win
    public void GoToWin()
    {
        SceneManager.LoadScene(winSceneName);
    }
}