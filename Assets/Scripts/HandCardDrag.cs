using UnityEngine;
using DG.Tweening;

public class HandCardDrag : MonoBehaviour
{
    private HandView hand;
    private Camera cam;
    private bool dragging;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private int originalOrder;

    [SerializeField] private int dragOrder = 100;

    public void Init(HandView hand)
    {
        this.hand = hand;
        cam = Camera.main;
    }

    private void OnMouseDown()
    {
        if (hand == null) return;

        dragging = true;
        originalPos = transform.position;
        originalRot = transform.rotation;

        // ✅ SFX: iniciar drag (solo una vez)
        SFXManager.I?.PlayDragCard();

        var sg = GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sg != null)
        {
            originalOrder = sg.sortingOrder;
            sg.sortingOrder = dragOrder;
        }
        else
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                originalOrder = sr.sortingOrder;
                sr.sortingOrder = dragOrder;
            }
        }
    }

    private void OnMouseDrag()
    {
        if (!dragging || cam == null) return;

        Vector3 m = Input.mousePosition;
        m.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(m);
        world.z = transform.position.z;

        transform.position = world;
    }

    private void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;

        bool consumed = hand.TryDropHandCardOnFaceDown(GetComponent<CardView>(), originalPos, originalRot);

        if (!consumed)
        {
            transform.DOMove(originalPos, 0.2f);
            transform.DORotateQuaternion(originalRot, 0.2f);
        }

        var sg = GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sg != null) sg.sortingOrder = originalOrder;
        else
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = originalOrder;
        }
    }
}