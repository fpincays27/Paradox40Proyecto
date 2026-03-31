using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CinematicPanelData
{
    [Header("Visual")]
    public Sprite panelSprite;

    [Header("Audio del panel (diálogo)")]
    public AudioClip panelSfx;
}

public class CinematicSequence : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private List<CinematicPanelData> panels = new List<CinematicPanelData>();
    [SerializeField] private Image panelImage;

    [Header("Texto 'Presiona Spacebar'")]
    [SerializeField] private TMP_Text pressSpaceText;
    [SerializeField] private Text legacyPressSpaceText;
    [SerializeField, Min(0f)] private float showPressTextDelay = 30f;
    [SerializeField, Min(0.05f)] private float blinkInterval = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip defaultPanelSfx; // opcional fallback
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] private bool stopPreviousAudioOnAdvance = true; // recomendado para diálogos

    [Header("Cambio de escena")]
    [SerializeField] private bool useSceneName = true;
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private int nextSceneBuildIndex = -1;
    [SerializeField] private bool allowSkipOnlyWithSpace = true;

    private int currentIndex = 0;
    private Coroutine blinkRoutine;
    private Coroutine delayRoutine;

    private bool canShowPressText = false;
    private bool isTransitioning = false;

    private void Start()
    {
        if (panelImage == null)
        {
            Debug.LogError("[CinematicSequence] Falta asignar panelImage.");
            enabled = false;
            return;
        }

        if (panels == null || panels.Count == 0)
        {
            Debug.LogError("[CinematicSequence] No hay paneles configurados.");
            enabled = false;
            return;
        }

        SetPressTextVisible(false);
        ShowPanel(0);
    }

    private void Update()
    {
        if (isTransitioning) return;

        if (allowSkipOnlyWithSpace && Input.GetKeyDown(KeyCode.Space))
        {
            Advance();
        }
    }

    public void Advance()
    {
        if (currentIndex < panels.Count - 1)
        {
            currentIndex++;
            ShowPanel(currentIndex);
        }
        else
        {
            GoToNextSceneWithTransition();
        }
    }

    private void ShowPanel(int index)
    {
        if (index < 0 || index >= panels.Count) return;

        // Imagen del panel
        panelImage.sprite = panels[index].panelSprite;
        panelImage.preserveAspect = true;

        // Audio del panel
        PlayPanelSfx(index);

        // Texto "Presiona Spacebar" (delay + parpadeo)
        canShowPressText = false;
        SetPressTextVisible(false);

        if (delayRoutine != null) StopCoroutine(delayRoutine);
        delayRoutine = StartCoroutine(ShowPressTextAfterDelay());
    }

    private void PlayPanelSfx(int panelIndex)
    {
        if (sfxSource == null) return;

        AudioClip clip = panels[panelIndex].panelSfx != null
            ? panels[panelIndex].panelSfx
            : defaultPanelSfx;

        if (clip == null) return;

        if (stopPreviousAudioOnAdvance && sfxSource.isPlaying)
            sfxSource.Stop();

        // Para diálogo por panel, mejor setear clip + Play (no PlayOneShot)
        sfxSource.clip = clip;
        sfxSource.volume = sfxVolume;
        sfxSource.Play();
    }

    private IEnumerator ShowPressTextAfterDelay()
    {
        yield return new WaitForSeconds(showPressTextDelay);
        canShowPressText = true;

        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkPressText());
    }

    private IEnumerator BlinkPressText()
    {
        while (canShowPressText)
        {
            SetPressTextVisible(true);
            yield return new WaitForSeconds(blinkInterval);

            SetPressTextVisible(false);
            yield return new WaitForSeconds(blinkInterval);
        }

        SetPressTextVisible(false);
    }

    private void SetPressTextVisible(bool value)
    {
        if (pressSpaceText != null) pressSpaceText.enabled = value;
        if (legacyPressSpaceText != null) legacyPressSpaceText.enabled = value;
    }

    private void GoToNextSceneWithTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        string gameSceneName = ResolveTargetSceneName();

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("[CinematicSequence] No se pudo resolver el nombre de la siguiente escena.");
            isTransitioning = false;
            return;
        }

        SceneTransitionPanel.LoadSceneWithTransition(gameSceneName);
    }

    private string ResolveTargetSceneName()
    {
        if (useSceneName && !string.IsNullOrWhiteSpace(nextSceneName))
            return nextSceneName;

        if (nextSceneBuildIndex >= 0 && nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
            return BuildIndexToSceneName(nextSceneBuildIndex);

        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int targetIndex = currentBuildIndex + 1;

        if (targetIndex >= 0 && targetIndex < SceneManager.sceneCountInBuildSettings)
            return BuildIndexToSceneName(targetIndex);

        return string.Empty;
    }

    private string BuildIndexToSceneName(int buildIndex)
    {
        string scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        if (string.IsNullOrWhiteSpace(scenePath)) return string.Empty;
        return Path.GetFileNameWithoutExtension(scenePath);
    }
}