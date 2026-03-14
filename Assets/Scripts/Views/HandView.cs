using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using UnityEngine.InputSystem;
using TMPro;

public class HandView : MonoBehaviour
{
    [Header("Hand")]
    [SerializeField] private int maxHandSize = 10;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private List<Sprite> cardSprites;

    [Header("Face Down (front of hand)")]
    [SerializeField] private Sprite cardBackSprite;
    [SerializeField] private float tableOffsetY = 1.6f;
    [SerializeField] private float tableSpacing = 0.8f;
    [SerializeField] private float zVisible = -5f;
    [SerializeField] private float tableOffsetX = 0f;

    [Header("UI Counter")]
    [SerializeField] private TextMeshProUGUI flipCounterText;
    [SerializeField] private TextMeshProUGUI tokenCounterText;
    [SerializeField] private Transform faceDownAnchor;

    [Header("Health")]
    [SerializeField] private HealthManager health;

    [Header("Card Destroy Animation")]
    [SerializeField] private float returnToDeckTime = 0.25f;
    [SerializeField] private float destroyScaleTime = 0.18f;
    [SerializeField] private float destroyFadeTime = 0.18f;

    private readonly List<GameObject> handCards = new();
    private readonly List<FaceDownCard> faceDownCards = new();

    private int drawsCount = 0;
    private int flipTokens = 0;

    private bool noMatchGameEnded = false;
    private bool inputLocked = false;

    private void Start()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnTokensChanged += OnTokensChanged;

