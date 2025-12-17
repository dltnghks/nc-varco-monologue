using UnityEngine;

[CreateAssetMenu(fileName = "new step", menuName = "Tutorial/TutorialStep")]
public class TutorialStep : ScriptableObject
{
    [Header("완료 조건")]
    [Tooltip("이 단계를 완료시키기 위해 필요한 게임 이벤트입니다.")]
    public EInGameEvent completionEvent;

    [Header("내용")]
    [TextArea(3, 5)] 
    public string tutorialText;

    [Tooltip("튜토리얼 오디오 이벤트")]
    public AK.Wwise.Event voiceEvent;
}
