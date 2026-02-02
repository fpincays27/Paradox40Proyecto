using System.Collections.Generic;
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

    [SerializeField] private Transform faceDownAnchor;

    [Header("Health")]
    [SerializeField] private HealthManager health;

    private readonly List<GameObject> handCards = new();
    private readonly List<FaceDownCard> faceDownCards = new();

    private int drawsCount = 0;   // cuántas cartas robaste
    private int flipTokens = 0;   // 1 token por cada 3 robos

    // Para que el GameOver por “sin matches” se dispare una sola vez
    private bool noMatchGameEnded = false;

    private void Start()
    {
        UpdateFlipUI();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            DrawCard();
    }

    private void DrawCard()
    {
        if (handCards.Count >= maxHandSize) return;
        if (cardPrefab == null || spawnPoint == null || splineContainer == null) return;
        if (cardSprites == null || cardSprites.Count == 0) return;
        if (cardBackSprite == null) return;

        // 1) Carta a la mano (boca arriba)
        GameObject g = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);

        int index = Random.Range(0, cardSprites.Count);
        Sprite s = cardSprites[index];
        g.GetComponent<CardView>()?.SetSprite(s);

        handCards.Add(g);
        UpdateCardPositions();

        var drag = g.GetComponent<HandCardDrag>();
        if (drag == null) drag = g.AddComponent<HandCardDrag>();
        drag.Init(this);

        // 2) Contador de robos + token cada 3
        drawsCount++;
        if (drawsCount % 3 == 0)
            flipTokens++;

        UpdateFlipUI();

        // 3) Carta enfrente (boca abajo)
        SpawnFaceDownCard();

        // ✅ Chequeo de derrota: sin matches + todo revelado
        CheckNoMatchLoseCondition();
    }

    private void SpawnFaceDownCard()
    {
        // “Qué carta será cuando se revele”
        int index = Random.Range(0, cardSprites.Count);
        Sprite hiddenFront = cardSprites[index];

        // Centro frente a la mano
        Vector3 center = splineContainer.Spline.EvaluatePosition(0.5f);
        center.x += tableOffsetX;
        center.y += tableOffsetY;
        center.z = zVisible;

        // Fila centrada
        float totalWidth = faceDownCards.Count * tableSpacing;
        Vector3 start = center - Vector3.right * (totalWidth * 0.5f);
        Vector3 targetPos = start + Vector3.right * (faceDownCards.Count * tableSpacing);

        // Instancia en el mazo y vuela
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
        card.Flip();

        UpdateFlipUI();

        // Chequeo de derrota: sin matches + todo revelado
        CheckNoMatchLoseCondition();
    }

    private void UpdateFlipUI()
    {
        if (flipCounterText == null) return;

        int mod = drawsCount % 3;
        int missing = (mod == 0) ? 0 : (3 - mod);

        if (flipTokens > 0)
            flipCounterText.text = $"FLIPS: {flipTokens} \nSiguiente flip en: {missing} carta(s)";
        else
            flipCounterText.text = $"FLIPS: 0 \nSiguiente flip en: {missing} carta(s)";
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

        // Si estaba boca abajo, flípeala (esto NO consume token)
        if (down.IsFaceDown)
            down.Flip();

        Sprite a = handCard.CurrentSprite;
        Sprite b = down.FrontSprite;

        Debug.Log($"DROP: hand={a?.name} vs down={b?.name}");

        // MATCH por NOMBRE (evita el bug de “todo hace match”)
        if (a != null && b != null && a.name == b.name)
        {
            // Daño al enemigo
            health?.ApplyMatchResult(true);

            // Descartar ambas al mazo
            ReturnToDeck(handCard.gameObject, a);
            ReturnToDeck(down.gameObject, b);

            handCards.Remove(handCard.gameObject);
            faceDownCards.Remove(down);

            UpdateCardPositions();
            LayoutFaceDownRow();

            // Chequeo derrota: sin matches + todo revelado
            CheckNoMatchLoseCondition();

            return true; // consumida
        }
        else
        {
            // Daño al jugador
            health?.ApplyMatchResult(false);

            // La carta de la mano vuelve a su lugar (drag lo hará)
            UpdateCardPositions();

            CheckNoMatchLoseCondition();

            return false;
        }
    }

    private void ReturnToDeck(GameObject cardGO, Sprite returnedSprite)
    {
        if (cardGO == null) return;

        // devuelve sprite al deck
        if (returnedSprite != null && cardSprites != null)
            cardSprites.Add(returnedSprite);

        // animación al spawnPoint y destruir
        cardGO.transform.DOMove(spawnPoint.position, 0.25f).OnComplete(() =>
        {
            Destroy(cardGO);
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

        // 1) Si todavía existe algún match posible -> NO pierde
        if (HasAnyPossibleMatch())
            return;

        // 2) Si todavía hay alguna carta boca abajo sin revelar -> NO pierde
        if (!AllFaceDownAreRevealed())
            return;

        // Se cumple: sin matches + todo revelado
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

                // ✅ match por nombre (tu sistema actual)
                if (a.name == b.name)
                    return true;
            }
        }

        return false;
    }
}