using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Drone : MonoBehaviour
{
    [SerializeField] private TutorialEventChannel tutorialEventChannel;
    [SerializeField] private Transform playerTransform;
    private Rigidbody rb;

    [Header("Position Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -3);

    [Header("Movement Settings")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private float wallDistance = 1.5f; // Distance to keep from walls

    // For a smooth, floating effect, set the Rigidbody's 'Drag' to ~2 and 'Angular Drag' to ~1 in the Inspector.

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Ensure the Rigidbody does not use gravity to allow for free floating.
        rb.useGravity = false;
        tutorialEventChannel.RaiseEvent(ETutorialEvent.AimToDrone_S);
    }

    public void Update()
    {
        // Always look at the player
        transform.LookAt(playerTransform);
    }

    public void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 1. Define the ideal target position based on the player's orientation and offset.
        Vector3 idealPosition = playerTransform.position + offset;
        
        // 2. Check for obstacles. Cast a ray from the player towards the drone's ideal position.
        Vector3 directionFromPlayer = idealPosition - playerTransform.position;
        Ray ray = new Ray(playerTransform.position, directionFromPlayer.normalized);
        
        Vector3 targetPosition = idealPosition;

        // If the ray hits an obstacle within the desired offset distance...
        if (Physics.Raycast(ray, out RaycastHit hit, directionFromPlayer.magnitude, obstacleLayerMask))
        {
            // ...set the new target position to be slightly in front of the obstacle.
            targetPosition = hit.point - directionFromPlayer.normalized * wallDistance;
        }

        Vector3 dir = (targetPosition-transform.position).normalized;

        // Idle 상태
        if(Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            return;
        }

        rb.MovePosition(transform.position + dir * followSpeed * Time.deltaTime);
    }
}
