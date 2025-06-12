using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 100f;
    public float maxVerticalAngle = 80f;

    private float verticalRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Vertical rotation (camera)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // Horizontal rotation (player body)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}