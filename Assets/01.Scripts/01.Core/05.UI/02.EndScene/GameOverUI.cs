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
        // Play the dialogue sequence and pass the load title action as a callback.
        DialogueManager.Instance.PlayDialogueSequence(endingDialogue);
    }

    void Update()
    {
        if(GameManager.Instance != null && GameManager.Instance.IsInteractionBlocked) return;
        // Detect any mouse click or touch input
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            Debug.Log("Screen clicked! Loading GameTitleScene...");
            SceneTransitionManager.Instance.LoadScene("GameTitleScene");
        }
    }

    private void QuitGame()
    {
        Debug.Log("Executing QuitGame.");
        SceneTransitionManager.Instance.LoadScene("GameTitleScene");  
    }
}
