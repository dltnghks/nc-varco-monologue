using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections; // For Coroutines

[System.Serializable]
public struct SoundIndicatorStyle
{
    public ESoundType soundType;
    public Color indicatorColor;
}


public class InGameUI : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private SoundEventChannel soundEventChannel;

    [Header("UI Elements")]
    [SerializeField]
    private RectTransform centerCircle; // The parent RectTransform for indicators
    [SerializeField]
    private GameObject soundIndicatorPrefab; // Prefab for the indicator UI element

    [Header("Indicator Settings")]
    [SerializeField]
    private float fadeInDuration = 0.1f;
    [SerializeField]
    private float stayDuration = 1.0f;
    [SerializeField]
    private float fadeOutDuration = 0.5f;

    [Header("Arc Indicator Settings")]
    [SerializeField]
    private float arcFillAmount = 0.1f; // The width of the arc (0.0 to 1.0)
    [SerializeField]
    private int poolSize = 10;

    [Header("Distance Effects")]
    [SerializeField]
    private float maxDistance = 50f; // Sounds beyond this distance are at min scale
    [SerializeField]
    private float minDistance = 5f;  // Sounds closer than this are at max scale
    [SerializeField]
    private Vector2 scaleRange = new Vector2(0.7f, 1.2f); // min and max scale

    [Header("Indicator Styles")]
    [SerializeField]
    private List<SoundIndicatorStyle> indicatorStyles;

    private List<Image> indicatorPool;
    private Camera mainCamera;
    private Dictionary<ESoundType, SoundIndicatorStyle> styleLookup;
    private Dictionary<Image, Coroutine> activeIndicatorCoroutines = new Dictionary<Image, Coroutine>();
    private Sprite defaultIndicatorSprite; // To store the original sprite from the prefab.

    void OnEnable()
    {
        if (soundEventChannel != null)
        {
            soundEventChannel.OnSoundPlayed += ShowSoundIndicator;
        }
    }

    void OnDisable()
    {
        if (soundEventChannel != null)
        {
            soundEventChannel.OnSoundPlayed -= ShowSoundIndicator;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        indicatorPool = new List<Image>();

        styleLookup = new Dictionary<ESoundType, SoundIndicatorStyle>();
        foreach (var style in indicatorStyles)
        {
            if (!styleLookup.ContainsKey(style.soundType))
            {
                styleLookup.Add(style.soundType, style);
            }
        }

        if (soundIndicatorPrefab != null && centerCircle != null)
        {
            var prefabImage = soundIndicatorPrefab.GetComponent<Image>();
            if (prefabImage != null)
            {
                defaultIndicatorSprite = prefabImage.sprite;
            }
            else
            {
                Debug.LogError("The Sound Indicator Prefab must have an Image component!");
                return;
            }
            
            for (int i = 0; i < poolSize; i++)
            {
                indicatorPool.Add(CreateIndicator(i));
            }
        }
        else
        {
            Debug.LogError("Center Circle or Sound Indicator Prefab is not set in the inspector!");
        }
    }
    
    private Image CreateIndicator(int index)
    {
        GameObject indicatorGO = Instantiate(soundIndicatorPrefab, centerCircle);
        indicatorGO.name = $"Indicator_{index}";
        Image indicator = indicatorGO.GetComponent<Image>();
        indicatorGO.SetActive(false);
        return indicator;
    }

    private Image GetIndicatorFromPool()
    {
        foreach (var indicator in indicatorPool)
        {
            if (!indicator.gameObject.activeInHierarchy)
            {
                return indicator;
            }
        }

        if (soundIndicatorPrefab != null)
        {
            Image newIndicator = CreateIndicator(indicatorPool.Count);
            indicatorPool.Add(newIndicator);
            return newIndicator;
        }

        return null;
    }
    
    public void ShowSoundIndicator(Vector3 soundWorldPosition, ESoundType soundType)
    {
        if (centerCircle == null || mainCamera == null) return;

        Image indicator = GetIndicatorFromPool();
        if (indicator == null)
        {
            Debug.LogWarning("Sound indicator pool is exhausted, and no prefab is set.");
            return;
        }

        if (activeIndicatorCoroutines.TryGetValue(indicator, out Coroutine existingCoroutine) && existingCoroutine != null)
        {
            StopCoroutine(existingCoroutine);
            activeIndicatorCoroutines.Remove(indicator);
        }
        
        Coroutine newCoroutine = StartCoroutine(ProcessFortniteIndicator(indicator, soundWorldPosition, soundType, mainCamera.transform));
        activeIndicatorCoroutines[indicator] = newCoroutine;
    }

    private IEnumerator ProcessFortniteIndicator(Image indicator, Vector3 soundWorldPosition, ESoundType soundType, Transform playerTransform)
    {
        // --- Setup for Arc/Ring display ---
        indicator.type = Image.Type.Filled;
        indicator.fillMethod = Image.FillMethod.Radial360;
        indicator.fillOrigin = (int)Image.Origin360.Top; // Start filling from the top
        indicator.fillAmount = arcFillAmount;

        RectTransform rt = indicator.rectTransform;
        rt.anchoredPosition = Vector2.zero; // Center the indicator

        // --- Distance Effects ---
        float distance = Vector3.Distance(soundWorldPosition, playerTransform.position);
        float scaleMultiplier = 1.0f - Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
        float targetScale = Mathf.Lerp(scaleRange.x, scaleRange.y, scaleMultiplier);
        
        // --- Icon and Color per Sound Type ---
        Color indicatorColor = Color.white;
        float targetAlpha = 1f;

        // Reset to default sprite first to handle reuse from pool
        indicator.sprite = defaultIndicatorSprite;

        if (styleLookup.TryGetValue(soundType, out SoundIndicatorStyle style))
        {
            indicatorColor = style.indicatorColor;
            targetAlpha = style.indicatorColor.a; //* scaleMultiplier; // Closer sounds can be more opaque
        }

        // --- Initial Setup and Animation ---
        indicator.gameObject.SetActive(true);
        indicator.DOKill();
        
        indicatorColor.a = 0;
        indicator.color = indicatorColor;
        rt.localScale = Vector3.one * targetScale;

        DOTween.Sequence()
            .Append(indicator.DOFade(targetAlpha, fadeInDuration))
            .AppendInterval(stayDuration)
            .Append(indicator.DOFade(0f, fadeOutDuration))
            .OnComplete(() =>
            {
                indicator.gameObject.SetActive(false);
                indicator.rectTransform.localScale = Vector3.one;
                if (activeIndicatorCoroutines.ContainsKey(indicator))
                {
                    activeIndicatorCoroutines.Remove(indicator);
                }
            });

        // --- Real-time Rotation Loop ---
        while (indicator.gameObject.activeInHierarchy)
        {
            Vector3 directionToSound = soundWorldPosition - playerTransform.position;
            directionToSound.y = 0;
            
            float angle = Vector3.SignedAngle(playerTransform.forward, directionToSound.normalized, Vector3.up);
            
            rt.localRotation = Quaternion.Euler(0, 0, -angle);

            yield return null; // Wait for the next frame
        }

        yield break;
    }
}
