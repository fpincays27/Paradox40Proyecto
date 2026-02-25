using UnityEngine;
using UnityEngine.SceneManagement;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    public bool HasWeapon { get; private set; }
    public int LifeTokens { get; private set; }

    // Persistencia de ojos
    public bool LeftEyeDestroyed { get; set; }
    public bool RightEyeDestroyed { get; set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================
    // Progreso normal
    // =========================

    public void GainWeapon() => HasWeapon = true;

    public void GainLifeToken() => LifeTokens++;

    public bool TryConsumeLifeToken()
    {
        if (LifeTokens <= 0) return false;
        LifeTokens--;
        return true;
    }

    // =========================
    // RESET COMPLETO (Nuevo Juego)
    // =========================

    public void ResetAll()
    {
        HasWeapon = false;
        LifeTokens = 0;
        LeftEyeDestroyed = false;
        RightEyeDestroyed = false;
    }
}