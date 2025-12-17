using UnityEngine;

public class Enterence : MonoBehaviour, IInteractObject
{
    [SerializeField] private ItemData requirementItem; 
    [SerializeField] private InventoryObject playerInventory;

    [Header("이벤트 채널")]
    [SerializeField] private GameplayEventChannel eventChannel;
    [SerializeField] private EInGameEvent eventOnInteract;
    [SerializeField] private EInGameEvent eventOnOpen;

    // 외부(플레이어)에서 호출할 함수
    public void Open()
    {
        if (eventChannel != null)
        {
            eventChannel.RaiseEvent(eventOnOpen);
        }
        Destroy(gameObject);
    }

    public bool CanInteraction()
    {
        return playerInventory.CheckItem(requirementItem);
    }

    public void Interaction()
    {
        if (eventChannel != null)
        {
            eventChannel.RaiseEvent(eventOnInteract);
        }

        if (!CanInteraction())
        {
            return;
        }

        Open();
    }
}
