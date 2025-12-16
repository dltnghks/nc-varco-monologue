using UnityEngine;

[System.Serializable] // 인스펙터에 보이게 하기 위함
public class InventorySlot
{
    public ItemData item; // 무슨 아이템인가?
    public int amount;    // 몇 개인가?

    public InventorySlot(ItemData _item, int _amount)
    {
        item = _item;
        amount = _amount;
    }
    
    public void AddAmount(int value) => amount += value;
}