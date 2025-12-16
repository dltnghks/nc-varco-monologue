using UnityEngine;

public class Enterence : MonoBehaviour, IInteractObject
{
    // 외부(플레이어)에서 호출할 함수
    public void Open()
    {
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

        Open();
    }
}
