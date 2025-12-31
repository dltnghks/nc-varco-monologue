using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

/// <summary>
/// Manages the overall game flow, state, and transitions.
/// It listens to a GameEventChannel to react to major gameplay events.
/// Implemented as a Singleton.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    [Header("Event Channel")]
    [Tooltip("The channel for receiving general game events.")]
    [SerializeField] private GameEventChannel gameEventChannel;
    [SerializeField] private EGameEvent onStartGameEvent; 

    /// <summary>
    /// Returns true if player interaction should be blocked (e.g., during a specific dialogue).
    /// This provides a central point for other systems to check this state.
    /// </summary>
    public bool IsInteractionBlocked => DialogueManager.Instance != null && DialogueManager.Instance.IsInteractionBlocked;

    //SerializedDictionary
    [Header("Event to Voice")]
    [SerializeField] private SerializedDictionary<EGameEvent, List<WwiseAudioData>> eventToVoiceData = new SerializedDictionary<EGameEvent, List<WwiseAudioData>>();
    

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Make the GameManager persist across scene loads
    }

    private void Start()
    {
        gameEventChannel.RaiseEvent(onStartGameEvent);
    }

    private void OnEnable()
    {
        if (gameEventChannel != null)
        {
            gameEventChannel.OnEventRaised += HandleGameEvent;
        }
    }

    private void OnDisable()
    {
        if (gameEventChannel != null)
        {
            gameEventChannel.OnEventRaised -= HandleGameEvent;
        }
    }

    /// <summary>
    /// Handles events raised by the GameEventChannel.
    /// </summary>
    /// <param name="eventKey">The key of the event that was raised.</param>
    private void HandleGameEvent(EGameEvent eventKey)
    {
        Debug.Log($"[GameManager] Received Event: <color=green>{eventKey}</color>");

        List<WwiseAudioData> audioDatas;
        if(eventToVoiceData.TryGetValue(eventKey, out audioDatas))
        {
            DialogueManager.Instance.PlayDialogueSequence(audioDatas);
        }

        switch (eventKey)
        {
            case EGameEvent.GameStarted:
                // Logic for when the game starts
                break;

            case EGameEvent.GameOver:
                // Logic for game over (e.g., show game over screen, stop player input)
                break;

            case EGameEvent.StepOnGlass:
                // Logic for when the player steps on glass
                break;

            case EGameEvent.DangerDetected:
                // Logic for when danger is detected
                break;

            case EGameEvent.SecurityModuleAcquired:
                // Logic for when a security module is acquired
                break;

            case EGameEvent.EscapeRouteOpenFail:
                // Logic for when the escape route is failed open 
                break;

            case EGameEvent.EscapeRouteOpen:
                // Logic for when the escape route is opened
                break;

            case EGameEvent.GameEnd:
                // Logic for when the game officially ends (e.g., showing credits, returning to main menu)
                break;

            default:
                Debug.LogWarning($"[GameManager] No handler for event: {eventKey}");
                break;
        }
    }
}
