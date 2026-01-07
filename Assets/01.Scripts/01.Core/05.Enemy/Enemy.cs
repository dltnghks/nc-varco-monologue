using UnityEngine;
using System.Collections;

/// <summary>
/// Controls an enemy that patrols between points using Rigidbody physics.
/// Allows for dynamic speed changes and physical interaction.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    // Patrol states
    private enum PatrolState
    {
        MovingToEndpoint, // Moving towards pointA or pointB
        MovingToCenter,   // Moving towards centerPoint
        WaitingAtCenter,  // Waiting at centerPoint
        Stopped
    }

    [Header("Patrol Points")]
    [Tooltip("The first point in the patrol route.")]
    [SerializeField] private Transform pointA;
    [Tooltip("The second point in the patrol route.")]
    [SerializeField] private Transform pointB;
    [Tooltip("The central point where the enemy waits.")]
    [SerializeField] private Transform centerPoint;

    [Header("Movement Settings")]
    [Tooltip("The base movement speed of the enemy in units per second.")]
    [SerializeField] private float moveSpeed = 3.0f;
    [Tooltip("Multiplier applied to base speed for dynamic adjustments (e.g., when aggroed).")]
    [SerializeField] private float multiplierSpeed = 1.0f; // Re-introduced multiplier
    [Tooltip("Time in seconds to wait at the center point.")]
    [SerializeField] private float waitAtCenterTime = 3.0f;
    
    [SerializeField] private float eventMultiplerSpeed = 4.0f; // 
    [Tooltip("How close the enemy needs to be to a point to consider it 'arrived'.")]
    [SerializeField] private float arrivalThreshold = 0.2f;

    [Header("Event Settings")]
    [Tooltip("The event channel for general game events.")]
    [SerializeField] private GameEventChannel gameEventChannel;
    [SerializeField] private EGameEvent OnEnemyGameEvent;
    [SerializeField] private EGameEvent OnEnemyScreamGameEvent;
    [Tooltip("The event channel for enemy-specific commands.")]
    [SerializeField] private EnemyEventChannel enemyEventChannel;

    [Header("Sound Data")]
    [SerializeField] private WwiseAudioData screamAudioData;
    [SerializeField] private WwiseAudioData ambientAudioData;
    
    private Rigidbody rb;
    private PatrolState currentState;
    private Transform currentTarget;
    private Transform nextPatrolEndpoint; // To remember if next is A or B
    private float waitTimer;
    
    private bool isAmbientSoundMuted = false;

    // CurrentSpeed property for dynamic speed calculation
    private float CurrentSpeed => moveSpeed * multiplierSpeed;

    private void OnEnable()
    {
        if (enemyEventChannel != null)
        {
            enemyEventChannel.OnEventRaised += OnEnemyEvent;
        }
        if (gameEventChannel != null)
        {
            gameEventChannel.OnEventRaised += OnPlayerEvent;
        }
    }

    private void OnDisable()
    {
        if (enemyEventChannel != null)
        {
            enemyEventChannel.OnEventRaised -= OnEnemyEvent;
        }
        if (gameEventChannel != null)
        {
            gameEventChannel.OnEventRaised -= OnPlayerEvent;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (pointA == null || pointB == null || centerPoint == null)
        {
            Debug.LogError("[Enemy] Patrol points are not fully assigned. Disabling script.", this);
            enabled = false;
            return;
        }

        if (gameEventChannel == null || enemyEventChannel == null)
        {
            Debug.LogError("[Enemy] Event Channels are not assigned. Disabling script.", this);
            enabled = false;
            return;
        }

        // Configure Rigidbody for 2D-like movement
        rb.useGravity = false;
        rb.freezeRotation = true;

        // Initialize starting position and patrol
        transform.position = pointA.position; // Start at Point A
        nextPatrolEndpoint = pointB; // Next endpoint after first center wait is B
        SetState(PatrolState.MovingToCenter); // Always start by moving to center from A
        
        StartCoroutine(SoundLoop());
    }

    /// <summary>
    /// The main logic loop for movement, handled in FixedUpdate for physics consistency.
    /// </summary>
    private void FixedUpdate()
    {
        // Pause movement if interaction is blocked
        if (GameManager.Instance != null && GameManager.Instance.IsInteractionBlocked)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        switch (currentState)
        {
            case PatrolState.MovingToEndpoint:
            case PatrolState.MovingToCenter:
                MoveTowardsTarget();
                break;

            case PatrolState.WaitingAtCenter:
                WaitAtCenter();
                break;

            case PatrolState.Stopped:
                rb.linearVelocity = Vector3.zero;
                break;
        }
    }

    /// <summary>
    /// Sets a new state and handles entry logic for that state.
    /// </summary>
    private void SetState(PatrolState newState)
    {
        currentState = newState;
        // Debug.Log($"[Enemy] New State: {currentState}");

        switch (currentState)
        {
            case PatrolState.MovingToEndpoint:
                currentTarget = nextPatrolEndpoint;
                isAmbientSoundMuted = false;
                break;
            case PatrolState.MovingToCenter:
                currentTarget = centerPoint;
                isAmbientSoundMuted = false; // Unmute ambient sound after scream
                break;
            case PatrolState.WaitingAtCenter:
                rb.linearVelocity = Vector3.zero;
                waitTimer = waitAtCenterTime;
                // Raise event and play sound upon arrival at center
                gameEventChannel.RaiseEvent(OnEnemyGameEvent);
                if (screamAudioData != null)
                {
                    AudioManager.Instance.PlayOneShot(screamAudioData, transform.position);
                    isAmbientSoundMuted = true; // Mute ambient sound while screaming/waiting
                }
                multiplierSpeed = 1.0f; // Reset multiplier when waiting
                break;
            case PatrolState.Stopped:
                rb.linearVelocity = Vector3.zero;
                currentTarget = null;
                isAmbientSoundMuted = true; // Mute ambient sound when stopped
                break;
        }
    }

    /// <summary>
    /// Moves the Rigidbody towards the current target and checks for arrival.
    /// </summary>
    private void MoveTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = (currentTarget.position - transform.position);
        float distanceToTarget = direction.magnitude;

        if (distanceToTarget <= arrivalThreshold)
        {
            // Arrived at target, decide next state
            if (currentState == PatrolState.MovingToCenter)
            {
                SetState(PatrolState.WaitingAtCenter);
            }
            else if (currentState == PatrolState.MovingToEndpoint)
            {
                // Arrived at an endpoint (A or B), now go to center
                SetState(PatrolState.MovingToCenter);
            }
        }
        else
        {
            // Not arrived yet, continue moving
            rb.linearVelocity = direction.normalized * CurrentSpeed;
            
            // Optional: Rotate to face the direction of movement
            if (rb.linearVelocity.sqrMagnitude > 0.01f) // Only rotate if actually moving
            {
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity.normalized);
            }
        }
    }

    /// <summary>
    /// Handles the waiting timer at the center point.
    /// </summary>
    private void WaitAtCenter()
    {
        waitTimer -= Time.fixedDeltaTime;
        if (waitTimer <= 0)
        {
            // After waiting, switch the next patrol endpoint and move to it
            nextPatrolEndpoint = (nextPatrolEndpoint == pointA) ? pointB : pointA;
            SetState(PatrolState.MovingToEndpoint);
        }
    }
    
    /// <summary>
    /// Handles player-related events.
    /// </summary>
    private void OnPlayerEvent(EGameEvent gameEvent)
    {
        if(gameEvent == EGameEvent.StepOnGlass)
        {
            // Example of how you might change speed and state
            multiplierSpeed *= eventMultiplerSpeed; // Double speed
            Debug.Log($"Enemy aggroed! Current Speed: {CurrentSpeed}");
            SetState(PatrolState.MovingToCenter); // Force enemy to center when aggroed
        }
    }

    /// <summary>
    /// Handles commands sent through the EnemyEventChannel.
    /// </summary>
    private void OnEnemyEvent(EEnemyEvent enemyEvent)
    {
        switch (enemyEvent)
        {
            case EEnemyEvent.Center:
                Debug.Log("[Enemy] Received Center command. Moving to center point.");
                SetState(PatrolState.MovingToCenter);
                break;
            case EEnemyEvent.Scream:
                Debug.Log("[Enemy] Received Scream command. Screaming!");
                gameEventChannel.RaiseEvent(OnEnemyScreamGameEvent);
                break;
        }
    }

    private IEnumerator SoundLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f / multiplierSpeed);
            if (!isAmbientSoundMuted && (GameManager.Instance == null || !GameManager.Instance.IsInteractionBlocked))
            {
                AudioManager.Instance.PlayOneShot(ambientAudioData, transform.position);
            }
        }
    }

    private void OnDestroy()
    {
        rb.linearVelocity = Vector3.zero; // Stop movement
    }
}
