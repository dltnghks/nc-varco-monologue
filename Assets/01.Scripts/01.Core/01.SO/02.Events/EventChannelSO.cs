using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A generic, serializable ScriptableObject that acts as a channel for events of a specific type.
/// </summary>
/// <typeparam name="T">The type of data this channel will raise events for.</typeparam>
public abstract class EventChannelSO<T> : ScriptableObject
{
    [Header("Debug")]
    [Tooltip("Log to the console when an event is raised on this channel.")]
    [SerializeField] private bool showDebugLog = true;

    public event UnityAction<T> OnEventRaised;

    public void RaiseEvent(T value)
    {
        if (showDebugLog)
        {
            Debug.Log($"[EventChannel<{typeof(T).Name}>] Event Raised with value: {value}");
        }

        OnEventRaised?.Invoke(value);
    }
}
