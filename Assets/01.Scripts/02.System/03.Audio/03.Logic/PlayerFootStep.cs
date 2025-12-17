using UnityEngine;

public class PlayerFootstep : MonoBehaviour
{
    // string으로 쓰지 마세요! (오타 위험)
    // Wwise 타입을 쓰면 인스펙터에서 Picker가 뜹니다.
    public AK.Wwise.Event footstepEvent; 

    public void PlayStep()
    {
        footstepEvent.Post(gameObject);
    }
}