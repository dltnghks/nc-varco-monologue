public enum EGameEvent
{
    GameStarted,
    GameOver, 
    DoorInteracted,
    DoorOpened,
    // 사용자 요청 이벤트
    StepOnGlass,          // 유리 밟기
    DangerDetected,       // 위험 감지
    SecurityModuleAcquired, // 보안 모듈 획득
    EscapeRouteOpen,      // 탈출구 오픈
    GameEnd,              // 게임 종료
}

// 사운드 종류
public enum ESoundType
{
    Common,
    Warning,
    Special,
}
