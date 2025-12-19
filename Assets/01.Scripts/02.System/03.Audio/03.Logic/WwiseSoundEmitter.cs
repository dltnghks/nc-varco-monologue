using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseSoundEmitter : MonoBehaviour
{
    public Queue<AK.Wwise.Event> EventsSequenceQueue;
    
    [Tooltip("각 이벤트 사이의 딜레이 (초)")]
    public float DelayBetweenEvents = 0.5f;
    private bool isPlaying = false;

    public void SetWwiseAudioData(WwiseAudioData audioData)
    {
        foreach(AK.Wwise.Event wwiseEvent in audioData.wwiseEvents)
        {
            EventsSequenceQueue.Enqueue(wwiseEvent);
        }
    }

    // 외부에서 이 함수를 호출하여 시퀀스 시작
    public void PlaySequence(WwiseAudioData audioData)
    {
        SetWwiseAudioData(audioData);
        if (isPlaying) return; // 이미 재생 중이면 무시 (혹은 중단 로직 추가)
    
        isPlaying = true;

        PlayNextEvent();
    }

    private void PlayNextEvent()
    {
        // Queue에 있는 이벤트가 끝나면 종료
        if (!EventsSequenceQueue.TryDequeue(out AK.Wwise.Event evt))
        {
            FinishSequence();
            return;
        }

        if (evt != null)
        {
            // 핵심: Post 시에 Callback Flags와 Callback 함수를 전달
            // (uint)AkCallbackType.AK_EndOfEvent : 이벤트가 완전히 끝났을 때 콜백 발생
            evt.Post(gameObject, (uint)AkCallbackType.AK_EndOfEvent, OnEventCallback, null);
        }
        else
        {
            // 비어있는 이벤트가 있다면 바로 다음으로
            PlayNextEvent();
        }
    }

    // Wwise 엔진이 이벤트가 끝났을 때 호출해주는 함수
    private void OnEventCallback(object in_cookie, AkCallbackType in_type, object in_info)
    {
        if (in_type == AkCallbackType.AK_EndOfEvent)
        {
            // 딜레이가 필요하면 코루틴으로, 아니면 바로 호출
            // 주의: Wwise 콜백은 메인 스레드가 아닐 수 있으나, 
            // Unity Integration은 기본적으로 메인 스레드 처리를 도와줌.
            // 안전하게 Unity 로직을 타기 위해 StartCoroutine 활용.
            StartCoroutine(WaitAndPlayNext());
        }
    }

    private IEnumerator WaitAndPlayNext()
    {
        if (DelayBetweenEvents > 0)
        {
            yield return new WaitForSeconds(DelayBetweenEvents);
        }
        else
        {
            // 딜레이가 0이어도 한 프레임 쉬어주는 게 스택 오버플로우 방지에 안전
            yield return null; 
        }

        PlayNextEvent();
    }

    private void FinishSequence()
    {
        isPlaying = false;
        Debug.Log("Wwise Sequence Completed!");
    }
}