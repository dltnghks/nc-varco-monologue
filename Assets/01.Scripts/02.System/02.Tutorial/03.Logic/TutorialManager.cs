using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Listening")]
    [SerializeField] private GameplayEventChannel gameplayChannel;

    [SerializeField] private TutorialSequence tutorialSequence;

    private int currentStepIndex = -1;
    private TutorialStep currentTutorialStep;

    void Start()
    {
        StartTutorial();
    }

    private void OnEnable()
    {
        // 채널 구독 (전화기 켬)
        if (gameplayChannel != null)
            gameplayChannel.OnEventRaised += HandleGameplayEvent;
    }

    private void OnDisable()
    {
        // 채널 구독 해지 (전화기 끔) - 필수! 메모리 누수 방지
        if (gameplayChannel != null)
            gameplayChannel.OnEventRaised -= HandleGameplayEvent;
    }

    private void StartTutorial()
    {
        currentStepIndex = 0;
        LoadCurrentStep();
    }

    private void LoadCurrentStep()
    {
        if (tutorialSequence == null || tutorialSequence.tutorialSteps == null || currentStepIndex >= tutorialSequence.tutorialSteps.Count)
        {
            Debug.Log("Tutorial Completed! or Tutorial Sequence not set up.");
            this.enabled = false; // 튜토리얼 매니저 비활성화
            return;
        }

        currentTutorialStep = tutorialSequence.tutorialSteps[currentStepIndex];
        Debug.Log($"Tutorial Step {currentStepIndex + 1}: Loaded {currentTutorialStep.name}");
        Debug.Log($"Waiting for event: {currentTutorialStep.completionEvent}");
    }

    private void HandleGameplayEvent(EInGameEvent eventKey)
    {
        if (currentTutorialStep != null && eventKey == currentTutorialStep.completionEvent)
        {
            Debug.Log($"Tutorial Step {currentStepIndex + 1} completed by event: {eventKey}");
            AdvanceTutorial();
        }
    }

    private void AdvanceTutorial()
    {
        currentStepIndex++;
        LoadCurrentStep();
    }
}