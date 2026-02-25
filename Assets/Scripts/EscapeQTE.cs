using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class EscapeQTE : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float duration = 10f;
    [SerializeField] private int requiredPresses = 10;

    [Header("Scenes")]
    [SerializeField] private string successScene = "SalonPrincipal";
    [SerializeField] private string failScene = "GameOver";

    [Header("UI (opcional)")]
    [SerializeField] private TextMeshProUGUI infoText;

    private bool running = false;
    private float timeLeft;
    private int presses;

    private void Start()
    {
        UpdateUI();
    }

    public void StartEscape()
    {
        if (running) return;

        running = true;
        timeLeft = duration;
        presses = 0;
        UpdateUI();

        Debug.Log("Escape iniciado: presiona SPACE 10 veces en 10 segundos");
    }

    private void Update()
    {
        if (!running) return;

        timeLeft -= Time.deltaTime;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            presses++;
            UpdateUI();

            if (presses >= requiredPresses)
            {
                running = false;
                SceneManager.LoadScene(successScene);
                return;
            }
        }

        if (timeLeft <= 0f)
        {
            running = false;
            SceneManager.LoadScene(failScene);
        }
    }

    private void UpdateUI()
    {
        if (infoText == null) return;

        if (!running)
            infoText.text = "";
        else
            infoText.text = $"ESCAPA!\nSpace: {presses}/{requiredPresses}\nTiempo: {timeLeft:0.0}s";
    }
    public void SetSuccessScene(string sceneName)
    {
        successScene = sceneName;
    }
}