using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody; // e.g., the "Player" GameObject
    public float mouseSensitivity = 100f;
    public float maxVerticalAngle = 80f;

    private float verticalRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Clamp vertical camera rotation
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);

        // Only rotate camera (pitch) on X axis
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // Rotate player body (yaw) on Y axis
        playerBody.Rotate(Vector3.up * mouseX);
    }
}