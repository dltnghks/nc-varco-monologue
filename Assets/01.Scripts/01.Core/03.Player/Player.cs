using UnityEngine;

// This script handles player controls for a mobile device.
// - Gyroscope for rotation.
// - Touch to move forward.
// - Double-tap to interact.
// - Second finger touch to run.
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private PlayerEventChannel playerEventChannel;

    [Header("General Settings")]
    [Tooltip("Movement speed in meters per second.")]
    [SerializeField] private FloatVariable moveSpeed;
    [SerializeField] private FloatVariable playerHP;

    [Header("Player Inventory")]
    [SerializeField] private InventoryObject inventory;
    
    [Header("Tilt-based Rotation (New)")]
    [Tooltip("How fast the player rotates when tilting the device.")]
    [SerializeField] private float tiltRotationSpeed = 60.0f;
    [Tooltip("The tilt angle (from 0 to 1) where rotation starts.")]
    [SerializeField] private float tiltDeadZone = 0.05f;

    [Header("Mobile Touch Controls")]
    [Tooltip("Time in seconds to hold a touch before it counts as movement.")]
    [SerializeField] private float holdToMoveTime = 0.2f;
    [Tooltip("Multiplier for the movement speed when running.")]
    [SerializeField] private float runSpeedMultiplier = 2.0f;

    [Header("Sound Settings")]
    private WwiseSoundEmitter soundEmitter;
    [SerializeField] private WwiseAudioData footstepAudioData;
    [Tooltip("Interval between footstep sounds when walking.")]
    [SerializeField] private float walkFootstepInterval = 0.5f;
    [Tooltip("Interval between footstep sounds when running.")]
    [SerializeField] private float runFootstepInterval = 0.3f;

    private Rigidbody rb;
    private bool gyroSupported;
    private Gyroscope gyro;

    // Input state
    private float keyboardRotationInput = 0f;
    private float mouseRotationInput = 0f;
    private float gyroRotationInput = 0f;
    private bool moveForwardInput = false;
    public bool IsRunning { get; private set; } = false;
    private bool wasRunning = false;
    private bool wasWalking = false; // Add this to track walking state

    // Touch hold state
    private float touchStartTime = 0f;
    private bool isHolding = false;

    // Footstep timing
    private float lastFootstepTime = 0f;

    void Awake()
    {
        soundEmitter = GetComponent<WwiseSoundEmitter>();
        rb = GetComponent<Rigidbody>();
        inventory.Container.Clear();
    }

    void Start()
    {
        // --- Gyroscope Initialization ---
        gyroSupported = SystemInfo.supportsGyroscope;

        if (gyroSupported)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            Debug.Log("Gyroscope has been enabled.");
        }
        else
        {
            Debug.LogWarning("This device does not support Gyroscope.");
        }
    }

    void Update()
    {
        if (GameManager.Instance.IsInteractionBlocked)
        {
            // Force stop movement and running if interaction is blocked.
            moveForwardInput = false;
            IsRunning = false;
        }
        else
        {
            // Otherwise, get state from user input.
            HandleInputs();
        }

        // After the final state for this frame is determined, check for changes and raise events.
        if (playerEventChannel != null)
        {
            bool currentIsMoving = moveForwardInput; // Player is moving if moveForwardInput is true
            bool currentIsWalking = currentIsMoving && !IsRunning;

            // Handle Running events
            if (IsRunning != wasRunning)
            {
                if (IsRunning)
                {
                    Debug.Log("Player started running, raising event.");
                    playerEventChannel.RaiseEvent(EPlayerEvent.StartedRunning);
                }
                else // Stopped Running
                {
                    // If stopped running, but still moving (i.e., now walking)
                    if (currentIsMoving)
                    {
                        // No need to raise StoppedRunning if now walking, Walking event handles it
                    }
                    else // Stopped all movement
                    {
                        Debug.Log("Player stopped running/moving, raising event.");
                        playerEventChannel.RaiseEvent(EPlayerEvent.StoppedRunning); // Means stopped all movement
                    }
                }
            }
            
            // Handle Walking event (only if not running and actually moving)
            // Only raise Walking event if player is actually walking and it's a state change
            if (currentIsWalking != wasWalking)
            {
                if (currentIsWalking)
                {
                    Debug.Log("Player started walking, raising event.");
                    playerEventChannel.RaiseEvent(EPlayerEvent.StartedWalking);
                }
                else
                {
                    playerEventChannel.RaiseEvent(EPlayerEvent.StoppedWalking);
                }
                // else: If stopped walking, EPlayerEvent.StoppedRunning already handles cessation of all movement.
                // No explicit EPlayerEvent.StoppedWalking is defined/needed with current setup.
            }

            wasRunning = IsRunning;
            wasWalking = currentIsWalking; // Update wasWalking state
        }
    }

    void FixedUpdate()
    {
        // Movement logic is now independent of input polling, it just uses the state variables.
        HandleRotation();
        HandleMovement();
    }

    private void HandleInputs()
    {
        // Assume not moving or running until proven otherwise by input.
        moveForwardInput = false;
        IsRunning = false;
        gyroRotationInput = 0f;

#if UNITY_IOS || UNITY_ANDROID
        // Gyro rotation
        if (gyroSupported)
        {
            float tilt = Input.gyro.gravity.x;
            if (Mathf.Abs(tilt) > tiltDeadZone)
            {
                gyroRotationInput = tilt * tiltRotationSpeed;
            }
        }
        
        // Touch movement, interaction, and running
        if (Input.touchCount > 0)
        {
            // Raise feedback events for ALL touches for multi-touch UI feedback
            foreach (var t in Input.touches)
            {
                playerEventChannel.RaiseEvent(new TouchContext(t.fingerId, t.position, t.phase));
            }

            // Use the FIRST touch for player control (movement, interaction)
            Touch touch = Input.GetTouch(0);

            // Handle double-tap for interaction separately
            if (touch.phase == TouchPhase.Began)
            {
                if (touch.tapCount == 2)
                {
                    Interact();
                    isHolding = false; // Reset hold state to prevent movement
                    return; // Exit to avoid processing movement on a double-tap
                }
            }

            // Handle holding for movement
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartTime = Time.time;
                    isHolding = false;
                    break;
                
                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    if (!isHolding && (Time.time - touchStartTime) > holdToMoveTime)
                    {
                        isHolding = true;
                    }
                    break;
                
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isHolding = false;
                    break;
            }

            // If we are in a recognized "hold" state, we should move.
            if (isHolding)
            {
                moveForwardInput = true;
                // If moving and a second finger is down, we are running.
                if (Input.touchCount > 1)
                {
                    //Debug.Log("RUN");
                    IsRunning = true;
                }
                else
                {
                    //Debug.Log("Walk");
                    IsRunning = false;
                }
            }
        }
        else
        {
            // No touches on the screen, so not holding.
            isHolding = false;
        }
