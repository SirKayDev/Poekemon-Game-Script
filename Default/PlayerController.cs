using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;

    [Header("Camera Settings")]
    public Transform cameraHolder;
    public float mouseSensitivity = 3.0f;
    public float rotationSmoothTime = 0.12f;
    public Vector3 defaultCameraOffset = new Vector3(0, 5, -10);

    [Header("Zoom Settings")]
    public float zoomInMin = 2f;
    public float zoomOutMax = 10f;
    public float zoomSpeed = 2f;

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    private CharacterController characterController;
    private float pitch = 0f;
    private float yaw = 0f;
    private bool isRotating = false;
    private Vector3 cameraOffset;
    private float currentZoom;

    private Vector3 velocity;
    private bool isGrounded;

    // Add a flag for movement control
    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        yaw = transform.eulerAngles.y;
        cameraOffset = defaultCameraOffset;
        currentZoom = cameraOffset.magnitude;
    }

    void Update()
    {
        HandleCameraRotation();
        HandleZoom();

        cameraHolder.position = transform.position + Quaternion.Euler(0, yaw, 0) * cameraOffset;
        cameraHolder.LookAt(transform.position);

        if (canMove)
        {
            HandleMovement();
        }
    }

    void FixedUpdate()
    {
        isGrounded = characterController.isGrounded;

        // Apply gravity if the player is grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 cameraForward = cameraHolder.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraHolder.right;

        Vector3 moveDirection = cameraForward * moveZ + cameraRight * moveX;
        moveDirection.Normalize();

        velocity.x = moveDirection.x * moveSpeed;
        velocity.z = moveDirection.z * moveSpeed;
    }

    void HandleCameraRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (isRotating)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            yaw += mouseX;
        }
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        currentZoom -= scrollInput * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, zoomInMin, zoomOutMax);

        cameraOffset = cameraOffset.normalized * currentZoom;
    }

    // Method to control whether player can move or not
    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove)
        {
            velocity = Vector3.zero;  // Freeze movement when not allowed
        }
    }
}

