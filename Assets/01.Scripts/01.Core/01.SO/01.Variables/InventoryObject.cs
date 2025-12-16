using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory/System")]
public class InventoryObject : ScriptableObject
{
    // 실제 아이템들이 들어갈 데이터베이스
    public List<InventorySlot> Container = new List<InventorySlot>();

    public void AddItem(ItemData _item, int _amount)
    {
        // 1. 이미 인벤토리에 있는지 확인 (Stackable 체크 등 로직 추가 가능)
        bool hasItem = false;
        for (int i = 0; i < Container.Count; i++)
        {
            if (Container[i].item == _item)
            {
                Container[i].AddAmount(_amount);
                hasItem = true;
                break;
            }
        }
        
        // 2. 없으면 새로 추가
        if (!hasItem)
        {
            Container.Add(new InventorySlot(_item, _amount));
        }
    }

    public bool CheckItem(ItemData _item)
    {
        bool hasItem = false;
        for (int i = 0; i < Container.Count; i++)
        {
            if (Container[i].item == _item)
            {
                hasItem = true;
                break;
            }
        }

        return hasItem;
    }
}