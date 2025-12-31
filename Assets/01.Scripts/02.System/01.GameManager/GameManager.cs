using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using AK.Wwise;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
// using UnityEngine.SceneManagement; // Now handled by SceneTransitionManager

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
    [Tooltip("The channel for receiving player-specific events.")]
    [SerializeField] private PlayerEventChannel playerEventChannel;
    [SerializeField] private EGameEvent onStartGameEvent; 

    [Header("Wwise")]
    [Tooltip("The ambient sound to play at the start of the game.")]
    [SerializeField] private AK.Wwise.Event ambEvent;
    [SerializeField] private AK.Wwise.Event heartbeatEvent;
    [Tooltip("The RTPC to control the danger level.")]
    [SerializeField] private AK.Wwise.RTPC dangerLevelRtpc;
    [Tooltip("How long the RTPC tween takes in seconds.")]
    [SerializeField] private float rtpcTweenDuration = 2.0f;
    [Tooltip("The easing function for the RTPC transition.")]
    [SerializeField] private Ease rtpcEaseType = Ease.InOutQuad;

    /// <summary>
    /// Returns true if player interaction should be blocked (e.g., during a specific dialogue).
    /// This provides a central point for other systems to check this state.
    /// </summary>
    public bool IsInteractionBlocked => DialogueManager.Instance != null && DialogueManager.Instance.IsInteractionBlocked;

    [Header("Game Rules")]
    [Tooltip("How long the 'Danger State' lasts in seconds after a danger event.")]
    [SerializeField] private float dangerStateDuration = 10.0f;
    [Tooltip("A grace period after a danger event before it becomes lethal to run or make more noise.")]
    [SerializeField] private float dangerGracePeriod = 1.0f;

    //SerializedDictionary
    [Header("Event to Voice")]
    [SerializeField] private SerializedDictionary<EGameEvent, List<WwiseAudioData>> eventToVoiceData = new SerializedDictionary<EGameEvent, List<WwiseAudioData>>();


    // State machine fields
    private bool isGameOverInProgress = false;
    private bool isEnteringDangerState = false;
    private bool isInDangerState = false;
    private Coroutine dangerStateCoroutine;
    private Tween dangerLevelTween;
    private float currentDangerLevel = 0f;

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
        if (playerEventChannel != null)
        {
            playerEventChannel.OnEventRaised += HandlePlayerEvent;
        }
    }

    private void OnDisable()
    {
        dangerLevelTween?.Kill();
        if (gameEventChannel != null)
        {
            gameEventChannel.OnEventRaised -= HandleGameEvent;
        }
        if (playerEventChannel != null)
        {
            playerEventChannel.OnEventRaised -= HandlePlayerEvent;
        }
    }

    /// <summary>
    /// Handles events raised by the PlayerEventChannel.
    /// </summary>
    private void HandlePlayerEvent(EPlayerEvent playerEvent)
    {
        if (playerEvent == EPlayerEvent.StartedRunning && isInDangerState)
        {
            Debug.LogWarning("[GameManager] Game Over: Player started running during Danger State!");
            StartGameOverSequence("Running makes too much noise when you're in danger.");
        }
    }

    /// <summary>
    /// Handles events raised by the GameEventChannel.
    /// </summary>
    private void HandleGameEvent(EGameEvent eventKey)
    {
        Debug.Log($"[GameManager] Received Event: <color=green>{eventKey}</color>");

        switch (eventKey)
        {
            case EGameEvent.StepOnGlass:
                // Play the glass sound, then immediately raise a DangerDetected event.
                HandleDefaultEvent(eventKey, () => gameEventChannel.RaiseEvent(EGameEvent.DangerDetected));
                break;

            case EGameEvent.DangerDetected:
                if (isEnteringDangerState || isInDangerState)
                {
                    Debug.LogWarning("[GameManager] Game Over: Consecutive danger events!");
                    StartGameOverSequence("One noise is a warning, two is a death sentence.");
                }
                else
                {
                    // This is the first danger event. Start the process of entering the danger state.
                    HandleDefaultEvent(eventKey); // Play the associated warning dialogue.
                    if (dangerStateCoroutine != null) StopCoroutine(dangerStateCoroutine);
                    dangerStateCoroutine = StartCoroutine(EnterDangerStateSequence());
                }
                break;

            case EGameEvent.GameStarted:
                HandleDefaultEvent(eventKey, () => HandleGameStart());
                break;

            case EGameEvent.GameEnd:
                // For GameEnd (Game Clear), play a final dialogue, then load the clear scene.
                Action onGameEndDialogueFinished = () => {
                    Debug.Log("GAME CLEAR: Loading GameClearScene with fade.");
                    SceneTransitionManager.Instance.LoadScene("GameClearScene");
                };
                HandleDefaultEvent(eventKey, onGameEndDialogueFinished);
                break;

            default:
                // For all other events, just play their dialogue.
                HandleDefaultEvent(eventKey);
                break;
        }
    }

    /// <summary>
    /// A helper method to centralize the process of triggering a game over.
    /// </summary>
    private void StartGameOverSequence(string reason)
    {
        if (isGameOverInProgress) return;
        isGameOverInProgress = true;

        Debug.LogWarning($"Starting GameOver Sequence. Reason: {reason}");

        // Define the final action: load the GameOver scene.
        Action onDialogueFinished = () => {
            Debug.Log("GAME OVER: Loading GameOverScene with fade.");
            SceneTransitionManager.Instance.LoadScene("GameOverScene");
        };
        
        // Play the generic GameOver dialogue, then execute the action.
        if (eventToVoiceData.TryGetValue(EGameEvent.GameOver, out var gameOverDialogue))
        {
            DialogueManager.Instance.PlayDialogueSequence(gameOverDialogue, onDialogueFinished);
        }
        else
            onDialogueFinished();
        
    }

    /// <summary>
    /// Coroutine that implements the grace period before entering the full danger state.
    /// </summary>
    private IEnumerator EnterDangerStateSequence()
    {
        isEnteringDangerState = true;
        Debug.Log($"[GameManager] Entering danger grace period for {dangerGracePeriod} seconds.");

        yield return new WaitForSeconds(dangerGracePeriod);

        isEnteringDangerState = false;
        isInDangerState = true;
        
        dangerLevelTween?.Kill();
        dangerLevelTween = DOTween.To(() => currentDangerLevel, x => {
            currentDangerLevel = x;
            dangerLevelRtpc?.SetValue(gameObject, currentDangerLevel);
        }, 100f, rtpcTweenDuration).SetEase(rtpcEaseType);
        
        Debug.Log($"[GameManager] Grace period over. Now in Danger State for {dangerStateDuration} seconds. Tweening DangerLevel to 100.");


        // Start the main timer for how long the danger state lasts.
        dangerStateCoroutine = StartCoroutine(DangerStateTimer());
    }

    /// <summary>
    /// Coroutine to automatically exit the danger state after a duration.
    /// </summary>
    private IEnumerator DangerStateTimer()
    {
        yield return new WaitForSeconds(dangerStateDuration);
        Debug.Log("[GameManager] Danger State has expired. Tweening DangerLevel back to 0.");

        dangerLevelTween?.Kill();
        dangerLevelTween = DOTween.To(() => currentDangerLevel, x => {
            currentDangerLevel = x;
            dangerLevelRtpc?.SetValue(gameObject, currentDangerLevel);
        }, 0f, rtpcTweenDuration).SetEase(rtpcEaseType);

        isInDangerState = false;
    }

    /// <summary>
    /// Contains the logic to be executed when the game starts (after any intro dialogue).
    /// </summary>
    private void HandleGameStart()
    {
        Debug.Log("GAME STARTED LOGIC");
        if (ambEvent != null)
        {
            ambEvent.Post(gameObject);
            Debug.Log("[GameManager] Posted AMB event.");
        }

        if (heartbeatEvent != null)
        {
            heartbeatEvent.Post(gameObject);
            Debug.Log("[GameManager] Posted heartbeatEvent event.");
        }
        this.enabled = true; // Ensure the component is active at game start.
    }

    /// <summary>
    /// The default event handler, which plays an associated dialogue and executes a callback upon completion.
    /// </summary>
    private void HandleDefaultEvent(EGameEvent eventKey, Action onFinished = null)
    {
        if (eventToVoiceData.TryGetValue(eventKey, out var audioDatas) && audioDatas.Count > 0)
        {
            DialogueManager.Instance.PlayDialogueSequence(audioDatas, onFinished);
        }
        else
        {
            onFinished?.Invoke();
        }
    }
}
