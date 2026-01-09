using UnityEngine;
using UnityEngine.Events;


public struct TouchContext
{
    public int fingerId;
    public Vector2 pos;
    public TouchPhase touchPhase;

    public TouchContext(int fingerId, Vector2 pos, TouchPhase touchPhase)
    {
        this.fingerId = fingerId;
        this.pos = pos;
        this.touchPhase = touchPhase;
    }
}

/// <summary>
/// A specific event channel for EPlayerEvent enums. Inherits from the generic EventChannelSO.
/// </summary>
[CreateAssetMenu(fileName = "PlayerEventChannel", menuName = "Events/Player Event Channel")]
public class PlayerEventChannel : EventChannelSO<EPlayerEvent>
{
    public event UnityAction<TouchContext> OnScreenEvent;
    
    public void RaiseEvent(TouchContext value)
    {
        if (showDebugLog)
        {
            Debug.Log($"[EventChannel<EPlayerEvent>] Event Raised with value: {value}");
        }

        OnScreenEvent?.Invoke(value);
    }
}
