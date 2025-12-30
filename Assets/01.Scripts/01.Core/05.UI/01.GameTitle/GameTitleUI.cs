using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI; // Added for Image component

public class GameTitleUI : MonoBehaviour
{
    [Header("DOTween Horror Effect")]
    [SerializeField]
    private TextMeshProUGUI gameTitleText;
    [SerializeField]
    private float shakeStrength = 1.5f;
    [SerializeField]
    private int shakeVibrato = 15;
    [SerializeField]
    private float minFlickerDelay = 0.1f;
    [SerializeField]
    private float maxFlickerDelay = 0.8f;
    [SerializeField]
    private float flickerDuration = 0.08f;
    [SerializeField]
    private float flickerMinAlpha = 0.3f;

    // New fields for background effect
    [Header("Background Effect Settings")]
    [SerializeField]
    private Image backgroundImage; // Reference to the background Image component
    [SerializeField]
    private Color backgroundTargetColor = new Color(0.1f, 0.1f, 0.1f, 1.0f); // Darker, desaturated color
    [SerializeField]
    private float backgroundColorChangeDuration = 2.0f; // Duration for color change
    [SerializeField]
    private LoopType backgroundColorLoopType = LoopType.Yoyo; // Loop type for color change

    void Start()
    {
        if (gameTitleText != null)
        {
            // Continuous shake effect. A duration of -1 makes it indefinite.
            gameTitleText.transform.DOShakePosition(-1, shakeStrength, shakeVibrato, 90, false, true);

            // Start the random flicker effect
            RandomFlicker();
        }

        // Apply background effect
        if (backgroundImage != null)
        {
            backgroundImage.DOColor(backgroundTargetColor, backgroundColorChangeDuration)
                .SetLoops(-1, backgroundColorLoopType)
                .SetEase(Ease.InOutSine); // Smooth color transition
        }
    }

    void RandomFlicker()
    {
        // Wait for a random delay, then flicker out and in, then call itself to create a loop.
        float delay = Random.Range(minFlickerDelay, maxFlickerDelay);

        DOTween.Sequence()
            .AppendInterval(delay)
            .Append(gameTitleText.DOFade(flickerMinAlpha, flickerDuration))
            .Append(gameTitleText.DOFade(1.0f, flickerDuration))
            .OnComplete(RandomFlicker);
    }
}