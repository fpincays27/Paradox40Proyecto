using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtons : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Ronda1";

    public void RestartGame()
    {
        // 🔄 Reiniciar todo el progreso
        GameProgress.Instance?.ResetAll();

        // 🔁 Cargar escena inicial
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}