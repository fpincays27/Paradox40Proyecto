using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenuPersistent : MonoBehaviour
{
    public static PauseMenuPersistent Instance { get; private set; }

    [Header("UI Root del menú de pausa")]
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("Volumen")]
    [Tooltip("Opcional: asigna un AudioMixer con parámetro expuesto (ej: MasterVolume). Si no lo asignas, usa AudioListener.volume.")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string mixerVolumeParameter = "MasterVolume";
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeValueText;

    [Header("Sensibilidad mouse")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityValueText;
    [SerializeField] private float minSensitivity = 0.1f;
    [SerializeField] private float maxSensitivity = 10f;

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    // Keys PlayerPrefs
    private const string PREF_VOLUME = "settings_master_volume_01";       // 0..1
    private const string PREF_SENS = "settings_mouse_sensitivity_01";     // min..max

    public static bool IsPaused { get; private set; }
    public static float MouseSensitivity { get; private set; } = 2f;

    private void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        LoadSettings();
        ApplySettingsToUI();
        ApplyVolume(GetSavedVolume());
        ApplyMouseSensitivity(GetSavedSensitivity());

        SetPauseMenuVisible(false);
        ResumeGameHard();
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (IsPaused) ResumeGame();
            else PauseGame();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Al cambiar escena, asegurar estado correcto
        SetPauseMenuVisible(false);
        ResumeGameHard();
    }

    // -------------------- PAUSA --------------------
    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        SetPauseMenuVisible(true);

        // En este proyecto queremos seguir usando mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SetPauseMenuVisible(false);

        // IMPORTANTE: mantener mouse visible para interactuar en escena
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGameHard()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        // También al cargar escena, mantenemos mouse usable
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetPauseMenuVisible(bool visible)
    {
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(visible);
    }

    // -------------------- SETTINGS --------------------
    private void LoadSettings()
    {
        float v = PlayerPrefs.GetFloat(PREF_VOLUME, 1f);
        float s = PlayerPrefs.GetFloat(PREF_SENS, 2f);

        v = Mathf.Clamp01(v);
        s = Mathf.Clamp(s, minSensitivity, maxSensitivity);

        SaveVolume(v);
        SaveSensitivity(s);
    }

    public void OnVolumeSliderChanged(float value01)
    {
        value01 = Mathf.Clamp01(value01);
        ApplyVolume(value01);
        SaveVolume(value01);
        UpdateVolumeText(value01);
    }

    public void OnSensitivitySliderChanged(float sens)
    {
        sens = Mathf.Clamp(sens, minSensitivity, maxSensitivity);
        ApplyMouseSensitivity(sens);
        SaveSensitivity(sens);
        UpdateSensitivityText(sens);
    }

    private void ApplySettingsToUI()
    {
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.wholeNumbers = false;
            volumeSlider.SetValueWithoutNotify(GetSavedVolume());
            UpdateVolumeText(volumeSlider.value);
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = minSensitivity;
            sensitivitySlider.maxValue = maxSensitivity;
            sensitivitySlider.wholeNumbers = false;
            sensitivitySlider.SetValueWithoutNotify(GetSavedSensitivity());
            UpdateSensitivityText(sensitivitySlider.value);
            sensitivitySlider.onValueChanged.RemoveAllListeners();
            sensitivitySlider.onValueChanged.AddListener(OnSensitivitySliderChanged);
        }
    }

    private void ApplyVolume(float value01)
    {
        if (audioMixer != null && !string.IsNullOrWhiteSpace(mixerVolumeParameter))
        {
            float dB = (value01 <= 0.0001f) ? -80f : Mathf.Log10(value01) * 20f;
            audioMixer.SetFloat(mixerVolumeParameter, dB);
        }
        else
        {
            AudioListener.volume = value01;
        }
    }

    private void ApplyMouseSensitivity(float sens)
    {
        MouseSensitivity = sens;
    }

    private void UpdateVolumeText(float value01)
    {
        if (volumeValueText != null)
            volumeValueText.text = $"Volumen: {Mathf.RoundToInt(value01 * 100f)}%";
    }

    private void UpdateSensitivityText(float sens)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = $"Sensibilidad: {sens:0.00}";
    }

    private float GetSavedVolume() => PlayerPrefs.GetFloat(PREF_VOLUME, 1f);
    private float GetSavedSensitivity() => PlayerPrefs.GetFloat(PREF_SENS, 2f);

    private void SaveVolume(float v)
    {
        PlayerPrefs.SetFloat(PREF_VOLUME, v);
        PlayerPrefs.Save();
    }

    private void SaveSensitivity(float s)
    {
        PlayerPrefs.SetFloat(PREF_SENS, s);
        PlayerPrefs.Save();
    }

    // Botones UI
    public void OnClickResume() => ResumeGame();

    public void OnClickQuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}