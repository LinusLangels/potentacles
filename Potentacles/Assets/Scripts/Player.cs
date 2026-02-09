using NUnit.Framework.Interfaces;
using System;
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
    [SerializeField] private float tiltInfluence = 1f; // How much the tilt affects sideways movement

    [Header("Gravity Settings")]
    [SerializeField] private float gravityValue = -9.81f;

    private float currentSpeed;
    private Vector2 moveInput;
    private PlayerInput playerInput;
    private CharacterController characterController;

    // Balance simulation variables
    private float balanceAngle = 0f; // Current tilt angle (radians) - 0 = hanging straight down
    private float balanceVelocity = 0f; // Angular velocity

    private const float MAX_ANGLE = Mathf.PI / 2f; // 90 degrees in radians

    // Velocity tracking for smooth movement
    private Vector3 velocity;
    private float sidewaysVelocity = 0f;

    public DrunkState balanceState;
    public WalletState walletState;

    public float VisitCooldown = 0f;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        balanceState = GetComponent<DrunkState>();
        walletState = GetComponent<WalletState>();
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }

        currentSpeed = 0f;
        velocity = Vector3.zero;
        sidewaysVelocity = 0f;

        // Start at rest (hanging straight down)
        balanceAngle = 0f;
        balanceVelocity = 0f;
    }

    void Update()
    {
        if (GameStateManager.Instance.CurrentState == GameState.Walking)
        {
            // Automatically accelerate forward
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            // Simulate pendulum balance physics
            SimulateBalance();

            // Calculate sideways acceleration from tilt
            float sidewaysAcceleration = CalculateTiltForce();

            //// Update sideways velocity based on tilt
            //sidewaysVelocity += sidewaysAcceleration;

            //// Apply some damping to sideways movement for smoothness
            //sidewaysVelocity *= 0.95f;

            // Forward movement (automatic)
            Vector3 forwardMovement = Vector3.forward * currentSpeed;

            // Side movement (entirely driven by balance tilt)
            Vector3 sideMovement = Vector3.right * sidewaysAcceleration;

            // Combine movements
            Vector3 horizontalMovement = forwardMovement + sideMovement;

            // Apply gravity
            if (characterController.isGrounded)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
            else
            {
                velocity.y += gravityValue * Time.deltaTime;
            }

            // Combine horizontal and vertical movement
            Vector3 totalMovement = horizontalMovement + Vector3.up * velocity.y;

            // Move the character controller
            characterController.Move(totalMovement * Time.deltaTime);
        }

        VisitCooldown = Mathf.Max(0f, VisitCooldown - Time.deltaTime);
    }

    void SimulateBalance()
    {
        // Get modifiers from BalanceState
        float playerControlMultiplier = balanceState != null ? balanceState.GetPlayerControlMultiplier() : 1f;
        float pendulumForceMultiplier = balanceState != null ? balanceState.GetPendulumForceMultiplier() : 1f;

        // Player input disturbs the pendulum (reduced by imbalance)
        float playerForce = moveInput.x * playerCorrectionStrength * playerControlMultiplier;

        // Normal pendulum physics: gravity pulls it back to center (hanging down)
        // This creates a RESTORING force (stable equilibrium)
        float restoringTorque = -(gravity / pendulumLength) * Mathf.Sin(balanceAngle);

        // Apply extra force when pendulum is in motion (increased by imbalance)
        float velocityBoost = Mathf.Abs(balanceVelocity) * (pendulumForceMultiplier - 1f);
        restoringTorque *= (1f + velocityBoost);

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

    internal void AddForce(float force)
    {
        balanceVelocity = force;
    }

    public float GetBalanceAngle()
    {
        return balanceAngle;
    }

    internal void OnATMPassed()
    {
    }

    public bool IsVisitCooldown()
    {
        return VisitCooldown > 0f;
    }

    internal void Visit()
    {
        VisitCooldown = 2f;
    }
}