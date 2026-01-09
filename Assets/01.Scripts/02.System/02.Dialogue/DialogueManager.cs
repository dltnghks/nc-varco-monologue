using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the playback of voice dialogue sequences.
/// Ensures that only one voice line plays at a time from a single, dedicated emitter.
/// New dialogue requests will interrupt any currently playing sequence.
/// Also tracks whether dialogue should block player actions.
/// </summary>
[RequireComponent(typeof(WwiseSoundEmitter))]
public class DialogueManager : MonoBehaviour
{
    // Singleton instance
    public static DialogueManager Instance { get; private set; }

    // State properties
    public bool IsInteractionBlocked { get; private set; } = false;
    private bool isSequenceRunning = false;

    private WwiseSoundEmitter dialogueEmitter;
    private Coroutine dialogueCoroutine = null;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Get the dedicated emitter for dialogue
        dialogueEmitter = GetComponent<WwiseSoundEmitter>();
    }

    /// <summary>
    /// Plays a sequence of dialogue audio data. Interrupts any currently playing dialogue.
    /// </summary>
    /// <param name="dialogueSequence">The list of dialogue audio to play in order.</param>
    /// <param name="onSequenceFinished">An optional action to be invoked when the entire sequence has finished playing.</param>
    public void PlayDialogueSequence(List<WwiseAudioData> dialogueSequence, Action onSequenceFinished = null)
    {
        if (dialogueSequence == null || dialogueSequence.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] Received null or empty dialogue sequence.");
            onSequenceFinished?.Invoke(); // Invoke callback immediately if there's nothing to play
            return;
        }

        // If a dialogue is already playing, stop it completely before starting the new one.
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueEmitter.Stop();
            
            isSequenceRunning = false;
            IsInteractionBlocked = false;
        }

        dialogueCoroutine = StartCoroutine(DialogueSequenceCoroutine(dialogueSequence, onSequenceFinished));
    }

    private IEnumerator DialogueSequenceCoroutine(List<WwiseAudioData> dialogueSequence, Action onSequenceFinished)
    {
        isSequenceRunning = true;
        Debug.Log("[DialogueManager] Starting dialogue sequence.");

        foreach (var audioData in dialogueSequence)
        {
            if (audioData == null) continue;

            // Set the blocking state based on the current dialogue line
            IsInteractionBlocked = audioData.blocksPlayerInteraction;
            if (IsInteractionBlocked)
            {
                Debug.Log($"[DialogueManager] Interaction blocked by: {audioData.name}");
            }
            
            bool isCurrentLineFinished = false;
            Action onFinished = () => { isCurrentLineFinished = true; };

            Debug.Log($"[DialogueManager] Playing line: {audioData.name}");
            dialogueEmitter.PlaySequence(audioData, onFinished);

            yield return new WaitUntil(() => isCurrentLineFinished);
        }

        Debug.Log("[DialogueManager] Dialogue sequence finished.");
        isSequenceRunning = false;
        IsInteractionBlocked = false; // Ensure it's false at the very end
        dialogueCoroutine = null;

        // Invoke the final callback now that the sequence is complete.
        onSequenceFinished?.Invoke();
    }
}