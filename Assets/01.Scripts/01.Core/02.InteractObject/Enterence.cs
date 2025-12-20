using UnityEngine;

public class Enterence : MonoBehaviour, IInteractObject
{
    [SerializeField] private ItemData requirementItem; 
    [SerializeField] private InventoryObject playerInventory;

    [Header("이벤트 채널")]
    [SerializeField] private TutorialEventChannel tutorialChannel;
    [SerializeField] private ETutorialEvent OnInteract_E;
    [SerializeField] private ETutorialEvent OnOpen_E;

    // 외부(플레이어)에서 호출할 함수
    public void Open()
    {
        if (tutorialChannel != null)
        {
            tutorialChannel.RaiseEvent(OnOpen_E);
            
        }
        Destroy(gameObject);
    }

    public bool CanInteraction()
    {
        return playerInventory.CheckItem(requirementItem);
    }

    public void Interaction()
    {
        if (tutorialChannel != null)
        {
            tutorialChannel.RaiseEvent(OnInteract_E);
        }

        if (!CanInteraction())
        {
            return;
        }

        Open();
    }
}
