using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewEvent", menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject
{
    // 현재 이 이벤트를 구독하고 있는 리스너들의 목록
    private readonly List<GameEventListener> eventListeners = new List<GameEventListener>();

    // 이벤트를 발생시키는 함수 (방송 시작!)
    public void Raise()
    {
        // 목록을 역순으로 순회합니다.
        // 이유: 이벤트 발생 중에 객체가 파괴(Destroy)되어 리스트에서 제거될 경우,
        // 순방향 루프는 인덱스 오류를 일으킬 수 있기 때문입니다.
        for (int i = eventListeners.Count - 1; i >= 0; i--)
        {
            eventListeners[i].OnEventRaised();
        }
    }

    public void RegisterListener(GameEventListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(GameEventListener listener)
    {
        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }
}