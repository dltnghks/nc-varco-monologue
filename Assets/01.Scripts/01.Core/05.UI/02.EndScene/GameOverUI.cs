using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the UI flow for the Game Over scene.
/// Plays a final dialogue sequence and then quits the application.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [Tooltip("The dialogue sequence to play on this screen.")]
    [SerializeField] private List<WwiseAudioData> endingDialogue;

    void Start()
    {
        // Ensure DialogueManager exists to play the sequence.
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[GameOverUI] DialogueManager not found. Cannot play ending dialogue. Quitting immediately.");
            QuitGame();
            return;
        }

        // Define the action to take after the dialogue is finished: quit the game.
        System.Action onDialogueFinished = () => {
            Debug.Log("Game Over dialogue finished. Quitting game.");
            QuitGame();
        };

        // Play the dialogue sequence and pass the quit action as a callback.
        DialogueManager.Instance.PlayDialogueSequence(endingDialogue, onDialogueFinished);
    }

    private void QuitGame()
    {
        Debug.Log("Executing QuitGame.");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
