using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WwiseSoundEmitter : MonoBehaviour
{
    public Queue<AK.Wwise.Event> EventsSequenceQueue = new Queue<AK.Wwise.Event>();
    
    [Tooltip("각 이벤트 사이의 딜레이 (초)")]
    public float DelayBetweenEvents = 0.5f;
    
    public bool IsPlaying { get; private set; } = false;
    private Action onSequenceFinished;
    private AK.Wwise.Event currentEvent;

    // A queue to hold actions that need to be executed on the main thread.
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();

    private void Update()
    {
        // Process any actions that have been queued from other threads (like the audio thread).
        while (mainThreadActions.Count > 0)
        {
            mainThreadActions.Dequeue()?.Invoke();
        }
    }

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

    public void PlaySequence(WwiseAudioData audioData, Action onFinished = null)
    {
        if (IsPlaying)
        {
            Debug.LogWarning($"WwiseSoundEmitter on {gameObject.name} is already playing a sequence. Interrupting.");
            Stop();
        }
        
        SetWwiseAudioData(audioData);
        onSequenceFinished = onFinished;
        IsPlaying = true;
        PlayNextEvent();
    }

    public void PlaySequenceAndReturn(WwiseAudioData audioData)
    {
        PlaySequence(audioData, () => AudioManager.Instance.ReturnEmitterToPool(this));
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
            PlayNextEvent();
        }
    }

    private void OnEventCallback(object in_cookie, AkCallbackType in_type, object in_info)
    {
        if (in_type == AkCallbackType.AK_EndOfEvent)
        {
            // This callback comes from the audio thread.
            // We must queue the coroutine to be started on the main thread in Update.
            mainThreadActions.Enqueue(() => StartCoroutine(WaitAndPlayNext()));
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
            yield return null;
        }

        PlayNextEvent();
    }

    private void FinishSequence()
    {
        IsPlaying = false;
        currentEvent = null;
        Debug.Log($"Wwise Sequence Completed on {gameObject.name}!");
        
        onSequenceFinished?.Invoke();
        onSequenceFinished = null;
    }
    
    public void Stop()
    {
        if (IsPlaying && currentEvent != null)
        {
            currentEvent.Stop(gameObject);
        }
        
        StopAllCoroutines();
        mainThreadActions.Clear(); // Clear any pending actions
        EventsSequenceQueue.Clear();
        IsPlaying = false;
        currentEvent = null;
        onSequenceFinished = null;
    }
}
