using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Event Channels")]
    [SerializeField] private SoundEventChannel soundEventChannel;

    [Header("Sound Emitter Pool")]
    [SerializeField] private WwiseSoundEmitter soundEmitterPrefab;
    [SerializeField] private int initialPoolSize = 15;

    private Queue<WwiseSoundEmitter> emitterPool = new Queue<WwiseSoundEmitter>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePool();
    }

    private void InitializePool()
    {
        if (soundEmitterPrefab == null)
        {
            Debug.LogError("soundEmitterPrefab is not set in the AudioManager inspector!");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            WwiseSoundEmitter emitter = Instantiate(soundEmitterPrefab, transform);
            emitter.gameObject.SetActive(false);
            emitterPool.Enqueue(emitter);
        }
    }

    public WwiseSoundEmitter GetEmitter()
    {
        if (emitterPool.Count > 0)
        {
            WwiseSoundEmitter emitter = emitterPool.Dequeue();
            emitter.gameObject.SetActive(true);
            return emitter;
        }
        else
        {
            // Pool is empty, create a new one dynamically
            WwiseSoundEmitter emitter = Instantiate(soundEmitterPrefab, transform);
            return emitter;
        }
    }

    public void ReturnEmitterToPool(WwiseSoundEmitter emitter)
    {
        emitter.gameObject.SetActive(false);
        emitter.transform.SetParent(transform);
        emitter.Stop(); // Stop any ongoing sound
        emitterPool.Enqueue(emitter);
    }

    /// <summary>
    /// Plays a sound at a specific world position. Good for one-shot effects.
    /// </summary>
    public void PlayOneShot(WwiseAudioData audioData, Vector3 position)
    {
        if (audioData == null || audioData.wwiseEvents.Count == 0) return;

        // Play the actual sound
        WwiseSoundEmitter emitter = GetEmitter();
        emitter.transform.position = position;
        emitter.PlaySequenceAndReturn(audioData);

        // Raise the visual indicator event
        if (soundEventChannel != null && audioData.needsVisualIndicator)
        {
            soundEventChannel.RaiseEvent(position, audioData.soundType);
        }
    }
    
    /// <summary>
    /// Plays a sound attached to a specific game object. Good for looping sounds or sounds that follow an object.
    /// </summary>
    public WwiseSoundEmitter PlayAttached(WwiseAudioData audioData, GameObject targetObject)
    {
        if (audioData == null || audioData.wwiseEvents.Count == 0) return null;

        WwiseSoundEmitter emitter = targetObject.GetComponent<WwiseSoundEmitter>();
        if (emitter == null)
        {
            emitter = targetObject.AddComponent<WwiseSoundEmitter>();
        }
        
        emitter.PlaySequence(audioData);
        
        // Raise the visual indicator event at the object's position
        if (soundEventChannel != null && audioData.needsVisualIndicator)
        {
            soundEventChannel.RaiseEvent(targetObject.transform.position, audioData.soundType);
        }

        return emitter;
    }

    /// <summary>
    /// Plays a 2D sound, ideal for UI.
    /// </summary>
    public void PlayUISound(WwiseAudioData audioData)
    {
        if (audioData == null || audioData.wwiseEvents.Count == 0) return;

        WwiseSoundEmitter emitter = GetEmitter();
        emitter.transform.position = transform.position; // Play from self
        emitter.PlaySequenceAndReturn(audioData);

        // UI sounds usually don't need a 3D world indicator, but the logic is here if needed.
        if (soundEventChannel != null && audioData.needsVisualIndicator)
        {
            soundEventChannel.RaiseEvent(transform.position, audioData.soundType);
        }
    }
}
