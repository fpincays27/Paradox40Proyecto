using UnityEngine;
using DG.Tweening;

public class FaceDownCard : MonoBehaviour
{
    private HandView hand;
    private Sprite frontSprite;
    private Sprite backSprite;
    private bool isFaceDown = true;

    private void OnMouseDown()
    {
        if (hand == null) return;
        hand.TryFlip(this);    
    }

    public void Init(HandView hand, Sprite front, Sprite back)
    {
        this.hand = hand;
        frontSprite = front;
        backSprite = back;

        GetComponent<CardView>()?.SetSprite(backSprite);
    }

    public bool IsFaceDown => isFaceDown;
    public Sprite FrontSprite => frontSprite;

    public void Flip()
    {
        if (!isFaceDown) return;

        transform.DOScaleX(0f, 0.12f).OnComplete(() =>
        {
            GetComponent<CardView>()?.SetSprite(frontSprite);
            transform.DOScaleX(1f, 0.12f);
        });

        isFaceDown = false;
    }
}