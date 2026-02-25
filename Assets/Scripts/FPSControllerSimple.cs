using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSControllerSimple : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float gravity = -20f;
    public float jumpHeight = 1.2f;

    [Header("Mouse Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    CharacterController cc;
    Vector3 velocity;
    float xRotation = 0f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>().transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
        Move();
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        // Movimiento WASD
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        cc.Move(move * speed * Time.deltaTime);

        // Gravedad
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // Salto
        if (cc.isGrounded && Input.GetButtonDown("Jump"))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}