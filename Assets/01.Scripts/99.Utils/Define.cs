public enum ETutorialEvent
{
    // 튜토리얼
    GameStart_S,
    GameStart_E,
    AimToDrone_S,
    AimToDrone_E,
    InteractToDoor_S,
    InteractToDoor_E,
    PickupKey_S,
    PickupKey_E,
    OpenDoor_S,
    OpenDoor_E,
    End,
}

// 게임 내 사운드 발생 이벤트
public enum ESoundEvent
{
    
}

// 사운드 종류
public enum ESoundType
{
    Common,
    Warning,
    Special,
}
