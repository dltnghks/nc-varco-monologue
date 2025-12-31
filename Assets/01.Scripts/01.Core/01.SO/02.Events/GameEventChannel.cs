using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A ScriptableObject-based event channel for general gameplay events.
/// Components can subscribe to this channel to be notified of events, and other components can raise events.
/// This decouples the event sender from the event receiver.
/// </summary>
[CreateAssetMenu(fileName = "GameEventChannel", menuName = "Events/Game Event Channel")]
public class GameEventChannel : ScriptableObject
{
    public event UnityAction<EGameEvent> OnEventRaised;

    [Header("Debug")]
    [Tooltip("Log to the console when an event is raised.")]
    [SerializeField] private bool showDebugLog = true;

    /// <summary>
    /// Raises an event on the channel.
    /// </summary>
    /// <param name="eventKey">The enum key for the event.</param>
    public void RaiseEvent(EGameEvent eventKey)
    {
        if (showDebugLog)
        {
            Debug.Log($"[GameEventChannel] Event Raised: <color=blue>{eventKey}</color> (Time: {Time.time})");
        }

        OnEventRaised?.Invoke(eventKey);
    }
}
