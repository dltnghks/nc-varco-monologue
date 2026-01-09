using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FeedbackEffectUI : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private GameEventChannel gameEventChannel;

    [Header("References")]
    [SerializeField] private Image feedbackImage;

    [Header("Settings")]
    [SerializeField] private float flashSpeed = 2f;
    [SerializeField] private Color positiveFeedbackColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color warningFeedbackColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private Color negativeFeedbackColor = new Color(1f, 0f, 0f, 0.5f);

    private void Awake()
    {
        if(feedbackImage == null)
        {
            feedbackImage = GetComponent<Image>();
        }

        if (feedbackImage != null)
        {
            feedbackImage.color = Color.clear;
            feedbackImage.raycastTarget = false;
        }
    }

    private void OnEnable()
    {
        if (gameEventChannel != null)
        {
            gameEventChannel.OnEventRaised += HandleGameEvent;
        }
    }

    private void OnDisable()
    {
        if (gameEventChannel != null)
        {
            gameEventChannel.OnEventRaised -= HandleGameEvent;
        }
    }

    private void Update()
    {
        // DOTween now handles the fading, so no continuous update is needed here.
    }

    private void HandleGameEvent(EGameEvent gameEvent)
    {
        switch (gameEvent)
        {
            case EGameEvent.DangerDetected:
                ShowFeedback(warningFeedbackColor);
                break;
            case EGameEvent.GameOver:
                ShowFeedback(negativeFeedbackColor);
                break;
            
            case EGameEvent.SecurityModuleAcquired:
            case EGameEvent.EscapeRouteOpen:
                ShowFeedback(positiveFeedbackColor);
                break;
            
        }
    }

    private void ShowFeedback(Color feedbackColor)
    {
        if (feedbackImage != null)
        {
            feedbackImage.color = feedbackColor;
            DOTween.Kill(feedbackImage); // Kill any existing tweens on this image
            feedbackImage.DOFade(0f, flashSpeed); // Fade to clear
        }
    }
}