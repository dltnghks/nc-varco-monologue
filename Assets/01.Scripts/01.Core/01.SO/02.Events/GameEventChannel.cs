using UnityEngine;

/// <summary>
/// A specific event channel for EGameEvent enums. Inherits from the generic EventChannelSO.
/// </summary>
[CreateAssetMenu(fileName = "GameEventChannel", menuName = "Events/Game Event Channel")]
public class GameEventChannel : EventChannelSO<EGameEvent>
{
    // All logic is handled by the generic base class.
}
