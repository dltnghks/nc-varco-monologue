using UnityEngine;

public class Enterence : MonoBehaviour, IInteractObject
{
    [SerializeField] private ItemData requirementItem; 
    [SerializeField] private InventoryObject playerInventory;

    [Header("이벤트 채널")]
    [SerializeField] private GameEventChannel gameEventChannel;
    [SerializeField] private EGameEvent onFailOpenEvent;
    [SerializeField] private EGameEvent onOpenEvent;

    // 외부(플레이어)에서 호출할 함수
    public void Open()
    {
        if (gameEventChannel != null)
        {
            gameEventChannel.RaiseEvent(onOpenEvent);
        }
        Destroy(gameObject);
    }

    public bool CanInteraction()
    {
        return playerInventory.CheckItem(requirementItem);
    }

    public void Interaction()
    {

        if (!CanInteraction())
        {
            
            if (gameEventChannel != null)
            {
                gameEventChannel.RaiseEvent(onFailOpenEvent);
            }
            return;
        }

        Open();
    }
}
