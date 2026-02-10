using UnityEngine;
using UnityEngine.InputSystem;

public class MouseObjectRotate : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference clickAction; // Button
    [SerializeField] private InputActionReference dragAction;  // Vector2 (Mouse Delta)

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;

    [Header("Auto Reset")]
    [SerializeField] private float resetDelay = 3f;      // Time without input before reset
    [SerializeField] private float resetSpeed = 2f;      // How fast it returns

    private Camera cam;
    private bool isDragging = false;
    private bool isSelected = false;

    private Quaternion initialRotation;
    private float idleTimer = 0f;

    private void Awake()
    {
        cam = Camera.main;
        initialRotation = transform.rotation;
    }

    private void OnEnable()
    {
        clickAction.action.Enable();
        dragAction.action.Enable();

        clickAction.action.started += OnClickStarted;
        clickAction.action.canceled += OnClickCanceled;
    }

    private void OnDisable()
    {
        clickAction.action.started -= OnClickStarted;
        clickAction.action.canceled -= OnClickCanceled;

        clickAction.action.Disable();
        dragAction.action.Disable();
    }

    private void Update()
    {
        HandleRotation();
        HandleAutoReset();
    }

    private void OnClickStarted(InputAction.CallbackContext ctx)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
        {
            isDragging = true;
            isSelected = true;
            idleTimer = 0f;
        }
    }

    private void OnClickCanceled(InputAction.CallbackContext ctx)
    {
        isDragging = false;
    }

    private void HandleRotation()
    {
        if (!isDragging || !isSelected)
            return;

        Vector2 dragDelta = dragAction.action.ReadValue<Vector2>();

        if (Mathf.Abs(dragDelta.x) < 0.001f)
            return;

        idleTimer = 0f;

        // FIXED: Correct horizontal direction
        float rotY = -dragDelta.x * rotationSpeed;

        transform.Rotate(Vector3.up, rotY, Space.World);
    }

    private void HandleAutoReset()
    {
        if (isDragging || !isSelected)
            return;

        idleTimer += Time.deltaTime;

        if (idleTimer >= resetDelay)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                initialRotation,
                resetSpeed * Time.deltaTime
            );

            // Stop tiny micro-rotations when close enough
            if (Quaternion.Angle(transform.rotation, initialRotation) < 0.1f)
            {
                transform.rotation = initialRotation;
                isSelected = false; // Fully reset, no longer "active"
            }
        }
    }
}
