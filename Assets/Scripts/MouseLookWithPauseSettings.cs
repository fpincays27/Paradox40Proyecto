using UnityEngine;

public class MouseLookWithPauseSettings : MonoBehaviour
{
    [SerializeField] private Transform playerBody;
    [SerializeField] private float defaultSensitivity = 2f;

    private float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (PauseMenuPersistent.IsPaused) return;

        float sens = PauseMenuPersistent.Instance != null
            ? PauseMenuPersistent.MouseSensitivity
            : defaultSensitivity;

        float mouseX = Input.GetAxis("Mouse X") * sens;
        float mouseY = Input.GetAxis("Mouse Y") * sens;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }
}