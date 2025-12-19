using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new step", menuName = "Tutorial/TutorialStep")]
public class TutorialStep : ScriptableObject
{
    [Header("튜토리얼 이벤트")] 
    public ETutorialEvent TutorialID; // ID같은 느낌?

    [Header("내용")]
    [TextArea(3, 5)] 
    public string TutorialText;

    [Header("나레이션")]
    public WwiseAudioData AudioData;

    [Tooltip("각 이벤트 사이의 딜레이 (초)")]
    public float DelayBetweenEvents = 0.5f;
}
