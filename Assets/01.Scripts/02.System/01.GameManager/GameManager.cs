using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using System;

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
    [SerializeField] private EGameEvent onGameOverEvnet; 

    /// <summary>
    /// Returns true if player interaction should be blocked (e.g., during a specific dialogue).
    /// This provides a central point for other systems to check this state.
    /// </summary>
    public bool IsInteractionBlocked => DialogueManager.Instance != null && DialogueManager.Instance.IsInteractionBlocked;

    //SerializedDictionary
    [Header("Event to Voice")]
    [SerializeField] private SerializedDictionary<EGameEvent, List<WwiseAudioData>> eventToVoiceData = new SerializedDictionary<EGameEvent, List<WwiseAudioData>>();
    
    private int consecutiveDangerDetectedCount = 0;

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

        // --- State Update: Check for consecutive danger events ---
        if (eventKey == EGameEvent.DangerDetected)
        {
            consecutiveDangerDetectedCount++;
        }
        else
        {
            consecutiveDangerDetectedCount = 0;
        }

        // --- Rule Logic: If danger count is too high, override the event to GameOver ---
        if(consecutiveDangerDetectedCount >= 2)
        {
            Debug.LogWarning("[GameManager] Two consecutive DangerDetected events! Overriding to GameOver.");
            eventKey = EGameEvent.GameOver;
            consecutiveDangerDetectedCount = 0;
        }

        // --- Logic Definition: Define what to do AFTER dialogue for each event ---
        Action onDialogueFinished = null;
        switch (eventKey)
        {
            case EGameEvent.GameOver:
                onDialogueFinished = () => {
                    Debug.Log("GAME OVER LOGIC: Show UI, stop player, etc.");
                    // gameEventChannel.RaiseEvent(onGameOverEvnet); // Be careful not to create an infinite loop if GameOver has its own dialogue.
                };
                break;

            case EGameEvent.GameStarted:
                onDialogueFinished = () => HandleGameStart();
                break;
            
            // Add other cases here for logic that should run after dialogue.
            // For events with no follow-up logic, no case is needed.
        }

        // --- Execution: Play dialogue and pass the defined logic as a callback ---
        HandleDefaultEvent(eventKey, onDialogueFinished);
    }

    private void HandleGameStart()
    {
        Debug.Log("GAME STARTED LOGIC");
    }

    /// <summary>
    /// The default event handler, which plays an associated dialogue and executes a callback upon completion.
    /// </summary>
    private void HandleDefaultEvent(EGameEvent eventKey, Action onFinished = null)
    {
        if (eventToVoiceData.TryGetValue(eventKey, out var audioDatas) && audioDatas.Count > 0)
        {
            // If dialogue exists, play it and pass the callback to the DialogueManager.
            DialogueManager.Instance.PlayDialogueSequence(audioDatas, onFinished);
        }
        else
        {
            // If no dialogue exists, execute the callback immediately.
            onFinished?.Invoke();
        }
    }
}