#endif

#if UNITY_EDITOR
        // PC keyboard and mouse input
        
        // Mouse rotation
        if (Input.GetMouseButton(1))
        {
            mouseRotationInput = Input.GetAxis("Mouse X") * 2.0f;
        }
        else
        {
            mouseRotationInput = 0f;
        }

        // Keyboard rotation
        keyboardRotationInput = Input.GetAxis("Horizontal") * 50f;
        
        // Movement and Running
        moveForwardInput = Input.GetMouseButton(0);
        //runInput = Input.GetKey(KeyCode.LeftControl);

        // Interaction
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Interact();
        }
#endif
    }

    private void HandleRotation()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (gyroSupported)
        {
            Quaternion deltaRotation = Quaternion.Euler(0, gyroRotationInput * Time.fixedDeltaTime, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
#endif

#if UNITY_EDITOR
        // PC Rotation
        Quaternion keyboardDeltaRotation = Quaternion.Euler(0, keyboardRotationInput * Time.fixedDeltaTime, 0);
        rb.MoveRotation(rb.rotation * keyboardDeltaRotation);
        
        if (mouseRotationInput != 0f)
        {
            Quaternion mouseDeltaRotation = Quaternion.Euler(0, mouseRotationInput, 0);
            rb.MoveRotation(rb.rotation * mouseDeltaRotation);
        }
#endif
    }

    private void HandleMovement()
    {
        if (moveForwardInput)
        {
            // Apply run speed multiplier if run input is active
            float currentSpeed = IsRunning ? moveSpeed.Value * runSpeedMultiplier : moveSpeed.Value;
            //Debug.Log(IsRunning + " : " + currentSpeed);
            rb.MovePosition(rb.position + transform.forward * currentSpeed * Time.fixedDeltaTime);

            // Determine current footstep interval based on movement state
            float currentFootstepInterval = IsRunning ? runFootstepInterval : walkFootstepInterval;

            // Play footstep sound at intervals
            if (footstepAudioData != null && Time.time >= lastFootstepTime + currentFootstepInterval)
            {
                soundEmitter.PlaySequence(footstepAudioData);
                lastFootstepTime = Time.time;
            }
        }
        else
        {
            // Reset lastFootstepTime when not moving, so a footstep plays immediately upon starting movement
            lastFootstepTime = 0f;
        }
    }

    private void Interact()
    {
        // Prevent interaction while dialogue is playing
        if (GameManager.Instance != null && GameManager.Instance.IsInteractionBlocked)
        {
            Debug.Log("Cannot interact: Dialogue is currently playing.");
            return;
        }

        Debug.Log("Interaction triggered!");
        if (playerEventChannel != null)
        {
            playerEventChannel.RaiseEvent(EPlayerEvent.Interacted);
        }

        // Draw a debug ray to visualize the interaction raycast
        // The ray will be red and visible for 1 second in the Scene view.
        Debug.DrawRay(transform.position, transform.forward * 3.0f, Color.red, 1.0f);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 3.0f))
        {
            Debug.Log("Player is facing: " + hit.collider.name);
            IInteractObject interactObject = hit.collider.GetComponent<IInteractObject>();
            if(interactObject == null)
            {
                return;
            }

            interactObject.Interaction();
        }
    }
}