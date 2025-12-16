using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractObject
{
    [Header("데이터 연결")]
    public ItemData itemData; // 위에서 만든 'KeyCard_Red' SO를 여기에 드래그
    public InventoryObject playerInventory; // 'PlayerInventory' SO를 여기에 드래그

    // 외부(플레이어)에서 호출할 함수
    public void Pickup()
    {
        playerInventory.AddItem(itemData, 1);
    
        Destroy(gameObject);
    }

    public bool CanInteraction()
    {
        return true;
    }

    public void Interaction()
    {
        if (!CanInteraction())
        {
            return;
        }

        Pickup();
    }
}