using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [Tooltip("구독할 이벤트 에셋을 연결하세요")]
    public GameEvent Event;

    [Tooltip("이벤트가 발생했을 때 실행할 동작을 등록하세요")]
    public UnityEvent Response;

    private void OnEnable()
    {
        // 오브젝트가 활성화되면 이벤트 구독
        if (Event != null)
            Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화되면 이벤트 구독 해제
        if (Event != null)
            Event.UnregisterListener(this);
    }

    // GameEvent에서 호출하는 함수
    public void OnEventRaised()
    {
        Response.Invoke();
    }
}