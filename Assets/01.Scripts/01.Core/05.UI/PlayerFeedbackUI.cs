using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro; // Assuming TextMeshPro is used for UI text
using DG.Tweening; // Import DOTween namespace

public class PlayerFeedbackUI : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private PlayerEventChannel playerEventChannel;

    [Header("Feedback Settings")]
    [Tooltip("The UI Prefab to instantiate for touch feedback. Should contain an Image component.")]
    [SerializeField] private GameObject touchFeedbackPrefab;
    [Tooltip("The RectTransform parent for the feedback UI elements (e.g., a Canvas).")]
    [SerializeField] private RectTransform feedbackParent;
    [Tooltip("Number of touch feedback prefabs to pre-instantiate for object pooling.")]
    [SerializeField] private int touchFeedbackPoolSize = 10;
    [Tooltip("Duration in seconds the touch feedback image stays visible.")]
    [SerializeField] private float touchFeedbackDisplayDuration = 0.1f;
    [Tooltip("Scale multiplier for the touch feedback image.")]
    [SerializeField] private float touchFeedbackScaleMultiplier = 1.5f;


    [Header("Player Status/Interaction Feedback")]
    [Tooltip("TextMeshProUGUI element to display player status messages (e.g., Running, Interacted).")]
    [SerializeField] private TextMeshProUGUI statusText;
    [Tooltip("Duration in seconds the status text stays visible.")]
    [SerializeField] private float statusTextDisplayDuration = 1.5f;
    [Tooltip("Vertical movement distance for the status text.")]
    [SerializeField] private float statusTextMoveYDistance = 50f;
    [Tooltip("Duration for status text fade in/out animations.")]
    [SerializeField] private float statusTextFadeDuration = 0.2f;


    private Queue<GameObject> touchFeedbackPool = new Queue<GameObject>();
    private Coroutine statusTextCoroutine;
    private Vector2 initialStatusTextPosition;

    void Awake()
    {
        InitializeTouchFeedbackPool();
        if (statusText != null)
        {
            initialStatusTextPosition = statusText.rectTransform.anchoredPosition;
            statusText.gameObject.SetActive(false); // Ensure it starts hidden
        }
    }

    void OnEnable()
    {
        if (playerEventChannel != null)
        {
            playerEventChannel.OnEventRaised += HandlePlayerEvent;
            playerEventChannel.OnScreenEvent += ShowTouchFeedback;
        }
    }

    void OnDisable()
    {
        if (playerEventChannel != null)
        {
            playerEventChannel.OnEventRaised -= HandlePlayerEvent;
            playerEventChannel.OnScreenEvent -= ShowTouchFeedback;
        }
    }

    private void InitializeTouchFeedbackPool()
    {
        if (touchFeedbackPrefab == null)
        {
            Debug.LogError("Touch Feedback Prefab is not assigned in PlayerFeedbackUI!", this);
            return;
        }
        if (feedbackParent == null)
        {
            Debug.LogError("Feedback Parent RectTransform is not assigned in PlayerFeedbackUI!", this);
            return;
        }

        for (int i = 0; i < touchFeedbackPoolSize; i++)
        {
            GameObject feedbackObject = Instantiate(touchFeedbackPrefab, feedbackParent);
            feedbackObject.SetActive(false);
            // Ensure feedback object has a CanvasGroup for fading with DOTween
            if (feedbackObject.GetComponent<CanvasGroup>() == null)
            {
                feedbackObject.AddComponent<CanvasGroup>();
            }
            touchFeedbackPool.Enqueue(feedbackObject);
        }
    }

    /// <summary>
    /// Displays a touch feedback image at the given screen position.
    /// </summary>
    /// <param name="screenPosition">The screen coordinates (e.g., Input.mousePosition or Touch.position).</param>
    public void ShowTouchFeedback(Vector2 screenPosition)
    {
        if (touchFeedbackPool.Count == 0)
        {
            Debug.LogWarning("Touch feedback pool is empty, consider increasing pool size.", this);
            GameObject newFeedbackObject = Instantiate(touchFeedbackPrefab, feedbackParent);
            if (newFeedbackObject.GetComponent<CanvasGroup>() == null)
            {
                newFeedbackObject.AddComponent<CanvasGroup>();
            }
            touchFeedbackPool.Enqueue(newFeedbackObject);
        }

        GameObject feedbackObject = touchFeedbackPool.Dequeue();
        feedbackObject.transform.position = screenPosition;
        feedbackObject.SetActive(true);

        // Reset state for DOTween animations
        CanvasGroup canvasGroup = feedbackObject.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        feedbackObject.transform.localScale = Vector3.one;

        // DOTween sequence for touch feedback
        Sequence touchSequence = DOTween.Sequence();
        touchSequence.Append(feedbackObject.transform.DOScale(Vector3.one * touchFeedbackScaleMultiplier, touchFeedbackDisplayDuration));
        if (canvasGroup != null)
        {
            touchSequence.Join(canvasGroup.DOFade(0f, touchFeedbackDisplayDuration));
        }
        touchSequence.OnComplete(() => {
            feedbackObject.SetActive(false);
            touchFeedbackPool.Enqueue(feedbackObject);
        });
    }

    // Removed the old IEnumerator DisplayTouchFeedbackRoutine as it's replaced by DOTween sequence.
    // private IEnumerator DisplayTouchFeedbackRoutine(GameObject feedbackObject) { ... }


    private void HandlePlayerEvent(EPlayerEvent eventType)
    {
        string message = string.Empty;
        switch (eventType)
        {
            case EPlayerEvent.StartedRunning:
                message = "Running";
                break;
            case EPlayerEvent.Walking:
                message = "Walking";
                break;
            case EPlayerEvent.Interacted:
                message = "Interacted!";
                break;
            default:
                return;
        }

        if (statusText != null && !string.IsNullOrEmpty(message))
        {
            // 상호작용의 경우에만 코루틴 중지하고 새로 시작
            if(EPlayerEvent.Interacted == eventType && statusTextCoroutine != null)
            {
                StopCoroutine(statusTextCoroutine);
            }
            else if (statusTextCoroutine != null)
            {
                return;
            }

            // Use DOTween for status text animation
            statusTextCoroutine = StartCoroutine(DisplayStatusTextDOTweenRoutine(message));
        }
    }

    private IEnumerator DisplayStatusTextDOTweenRoutine(string message)
    {
        statusText.text = message;
        statusText.gameObject.SetActive(true);
        statusText.rectTransform.anchoredPosition = initialStatusTextPosition; // Reset position

        // Ensure text is fully transparent before fading in
        Color originalColor = statusText.color;
        statusText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        Sequence statusSequence = DOTween.Sequence();

        // Fade in and move up
        statusSequence.Append(statusText.DOFade(1f, statusTextFadeDuration));
        statusSequence.Join(statusText.rectTransform.DOAnchorPosY(initialStatusTextPosition.y + statusTextMoveYDistance, statusTextDisplayDuration));

        // Wait for display duration minus fade times
        statusSequence.AppendInterval(statusTextDisplayDuration - (statusTextFadeDuration * 2));

        // Fade out
        statusSequence.Append(statusText.DOFade(0f, statusTextFadeDuration));

        statusSequence.OnComplete(() => {
            statusText.gameObject.SetActive(false);
            statusTextCoroutine = null;
        });

        yield return statusSequence.WaitForCompletion(); // Wait for the DOTween sequence to complete
    }

    // Removed the old IEnumerator DisplayStatusTextRoutine as it's replaced by DOTween sequence.
    // private IEnumerator DisplayStatusTextRoutine(string message) { ... }
}

