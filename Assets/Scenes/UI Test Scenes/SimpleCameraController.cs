using UnityEngine;

public class SimpleFlyingFPSController : MonoBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float verticalSpeed = 8f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private float pitch = 0f;
    private float yaw = 0f;

    void Start() {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // If no camera assigned, try to find one
        if (cameraTransform == null) {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) {
                cameraTransform = cam.transform;
            }
        }

        // Initialize yaw to current rotation
        yaw = transform.eulerAngles.y;
    }

    void Update() {
        HandleMouseLook();
        HandleMovement();
        HandleCursorToggle();
    }

    void HandleMovement() {
        // Get input
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down

        // Calculate move direction relative to where we're looking
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        // Vertical movement (Q/E or Space/Ctrl)
        float verticalMovement = 0f;
        if (Input.GetKey(KeyCode.Space)) verticalMovement = 1f;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C)) verticalMovement = -1f;

        moveDirection += Vector3.up * verticalMovement;

        // Apply sprint
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) {
            currentSpeed *= sprintMultiplier;
        }

        // Move the player
        transform.position += moveDirection.normalized * currentSpeed * Time.deltaTime;
    }

    void HandleMouseLook() {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Update yaw (left/right) and pitch (up/down)
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        // Apply rotation to player body (yaw only)
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Apply rotation to camera (pitch only, relative to player)
        if (cameraTransform != null) {
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void HandleCursorToggle() {
        // Press Escape to unlock cursor
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (Cursor.lockState == CursorLockMode.Locked) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}