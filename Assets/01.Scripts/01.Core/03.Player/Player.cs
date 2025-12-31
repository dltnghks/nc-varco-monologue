using UnityEngine;

// This script handles player controls for a mobile device.
// - Gyroscope for rotation.
// - Touch to move forward.
// - Double-tap to interact.
// - Second finger touch to run.
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Game Play Event Channel")]
    [SerializeField] private TutorialEventChannel playerActionChannel;

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
    private bool runInput = false;

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
        HandleInputs();
    }

    void FixedUpdate()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleInputs()
    {
        // Reset per-frame input state
        moveForwardInput = false;
        //runInput = false;
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
            Touch touch = Input.GetTouch(0);

            // Handle double-tap for interaction separately
            if (touch.phase == TouchPhase.Began && touch.tapCount == 2)
            {
                Interact();
                isHolding = false; // Reset hold state to prevent movement
                return; // Exit to avoid processing movement on a double-tap
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
                    runInput = true;
                }
                else
                {
                    //Debug.Log("Walk");
                    runInput = false;
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
            float currentSpeed = runInput ? moveSpeed.Value * runSpeedMultiplier : moveSpeed.Value;
            //Debug.Log(runInput + " : " + currentSpeed);
            rb.MovePosition(rb.position + transform.forward * currentSpeed * Time.fixedDeltaTime);

            // Determine current footstep interval based on movement state
            float currentFootstepInterval = runInput ? runFootstepInterval : walkFootstepInterval;

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
        Debug.Log("Interaction triggered!");

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