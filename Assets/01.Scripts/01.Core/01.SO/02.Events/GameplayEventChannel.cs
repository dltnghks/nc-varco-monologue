using UnityEngine;
using UnityEngine.Events;

/// 게임 전반의 비동기 이벤트를 중계하는 메신저 역할을 합니다.
/// (예: "PlayerJump", "EnemyKilled", "GamePaused")
[CreateAssetMenu(fileName = "GamePlayEventChannel", menuName = "Events/Gameplay Event Channel")]
public class GameplayEventChannel : ScriptableObject
{
    // 이벤트를 구독할 리스너들을 위한 Action
    // string 매개변수는 이벤트의 이름(Key)이나 추가 정보로 사용됩니다.
    public event UnityAction<EInGameEvent> OnEventRaised;

    [Header("Debug")]
    [Tooltip("체크하면 콘솔창에 어떤 이벤트가 발생했는지 로그를 찍습니다.")]
    [SerializeField] private bool showDebugLog = true;


    // 이벤트를 발생시킵니다. (Sender가 호출)
    public void RaiseEvent(EInGameEvent eventKey)
    {
        // 디버그 모드가 켜져있으면 로그 출력
        if (showDebugLog)
        {
            Debug.Log($"[Channel] Event Raised: {eventKey} <color=grey>(Time: {Time.time})</color>");
        }

        // 구독자가 있다면 이벤트 전파
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(eventKey);
        }
        else if (showDebugLog)
        {
            Debug.LogWarning($"[Channel] '{eventKey}' 이벤트가 발생했지만, 듣고 있는 시스템이 없습니다.");
        }
    }
}