using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the UI flow for the Game Clear scene.
/// Plays a final dialogue sequence and then quits the application.
/// </summary>
public class GameClearUI : MonoBehaviour
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
