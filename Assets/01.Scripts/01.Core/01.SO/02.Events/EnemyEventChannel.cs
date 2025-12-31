using UnityEngine;

/// <summary>
/// A specific event channel for EEnemyEvent enums. Inherits from the generic EventChannelSO.
/// </summary>
[CreateAssetMenu(fileName = "EnemyEventChannel", menuName = "Events/Enemy Event Channel")]
public class EnemyEventChannel : EventChannelSO<EEnemyEvent>
{
    // All logic is handled by the generic base class.
}