        UpdateFlipUI();
    }

    private void OnDestroy()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.OnTokensChanged -= OnTokensChanged;
    }

    private void OnTokensChanged(int currentTokens)
    {
        UpdateTokenUI();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            DrawCard();
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
    }

    private void DrawCard()
    {
        if (inputLocked) return;
        if (health != null && health.EnemyHP <= 0) return;

        if (handCards.Count >= maxHandSize) return;
        if (cardPrefab == null || spawnPoint == null || splineContainer == null) return;
        if (cardSprites == null || cardSprites.Count == 0) return;
        if (cardBackSprite == null) return;

        GameObject g = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);

        SFXManager.I?.PlayDrawCard();

        int index = Random.Range(0, cardSprites.Count);
        Sprite s = cardSprites[index];
        g.GetComponent<CardView>()?.SetSprite(s);

        handCards.Add(g);
        UpdateCardPositions();

        var drag = g.GetComponent<HandCardDrag>();
        if (drag == null) drag = g.AddComponent<HandCardDrag>();
        drag.Init(this);

        drawsCount++;
        if (drawsCount % 3 == 0)
            flipTokens++;

        UpdateFlipUI();
        SpawnFaceDownCard();
        CheckNoMatchLoseCondition();
    }

    private void SpawnFaceDownCard()
    {
        int index = Random.Range(0, cardSprites.Count);
        Sprite hiddenFront = cardSprites[index];

        Vector3 center = splineContainer.Spline.EvaluatePosition(0.5f);
        center.x += tableOffsetX;
        center.y += tableOffsetY;
        center.z = zVisible;

        float totalWidth = faceDownCards.Count * tableSpacing;
        Vector3 start = center - Vector3.right * (totalWidth * 0.5f);
        Vector3 targetPos = start + Vector3.right * (faceDownCards.Count * tableSpacing);

        GameObject downGO = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);
        downGO.transform.rotation = Quaternion.identity;

        FaceDownCard fdc = downGO.GetComponent<FaceDownCard>();
        if (fdc == null) fdc = downGO.AddComponent<FaceDownCard>();

        fdc.Init(this, hiddenFront, cardBackSprite);
        downGO.transform.DOMove(targetPos, 0.25f);

        faceDownCards.Add(fdc);
        LayoutFaceDownRow();
    }

    public void TryFlip(FaceDownCard card)
    {
        if (card == null) return;
        if (!card.IsFaceDown) return;

        if (flipTokens <= 0)
        {
            UpdateFlipUI();
            return;
        }

        flipTokens--;

        SFXManager.I?.PlayCardFlip();

        card.Flip();

        UpdateFlipUI();
        CheckNoMatchLoseCondition();
    }

    private void UpdateFlipUI()
    {
        if (flipCounterText != null)
        {
            int mod = drawsCount % 3;
            int missing = (mod == 0) ? 0 : (3 - mod);

            flipCounterText.text =
                $"FLIPS: {flipTokens}\n" +
                $"Siguiente flip en: {missing} carta(s)";
        }

        UpdateTokenUI();
    }

    private void UpdateTokenUI()
    {
        if (tokenCounterText == null) return;

        int genericTokens = GameProgress.Instance != null ? GameProgress.Instance.Tokens : 0;
        tokenCounterText.text = $"TOKENS: {genericTokens}";
    }

    public void RefreshCountersUI()
    {
        UpdateFlipUI();
    }

    public bool CanShuffleNow()
    {
        if (inputLocked) return false;
        return handCards.Count > 0 || faceDownCards.Count > 0;
    }

    public void ShuffleHandAndTable()
    {
        if (!CanShuffleNow()) return;

        List<GameObject> handSnapshot = new(handCards);
        List<FaceDownCard> tableSnapshot = new(faceDownCards);

        // Devolver sprites al mazo
        for (int i = 0; i < handSnapshot.Count; i++)
        {
            if (handSnapshot[i] == null) continue;
            Sprite s = handSnapshot[i].GetComponent<CardView>()?.CurrentSprite;
            if (s != null) cardSprites.Add(s);
        }

        for (int i = 0; i < tableSnapshot.Count; i++)
        {
            if (tableSnapshot[i] == null) continue;
            Sprite s = tableSnapshot[i].FrontSprite;
            if (s != null) cardSprites.Add(s);
        }

        // Limpiar listas activas primero para bloquear interacción inmediata
        handCards.Clear();
        faceDownCards.Clear();

        // Animar y destruir
        for (int i = 0; i < handSnapshot.Count; i++)
        {
            if (handSnapshot[i] == null) continue;
            AnimateDestroyCard(handSnapshot[i]);
        }

        for (int i = 0; i < tableSnapshot.Count; i++)
        {
            if (tableSnapshot[i] == null) continue;
            AnimateDestroyCard(tableSnapshot[i].gameObject);
        }

        noMatchGameEnded = false;
        SFXManager.I?.PlayShuffle();
        UpdateFlipUI();
    }

    private void UpdateCardPositions()
    {
        if (handCards.Count == 0) return;
        if (splineContainer == null) return;

        float cardSpacing = 0.35f / maxHandSize;
        float firstCardPosition = 0.5f - (handCards.Count - 1) * cardSpacing / 2;

        Spline spline = splineContainer.Spline;

        for (int i = 0; i < handCards.Count; i++)
        {
            if (handCards[i] == null) continue;

            float p = Mathf.Clamp01(firstCardPosition + i * cardSpacing);

            Vector3 splinePosition = spline.EvaluatePosition(p);
            splinePosition.z = zVisible;

            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

            handCards[i].transform.DOMove(splinePosition, 0.25f);
            handCards[i].transform.DOLocalRotateQuaternion(rotation, 0.25f);
        }
    }

    public bool TryDropHandCardOnFaceDown(CardView handCard, Vector3 originalPos, Quaternion originalRot)
    {
        if (handCard == null) return false;

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = new Vector2(world.x, world.y);

        Collider2D[] hits = Physics2D.OverlapPointAll(p);
        FaceDownCard down = null;

        for (int i = 0; i < hits.Length; i++)
        {
            down = hits[i].GetComponentInParent<FaceDownCard>();
            if (down != null) break;
        }

        if (down == null) return false;

        if (down.IsFaceDown)
            down.Flip();

        Sprite a = handCard.CurrentSprite;
        Sprite b = down.FrontSprite;

        Debug.Log($"DROP: hand={a?.name} vs down={b?.name}");

        if (a != null && b != null && a.name == b.name)
        {
            SFXManager.I?.PlayMatch();

            health?.ApplyMatchResult(true);

            ReturnToDeck(handCard.gameObject, a);
            ReturnToDeck(down.gameObject, b);

            handCards.Remove(handCard.gameObject);
            faceDownCards.Remove(down);

            UpdateCardPositions();
            LayoutFaceDownRow();

            CheckNoMatchLoseCondition();
            return true;
        }
        else
        {
            SFXManager.I?.PlayNoMatch();

            health?.ApplyMatchResult(false);

            UpdateCardPositions();
            CheckNoMatchLoseCondition();
            return false;
        }
    }

    private void ReturnToDeck(GameObject cardGO, Sprite returnedSprite)
    {
        if (cardGO == null) return;

        if (returnedSprite != null && cardSprites != null)
            cardSprites.Add(returnedSprite);

        AnimateDestroyCard(cardGO);
    }

    private void AnimateDestroyCard(GameObject cardGO)
    {
        if (cardGO == null) return;

        Transform t = cardGO.transform;
        t.DOKill();

        SpriteRenderer sr = cardGO.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.DOKill();

        float moveTime = Mathf.Max(0.01f, returnToDeckTime);
        float scaleTime = Mathf.Max(0.01f, destroyScaleTime);
        float fadeTime = Mathf.Max(0.01f, destroyFadeTime);

        Vector3 target = spawnPoint != null ? spawnPoint.position : t.position;

        Sequence seq = DOTween.Sequence();
        seq.Join(t.DOMove(target, moveTime).SetEase(Ease.InQuad));
        seq.Join(t.DOScale(0.01f, scaleTime).SetEase(Ease.InBack));

        if (sr != null)
            seq.Join(sr.DOFade(0f, fadeTime).SetEase(Ease.Linear));

        seq.OnComplete(() =>
        {
            if (cardGO != null) Destroy(cardGO);
        });
    }

    private void LayoutFaceDownRow()
    {
        if (faceDownAnchor == null) return;

        Vector3 center = faceDownAnchor.position;
        center.x += tableOffsetX;
        center.y += tableOffsetY;
        center.z = zVisible;

        int n = faceDownCards.Count;

        for (int i = 0; i < n; i++)
        {
            if (faceDownCards[i] == null) continue;

            float x = (i - (n - 1) * 0.5f) * tableSpacing;
            Vector3 target = center + Vector3.right * x;
            faceDownCards[i].transform.DOMove(target, 0.25f);
        }
    }

    private void CheckNoMatchLoseCondition()
    {
        if (noMatchGameEnded) return;
        if (health == null) return;

        if (health.EnemyHP <= 0)
            return;

        if (HasAnyPossibleMatch())
            return;

        if (!AllFaceDownAreRevealed())
            return;

        noMatchGameEnded = true;
        Debug.Log("GAME OVER: No hay matches posibles y todas las cartas están reveladas.");

        health.ForceLose();
    }

    private bool AllFaceDownAreRevealed()
    {
        for (int i = 0; i < faceDownCards.Count; i++)
        {
            if (faceDownCards[i] == null) continue;
            if (faceDownCards[i].IsFaceDown) return false;
        }
        return true;
    }

    private bool HasAnyPossibleMatch()
    {
        if (handCards.Count == 0 || faceDownCards.Count == 0) return false;

        for (int i = 0; i < handCards.Count; i++)
        {
            if (handCards[i] == null) continue;

            CardView hv = handCards[i].GetComponent<CardView>();
            if (hv == null) continue;

            Sprite a = hv.CurrentSprite;
            if (a == null) continue;

            for (int j = 0; j < faceDownCards.Count; j++)
            {
                if (faceDownCards[j] == null) continue;

                Sprite b = faceDownCards[j].FrontSprite;
                if (b == null) continue;

                if (a.name == b.name)
                    return true;
            }
        }
        return false;
    }

    public void ClearAllCards()
    {
        List<GameObject> handSnapshot = new(handCards);
        List<FaceDownCard> tableSnapshot = new(faceDownCards);

        handCards.Clear();
        faceDownCards.Clear();

        for (int i = 0; i < handSnapshot.Count; i++)
        {
            if (handSnapshot[i] == null) continue;
            AnimateDestroyCard(handSnapshot[i]);
        }

        for (int i = 0; i < tableSnapshot.Count; i++)
        {
            if (tableSnapshot[i] == null) continue;
            AnimateDestroyCard(tableSnapshot[i].gameObject);
        }
    }
}