using UnityEngine;

public class SceneButtons : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Ronda1";

    public void RestartGame()
    {
        GameProgress.Instance?.ResetAll();
        SceneTransitionPanel.LoadSceneWithTransition(gameSceneName);
    }

    public void PlayGame()
    {
        SceneTransitionPanel.LoadSceneWithTransition(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}