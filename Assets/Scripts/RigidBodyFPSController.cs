using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyFPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float airControlMultiplier = 0.5f;

    [Header("Look Settings")]
    public float sensitivity = 2f;
    public float maxLookX = 85f;
    public float minLookX = -85f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.3f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private Camera cam;
    private float rotX;
    private bool isGrounded;
    private Vector3 _lastPos;
    private float _horizontalSpeed; // magnitud horizontal estimada

    [Header("Debug (Controller)")]
    public bool debugMovementState = false;

    // Expuestos para otros componentes (lectura)
    public bool IsGrounded => isGrounded;
    public float HorizontalSpeed => _horizontalSpeed;
    public bool IsMoving(float minSpeed = 0.1f) => _horizontalSpeed >= minSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        _lastPos = transform.position;
    }

    void Update()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused) return; // Do nothing if paused

        Look();
        CheckGround();
        UpdateHorizontalSpeed();
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused) return;

        Move();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Desired direction relative to camera
        Vector3 dir = transform.right * x + transform.forward * z;
        dir.Normalize();

        // Apply different force if in air
        float control = isGrounded ? 1f : airControlMultiplier;

        // Maintain existing vertical velocity
        Vector3 targetVelocity = dir * moveSpeed * control;
        Vector3 velocityChange = targetVelocity - new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void Look()
    {
        float y = Input.GetAxis("Mouse X") * sensitivity;
        rotX += -Input.GetAxis("Mouse Y") * sensitivity;

        rotX = Mathf.Clamp(rotX, minLookX, maxLookX);

        // Rotate camera vertically
        cam.transform.localRotation = Quaternion.Euler(rotX, 0, 0);
        // Rotate player horizontally
        transform.Rotate(Vector3.up * y);
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);
    }

    void UpdateHorizontalSpeed()
    {
        Vector3 pos = transform.position;
        Vector3 delta = pos - _lastPos; delta.y = 0f;
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        // Clamp para evitar picos raros (ej: recovery focus frames)
        if (dt > 0.05f) dt = 0.05f;
        _horizontalSpeed = delta.magnitude / dt;
        _lastPos = pos;
        if (debugMovementState && Time.frameCount % 20 == 0)
            Debug.Log($"[RigidbodyFPSController] grounded={isGrounded} speed={_horizontalSpeed:F3}");
    }
}
