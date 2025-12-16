using UnityEngine;

// This script handles player controls for a mobile device.
// - Gyroscope for rotation.
// - Touch to move forward.
// - Double-tap to interact.
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
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

    private Rigidbody rb;
    private bool gyroSupported;
    private Gyroscope gyro;

    // Input state
    private float keyboardRotationInput = 0f;
    private float mouseRotationInput = 0f;
    private float gyroRotationInput = 0f;
    private bool moveForwardInput = false;

    // Touch hold state
    private float touchStartTime = 0f;
    private bool isHolding = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
#if UNITY_IOS || UNITY_ANDROID
        // Gyro rotation
        if (gyroSupported)
        {
            float tilt = Input.gyro.gravity.x;
            if (Mathf.Abs(tilt) > tiltDeadZone)
            {
                gyroRotationInput = tilt * tiltRotationSpeed;
            }
            else
            {
                gyroRotationInput = 0f;
            }
        }
        
        // Touch movement and interaction
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (touch.tapCount == 2)
                    {
                        // This is a double-tap for interaction.
                        Interact();
                        isHolding = false;
                        moveForwardInput = false;
                    }
                    else
                    {
                        // This is a single tap, which could be the start of a hold.
                        touchStartTime = Time.time;
                        isHolding = false;
                        moveForwardInput = false;
                    }
                    break;

                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    // The touch is being held down. Check if the hold time has passed.
                    if (!isHolding && (Time.time - touchStartTime) > holdToMoveTime)
                    {
                        isHolding = true;
                    }
                    
                    // If we are in a recognized "hold" state, then we should move.
                    moveForwardInput = isHolding;
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // The touch has been released. Stop holding and moving.
                    isHolding = false;
                    moveForwardInput = false;
                    break;
            }
        }
        else
        {
            // No touches on the screen.
            isHolding = false;
            moveForwardInput = false;
        }
#else
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
        
        // Movement
        moveForwardInput = Input.GetMouseButton(0);
        
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
#else
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
            rb.MovePosition(rb.position + transform.forward * moveSpeed.Value * Time.fixedDeltaTime);
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