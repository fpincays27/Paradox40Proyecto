using System.Collections;
using UnityEngine;

public class ChestToken : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private int tokensToGive = 1;
    [SerializeField] private TrapdoorToScene trapdoor;

    [Header("Disappear Animation")]
    [SerializeField] private float disappearDuration = 0.8f;
    [SerializeField] private float sinkDistance = 0.15f;
    [SerializeField] private AnimationCurve disappearCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Optional")]
    [SerializeField] private Collider clickableCollider;
    [SerializeField] private bool disableObjectAtEnd = true;
    [SerializeField] private bool verboseLogs = true;

    private bool used;
    private bool isDisappearing;

    private Vector3 startScale;
    private Vector3 startPosition;
    private Collider[] allColliders;

    private void Awake()
    {
        if (clickableCollider == null)
            clickableCollider = GetComponent<Collider>();

        startScale = transform.localScale;
        startPosition = transform.position;
        allColliders = GetComponentsInChildren<Collider>(true);
    }

    private void OnMouseDown()
    {
        if (used || isDisappearing) return;

        used = true;

        if (verboseLogs)
            Debug.Log($"[{name}] Cofre clickeado.", this);

        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.GainToken(tokensToGive);

            if (verboseLogs)
                Debug.Log($"[{name}] Token entregado: +{tokensToGive}", this);
        }
        else
        {
            Debug.LogWarning($"[{name}] No se encontró GameProgress.Instance.", this);
        }

        if (trapdoor != null)
        {
            trapdoor.UnlockTrapdoor();

            if (verboseLogs)
                Debug.Log($"[{name}] Trampilla desbloqueada.", this);
        }
        else
        {
            Debug.LogWarning($"[{name}] No hay Trapdoor asignada.", this);
        }

        StartCoroutine(DisappearRoutine());
    }

    private IEnumerator DisappearRoutine()
    {
        isDisappearing = true;

        for (int i = 0; i < allColliders.Length; i++)
        {
            if (allColliders[i] != null)
                allColliders[i].enabled = false;
        }

        float elapsed = 0f;
        Vector3 endScale = startScale * 0.15f;
        Vector3 endPosition = startPosition + Vector3.down * sinkDistance;

        while (elapsed < disappearDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / disappearDuration);
            float curvedT = disappearCurve.Evaluate(t);

            transform.localScale = Vector3.Lerp(startScale, endScale, curvedT);
            transform.position = Vector3.Lerp(startPosition, endPosition, curvedT);

            yield return null;
        }

        transform.localScale = endScale;
        transform.position = endPosition;

        if (disableObjectAtEnd)
            gameObject.SetActive(false);

        isDisappearing = false;
    }
}