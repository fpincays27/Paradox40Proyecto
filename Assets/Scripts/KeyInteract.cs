using UnityEngine;
using DG.Tweening;

public class KeyInteract : MonoBehaviour
{
    [SerializeField] private RoundFlowController flow;
    [SerializeField] private float fadeTime = 0.5f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        if (flow == null) return;

        // Fade out y avisar al flow
        if (sr != null)
        {
            sr.DOFade(0f, fadeTime).OnComplete(() =>
            {
                gameObject.SetActive(false);
                flow.OnKeyClicked(transform.position);
            });
        }
        else
        {
            gameObject.SetActive(false);
            flow.OnKeyClicked(transform.position);
        }
    }
}