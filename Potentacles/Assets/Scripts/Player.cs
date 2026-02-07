using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float drag = 5f; // How quickly the player slows down

    private Vector3 velocity;
    private Vector3 acceleration;
    private Vector2 moveInput;
    private PlayerInput playerInput;

    private Rigidbody rb;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        rb = GetComponent<Rigidbody>();

        // Set up Rigidbody for collision detection
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.freezeRotation = true; // Prevent tipping over
        rb.useGravity = true;
    }

    void Update()
    {
        // Calculate acceleration based on input
        acceleration = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;

        // Apply acceleration to velocity
        velocity += acceleration * Time.deltaTime;

        // Apply drag when there's no input
        if (moveInput.magnitude == 0)
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, drag * Time.deltaTime);
        }

        // Clamp velocity to max speed
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        // Apply velocity to position
        transform.position += velocity * Time.deltaTime;

        // Optional: Rotate character to face movement direction
        if (velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        // Store the input value to use in Update
        moveInput = ctx.ReadValue<Vector2>();
    }
}
