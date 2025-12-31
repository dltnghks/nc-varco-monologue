using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Audio/Sound Event Channel")]
public class SoundEventChannel : ScriptableObject
{
    // Event to be raised when a sound is played that needs a visual indicator.
    // It passes the world position of the sound and its type.
    public Action<Vector3, ESoundType> OnSoundPlayed;

    /// <summary>
    /// Raises the sound event, notifying all listeners (e.g., InGameUI)
    /// to display a visual indicator for the sound.
    /// </summary>
    /// <param name="position">The world position where the sound originated.</param>
    /// <param name="soundType">The type of sound (e.g., Footstep, Gunshot).</param>
    public void RaiseEvent(Vector3 position, ESoundType soundType)
    {
        OnSoundPlayed?.Invoke(position, soundType);
    }
}
