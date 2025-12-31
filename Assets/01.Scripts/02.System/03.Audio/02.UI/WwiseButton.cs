using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class WwiseButton : MonoBehaviour, IPointerEnterHandler
{
    [Header("Audio Data")]
    [SerializeField] private WwiseAudioData onClickSound;
    [SerializeField] private WwiseAudioData onHoverSound;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(HandleClick);
    }

    private void HandleClick()
    {
        if (onClickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISound(onClickSound);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onHoverSound != null && AudioManager.Instance != null && button.interactable)
        {
            AudioManager.Instance.PlayUISound(onHoverSound);
        }
    }
}
