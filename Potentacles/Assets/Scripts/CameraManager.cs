using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("References")]
    public Player player;

    [Header("Camera Position")]
    [SerializeField] private Vector3 offset = new Vector3(1.5f, 2f, -3f); // Right, Up, Back
    [SerializeField] private float followSpeed = 10f;

    [Header("Camera Rotation")]
    [SerializeField] private float lookAheadDistance = 2f; // How far ahead to look
    [SerializeField] private float rotationSpeed = 5f;

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (player == null) return;

        if (GameStateManager.Instance.CurrentState == GameState.Walking)
        {

            // Calculate target position with offset, but lock to world Z-forward
            Vector3 targetPosition = player.transform.position + offset;

            // Smoothly move camera to target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition,
                                                    ref currentVelocity, 1f / followSpeed);

            // Lock camera rotation to look forward along world Z-axis
            transform.rotation = Quaternion.identity; // Or Quaternion.Euler(0, 0, 0)
        }

    }
}