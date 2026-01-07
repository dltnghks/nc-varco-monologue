using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A specific event channel for EPlayerEvent enums. Inherits from the generic EventChannelSO.
/// </summary>
[CreateAssetMenu(fileName = "PlayerEventChannel", menuName = "Events/Player Event Channel")]
public class PlayerEventChannel : EventChannelSO<EPlayerEvent>
{
    public event UnityAction<Vector2> OnScreenEvent;
    
    public void RaiseEvent(Vector2 value)
    {
        if (showDebugLog)
        {
            Debug.Log($"[EventChannel<EPlayerEvent>] Event Raised with value: {value}");
        }

        OnScreenEvent?.Invoke(value);
    }
}
