using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // For Action

public class WwiseSoundEmitter : MonoBehaviour
{
    public Queue<AK.Wwise.Event> EventsSequenceQueue = new Queue<AK.Wwise.Event>();
    
    [Tooltip("각 이벤트 사이의 딜레이 (초)")]
    public float DelayBetweenEvents = 0.5f;
    
    private bool isPlaying = false;
    private Action onSequenceFinished;
    private AK.Wwise.Event currentEvent;

    public void SetWwiseAudioData(WwiseAudioData audioData)
    {
        EventsSequenceQueue.Clear();
        if (audioData != null)
        {
            foreach(AK.Wwise.Event wwiseEvent in audioData.wwiseEvents)
            {
                EventsSequenceQueue.Enqueue(wwiseEvent);
            }
        }
    }

    // For sounds attached to an object, managed manually
    public void PlaySequence(WwiseAudioData audioData)
    {
        SetWwiseAudioData(audioData);
        
        // Removed: if (isPlaying) Stop();
        // This allows multiple single-shot sounds to layer without interrupting each other.
        // The 'isPlaying' flag primarily manages internal sequence progression, not individual sound outputs.
    
        isPlaying = true;
        PlayNextEvent();
    }

    // For fire-and-forget sounds from the pool
    public void PlaySequenceAndReturn(WwiseAudioData audioData)
    {
        // When the sequence is done, this emitter will be returned to the pool.
        onSequenceFinished = () => AudioManager.Instance.ReturnEmitterToPool(this);
        PlaySequence(audioData);
    }

    private void PlayNextEvent()
    {
        if (!EventsSequenceQueue.TryDequeue(out currentEvent))
        {
            FinishSequence();
            return;
        }

        if (currentEvent != null)
        {
            Debug.Log($"Playing Wwise Event: {currentEvent.Name} on {gameObject.name}");
            currentEvent.Post(gameObject, (uint)AkCallbackType.AK_EndOfEvent, OnEventCallback, null);
        }
        else
        {
            // If there's a null event in the queue, skip it after a frame.
            StartCoroutine(WaitAndPlayNext());
        }
    }

    private void OnEventCallback(object in_cookie, AkCallbackType in_type, object in_info)
    {
        if (in_type == AkCallbackType.AK_EndOfEvent)
        {
            StartCoroutine(WaitAndPlayNext());
        }
    }

    private IEnumerator WaitAndPlayNext()
    {
        if (DelayBetweenEvents > 0)
        {
            yield return new WaitForSeconds(DelayBetweenEvents);
        }
        else
        {
            yield return null; // Wait a frame to prevent potential stack overflow
        }

        PlayNextEvent();
    }

    private void FinishSequence()
    {
        isPlaying = false;
        currentEvent = null;
        Debug.Log($"Wwise Sequence Completed on {gameObject.name}!");
        
        onSequenceFinished?.Invoke();
        onSequenceFinished = null; // Reset for next use
    }
    
    /// <summary>
    /// Stops the currently playing event and clears the sequence queue.
    /// </summary>
    public void Stop()
    {
        if (isPlaying && currentEvent != null)
        {
            // Stop the specific event instance on this game object.
            currentEvent.Stop(gameObject);
        }
        
        StopAllCoroutines();
        EventsSequenceQueue.Clear();
        isPlaying = false;
        currentEvent = null;
        onSequenceFinished = null;
    }
}