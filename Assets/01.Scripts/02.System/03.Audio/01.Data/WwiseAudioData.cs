using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioData", menuName = "Audio/Wwise Audio Data")]
public class WwiseAudioData : ScriptableObject
{
    [Header("Wwise Event")]
    // 문자열("Play_Footstep") 대신 Wwise Type을 쓰면 드래그&드롭이 가능하고 안전합니다.
    public List<AK.Wwise.Event> wwiseEvents; 

    [Header("Optional Settings")]
    public string comment; // 사운드 디자이너를 위한 메모
}