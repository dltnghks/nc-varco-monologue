using UnityEngine;

/// <summary>
/// A specific event channel for EPlayerEvent enums. Inherits from the generic EventChannelSO.
/// </summary>
[CreateAssetMenu(fileName = "PlayerEventChannel", menuName = "Events/Player Event Channel")]
public class PlayerEventChannel : EventChannelSO<EPlayerEvent>
{
    // All logic is handled by the generic base class.
}
