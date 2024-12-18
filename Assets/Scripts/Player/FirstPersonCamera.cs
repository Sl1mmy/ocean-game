using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f; // Sensitivity of the mouse movement
    public Transform playerBody; // Reference to the player's body for rotation
    public float moveSpeed = 5f; // Speed at which the player moves

    private float xRotation = 0f; // To store the x-axis (vertical) rotation of the camera

    private void Start()
    {
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse input for camera rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the camera vertically (up/down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent flipping over
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player body horizontally (left/right)
        playerBody.Rotate(Vector3.up * mouseX);

        // Get input for movement
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime; // Left/Right
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;   // Forward/Backward

        // Move in the direction the camera is facing
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        playerBody.position += move; // Apply the movement to the player body
    }
}
