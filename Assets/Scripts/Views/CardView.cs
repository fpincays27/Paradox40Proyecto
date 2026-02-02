using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer artwork;

    public Sprite CurrentSprite => artwork != null ? artwork.sprite : null;

    public void SetSprite(Sprite sprite)
    {
        if (artwork != null) artwork.sprite = sprite;
    }
}