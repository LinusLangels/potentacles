using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float targetSpeed = 10f;
    [SerializeField] private float acceleration = 2f;

    [Header("Balance Settings")]
    [SerializeField] private float pendulumLength = 2f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float damping = 0.1f;
    [SerializeField] private float playerCorrectionStrength = 5f;
    [SerializeField] private float bounceAbsorption = 0.7f; // How much velocity is lost at bounds (0-1)

    private float currentSpeed;
    private Vector2 moveInput;
    private PlayerInput playerInput;
    private Rigidbody rb;

    // Balance simulation variables
    private float balanceAngle = 0f; // Current tilt angle (radians) - 0 = hanging straight down
    private float balanceVelocity = 0f; // Angular velocity

    private const float MAX_ANGLE = Mathf.PI / 2f; // 90 degrees in radians

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.freezeRotation = true;
        rb.useGravity = true;

        currentSpeed = 0f;

        // Start at rest (hanging straight down)
        balanceAngle = 0f;
        balanceVelocity = 0f;
    }

    void Update()
    {
        // Automatically accelerate forward
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        // Simulate pendulum balance physics
        SimulateBalance();

        // Calculate sideways acceleration from tilt
        float sidewaysAcceleration = CalculateTiltForce();

        // Forward movement (automatic)
        Vector3 forwardMovement = Vector3.forward * currentSpeed;

        // Side movement (entirely driven by balance tilt)
        Vector3 sideMovement = Vector3.right * sidewaysAcceleration;

        Vector3 totalMovement = forwardMovement + sideMovement;

        // Apply movement
        transform.position += totalMovement * Time.deltaTime;
    }

    void SimulateBalance()
    {
        // Player input disturbs the pendulum
        // When you press left/right, you push the pendulum that direction
        float playerForce = moveInput.x * playerCorrectionStrength;

        // Normal pendulum physics: gravity pulls it back to center (hanging down)
        // This creates a RESTORING force (stable equilibrium)
        float restoringTorque = -(gravity / pendulumLength) * Mathf.Sin(balanceAngle);

        // Player input adds force to swing it
        float angularAcceleration = restoringTorque + playerForce;

        // Apply damping
        angularAcceleration -= damping * balanceVelocity;

        // Update velocity and angle
        balanceVelocity += angularAcceleration * Time.deltaTime;
        balanceAngle += balanceVelocity * Time.deltaTime;

        // Clamp angle to ±90 degrees and handle bounce
        if (balanceAngle > MAX_ANGLE)
        {
            balanceAngle = MAX_ANGLE;
            // Absorb/reverse velocity at the bound
            balanceVelocity = -balanceVelocity * (1f - bounceAbsorption);
        }
        else if (balanceAngle < -MAX_ANGLE)
        {
            balanceAngle = -MAX_ANGLE;
            // Absorb/reverse velocity at the bound
            balanceVelocity = -balanceVelocity * (1f - bounceAbsorption);
        }
    }

    float CalculateTiltForce()
    {
        // The pendulum angle translates to sideways acceleration
        float sidewaysForce = Mathf.Sin(balanceAngle) * gravity;

        return sidewaysForce;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw the pendulum (hanging DOWN from pivot)
        Vector3 pivot = transform.position + Vector3.up * 2f;
        Vector3 mass_pos = pivot - Vector3.up * pendulumLength * Mathf.Cos(balanceAngle)
                                  + Vector3.right * pendulumLength * Mathf.Sin(balanceAngle);

        // Draw the rod
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pivot, mass_pos);

        // Draw the mass
        Gizmos.DrawSphere(mass_pos, 0.5f);

        // Draw vertical reference line (rest position)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pivot, pivot - Vector3.up * pendulumLength);

        // Draw ±90 degree bounds
        Gizmos.color = Color.red;
        Vector3 leftBound = pivot + Vector3.right * -pendulumLength;
        Vector3 rightBound = pivot + Vector3.right * pendulumLength;
        Gizmos.DrawLine(pivot, leftBound);
        Gizmos.DrawLine(pivot, rightBound);

        // Draw arc showing range
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Vector3 prevPoint = leftBound;
        for (int i = 1; i <= 20; i++)
        {
            float t = i / 20f;
            float angle = Mathf.Lerp(-MAX_ANGLE, MAX_ANGLE, t);
            Vector3 point = pivot - Vector3.up * pendulumLength * Mathf.Cos(angle)
                                  + Vector3.right * pendulumLength * Mathf.Sin(angle);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}