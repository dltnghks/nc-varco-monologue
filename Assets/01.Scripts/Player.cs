using UnityEngine;

// This script handles player controls for a mobile device.
// - Gyroscope for rotation.
// - Touch to move forward.
// - Double-tap to interact.
public class Player : MonoBehaviour
{
    [Header("General Settings")]
    [Tooltip("Movement speed in meters per second.")]
    [SerializeField] private float moveSpeed = 2.0f;
    
    [Header("Tilt-based Rotation (New)")]
    [Tooltip("How fast the player rotates when tilting the device.")]
    [SerializeField] private float tiltRotationSpeed = 60.0f;
    [Tooltip("The tilt angle (from 0 to 1) where rotation starts.")]
    [SerializeField] private float tiltDeadZone = 0.05f;

    private bool gyroSupported;
    private Gyroscope gyro;

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
        HandleRotation();
        HandleMovementAndInteraction();
    }

    private void HandleRotation()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (gyroSupported)
        {
            // --- Tilt-based Rotation ---
            // We use the gravity vector's x component to determine left/right tilt.
            // When holding the phone upright, tilting left makes the gravity vector's x component positive.
            // Tilting right makes it negative.
            float tilt = Input.gyro.gravity.x;

            float rotationAmount = 0.0f;

            // Check if the phone is tilted beyond the dead zone.
            if (Mathf.Abs(tilt) > tiltDeadZone)
            {
                rotationAmount = tilt * tiltRotationSpeed;
            }

            // Apply the rotation, scaled by Time.deltaTime for frame-rate independence.
            transform.Rotate(0, rotationAmount * Time.deltaTime, 0);
        }
#else
        // On PC (Editor or Build), use Mouse and Keyboard for rotation.
        
        // 1. Mouse-based rotation (hold right-click and move)
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * 2.0f; // Mouse sensitivity
            transform.Rotate(0, mouseX, 0);
        }

        // 2. Keyboard-based rotation (A/D or Left/Right arrows)
        float keyboardRotation = Input.GetAxis("Horizontal") * 50f * Time.deltaTime; // Keyboard rotation speed (degrees per second)
        transform.Rotate(0, keyboardRotation, 0);
#endif
    }

    private void HandleMovementAndInteraction()
    {
#if UNITY_IOS || UNITY_ANDROID
        // Use touch controls for mobile
        if (Input.touchCount > 0)
        {
            MoveForward();

            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && touch.tapCount == 2)
            {
                Interact();
            }
        }
#else
        // Use keyboard/mouse controls for PC
        if (Input.GetMouseButton(0))
        {
            MoveForward();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Interact();
        }
#endif
    }

    private void MoveForward()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void Interact()
    {
        Debug.Log("Interaction triggered!");

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 3.0f))
        {
            Debug.Log("Player is facing: " + hit.collider.name);
        }
    }
}
