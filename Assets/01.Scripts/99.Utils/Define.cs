public enum EGameEvent
{
    GameStarted,
    GameOver, 
    // 사용자 요청 이벤트
    StepOnGlass,          // 유리 밟기
    DangerDetected,       // 위험 감지
    Safety,                 // 위험 종료
    SecurityModuleAcquired, // 보안 모듈 획득
    EscapeRouteOpenFail,    // 탈출구 오픈 실패
    EscapeRouteOpen,      // 탈출구 오픈
    GameEnd,              // 게임 종료
}

public enum EEnemyEvent
{
    Center,
    Scream,
}

// 사운드 종류
public enum ESoundType
{
    Common,
    Warning,
    Special,
}

public enum EPlayerEvent
{
    Walking,
    StartedRunning,
    StoppedRunning,
    Interacted,
}
