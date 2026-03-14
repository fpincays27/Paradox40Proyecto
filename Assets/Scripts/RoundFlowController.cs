using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class RoundFlowController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HealthManager health;
    [SerializeField] private HandView handView;

    [Header("Key / Chest")]
    [SerializeField] private GameObject keyObject;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private Transform chestSpawnPoint;
    [SerializeField] private ChestChoiceUI chestChoiceUI;

    [Header("Chest Cinematic")]
    [SerializeField] private float chestDropHeight = 2.5f;
    [SerializeField] private float chestDropDuration = 0.6f;
    [SerializeField] private float chestBounceDuration = 0.15f;
    [SerializeField] private float chestBounceHeight = 0.18f;

    [SerializeField] private Transform cameraShakeTarget;
    [SerializeField] private float cameraShakeDuration = 0.25f;
    [SerializeField] private float cameraShakeStrength = 0.2f;

    [SerializeField] private ParticleSystem chestSpawnVFX;

    [Header("Auto Open UI")]
    [SerializeField] private float autoOpenChestUIDelay = 3f;

    [Header("Weapon")]
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private Transform weaponSpawnPoint;

    [Header("Scenes")]
    [SerializeField] private string nextScene = "SalonPrincipal";

    [Header("Blackout (post-ojo)")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip bulbBreakClip;
    [SerializeField] private CanvasGroup blackoutGroup;
    [SerializeField] private float fadeToBlackTime = 0.35f;
    [SerializeField] private float loadSceneDelay = 3f;

    private bool rewardGiven = false;
    private bool chestSpawned = false;
    private bool chestUIOpened = false;
    private Coroutine autoOpenRoutine;

    private GameObject chestInstance;
    private GameObject spawnedWeaponInstance;

    private bool blackoutStarted = false;

    private void Start()
    {
        if (health != null)
            health.OnHpChanged += OnHpChanged;

        chestChoiceUI?.Hide();

        if (keyObject != null) keyObject.SetActive(false);

        handView?.SetInputLocked(false);

        if (blackoutGroup != null)
        {
            blackoutGroup.alpha = 0f;
            blackoutGroup.blocksRaycasts = false;
            blackoutGroup.interactable = false;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnHpChanged -= OnHpChanged;
    }

    private void OnHpChanged(int playerHP, int enemyHP)
    {
        if (rewardGiven) return;

        if (enemyHP <= 0)
        {
            rewardGiven = true;
            StartWinSequence();
        }
    }

    private void StartWinSequence()
    {
        handView?.SetInputLocked(true);
        handView?.ClearAllCards();
        ShowKey();
    }

    private void ShowKey()
    {
        if (keyObject == null) return;

        SFXManager.I?.PlayKeyReveal();

        keyObject.SetActive(true);

        var sr = keyObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
            sr.DOFade(1f, 0.35f);
        }

        keyObject.transform.DOKill();
        keyObject.transform.localScale = Vector3.one * 0.9f;
        keyObject.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }

    public void OnKeyClicked(Vector3 keyWorldPos)
    {
        if (chestSpawned) return;
        chestSpawned = true;

        if (keyObject != null)
        {
            var sr = keyObject.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.DOFade(0f, 0.35f).OnComplete(() => keyObject.SetActive(false));
            else
                keyObject.SetActive(false);
        }

        Vector3 spawnPos = (chestSpawnPoint != null) ? chestSpawnPoint.position : keyWorldPos;
        Quaternion spawnRot = (chestSpawnPoint != null) ? chestSpawnPoint.rotation : Quaternion.identity;

        SpawnChestCinematic(spawnPos, spawnRot);
    }

    private void SpawnChestCinematic(Vector3 finalPos, Quaternion finalRot)
    {
        if (chestPrefab == null) return;

        Vector3 startPos = finalPos + Vector3.up * chestDropHeight;
        chestInstance = Instantiate(chestPrefab, startPos, finalRot);

        Sequence seq = DOTween.Sequence();

        float delay = 0.12f;
        float impactTime = delay + chestDropDuration;

        seq.AppendInterval(delay);

        // Caída física
        seq.Append(
            chestInstance.transform
                .DOMove(finalPos, chestDropDuration)
                .SetEase(Ease.InQuad)
        );

        // 🔥 IMPACTO EXACTO
        seq.InsertCallback(impactTime, () =>
        {
            if (chestSpawnVFX != null)
            {
                chestSpawnVFX.transform.position = finalPos;
                chestSpawnVFX.Play();
            }

            SFXManager.I?.PlayChestSpawn();
        });

        // Bounce arriba
        seq.Append(
            chestInstance.transform
                .DOMoveY(finalPos.y + chestBounceHeight, chestBounceDuration)
                .SetEase(Ease.OutSine)
        );

        // Bounce abajo
        seq.Append(
            chestInstance.transform
                .DOMoveY(finalPos.y, chestBounceDuration)
                .SetEase(Ease.InSine)
        );

        seq.Join(
            chestInstance.transform
                .DOPunchScale(new Vector3(0.12f, 0.12f, 0.12f), 0.25f, 8, 0.7f)
        );

        if (cameraShakeTarget != null)
        {
            seq.Join(
                cameraShakeTarget.DOPunchPosition(
                    new Vector3(cameraShakeStrength, cameraShakeStrength, 0f),
                    cameraShakeDuration, 12, 0.6f
                )
            );
        }

        if (autoOpenRoutine != null) StopCoroutine(autoOpenRoutine);
        autoOpenRoutine = StartCoroutine(AutoOpenChestUIAfterDelay());
    }

    public void OpenChestUI()
    {
        if (chestUIOpened) return;
        chestUIOpened = true;
        chestChoiceUI?.Show(this);
    }

    public void OnChooseWeapon()
    {
        HideChest();

        if (weaponPrefab != null && weaponSpawnPoint != null)
        {
            spawnedWeaponInstance = Instantiate(
                weaponPrefab,
                weaponSpawnPoint.position,
                weaponSpawnPoint.rotation
            );

            SFXManager.I?.PlayWeaponSpawn();
        }
    }
    
    // RoundFlowController.OnChooseToken()
    public void OnChooseToken()
    {
        Debug.Log($"[RoundFlow] OnChooseToken - GP null? {GameProgress.Instance == null}");
        if (GameProgress.Instance != null)
        Debug.Log($"[RoundFlow] Tokens antes: {GameProgress.Instance.Tokens}");
            
        
        HideChest();
        
        GameProgress.Instance?.GainToken(1);
        
        if (GameProgress.Instance != null)
        Debug.Log($"[RoundFlow] Tokens despues: {GameProgress.Instance.Tokens}");
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}

    private void HideChest()
    {
        if (chestInstance == null) return;

        chestInstance.transform.DOKill();

        var sr = chestInstance.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.DOFade(0f, 0.25f).OnComplete(() =>
            {
                Destroy(chestInstance);
                chestInstance = null;
            });
            return;
        }

        chestInstance.transform
            .DOScale(0f, 0.25f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                Destroy(chestInstance);
                chestInstance = null;
            });
    }

    private System.Collections.IEnumerator AutoOpenChestUIAfterDelay()
    {
        yield return new WaitForSecondsRealtime(autoOpenChestUIDelay);
        OpenChestUI();
    }

    public void StartBlackoutAndLoadNextScene()
    {
        if (blackoutStarted) return;
        blackoutStarted = true;

        if (musicSource != null)
            musicSource.DOFade(0f, 0.35f).OnComplete(() => musicSource.Stop());

        if (sfxSource != null && bulbBreakClip != null)
            sfxSource.PlayOneShot(bulbBreakClip);

        if (spawnedWeaponInstance != null)
        {
            Destroy(spawnedWeaponInstance);
            spawnedWeaponInstance = null;
        }

        if (blackoutGroup != null)
        {
            blackoutGroup.blocksRaycasts = true;
            blackoutGroup.interactable = true;
            blackoutGroup.DOFade(1f, fadeToBlackTime).SetEase(Ease.Linear);
        }

        DOVirtual.DelayedCall(loadSceneDelay, () =>
        {
            SceneManager.LoadScene(nextScene);
        });
    }
}