using UnityEngine;

public class EyesFollow : MonoBehaviour
{
    [SerializeField] private float maxOffset = 0.12f;
    [SerializeField] private float smooth = 10f;
    [SerializeField] private Camera cam;

    private Vector3 startLocalPos;
    private Vector3 currentOffset;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (cam == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = mouseWorld - transform.position;
        dir.z = 0f;

        Vector3 targetOffset = Vector3.zero;

        if (dir.magnitude > 0.001f)
            targetOffset = dir.normalized * maxOffset;

        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * smooth);

        transform.localPosition = startLocalPos + currentOffset;
    }
}
