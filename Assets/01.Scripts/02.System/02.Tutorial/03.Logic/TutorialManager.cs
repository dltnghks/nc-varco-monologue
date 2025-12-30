using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WwiseSoundEmitter))]
public class TutorialManager : MonoBehaviour
{
    [Header("Listening")]
    [SerializeField] private TutorialEventChannel tutorialEventChannel;
    [SerializeField] private TutorialSequence tutorialSequence;
    private bool[] completedTutorial = new bool[(int)ETutorialEvent.End];

    private WwiseSoundEmitter soundEmitter;
    private int currentStepIndex = -1;
    private TutorialStep currentTutorialStep;

    public void Awake()
    {
        soundEmitter = GetComponent<WwiseSoundEmitter>();
        StartTutorial();
    }

    private void OnEnable()
    {
        // 채널 구독
        if (tutorialEventChannel != null)
            tutorialEventChannel.OnEventRaised += HandleGameplayEvent;

    }

    private void OnDisable()
    {
        // 채널 구독 해지
        if (tutorialEventChannel != null)
            tutorialEventChannel.OnEventRaised -= HandleGameplayEvent;
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

        if (currentTutorialStep.AutomateNextEvent)
        {
            Debug.Log($"Automate next event: {currentTutorialStep.TutorialID}");
            HandleGameplayEvent(currentTutorialStep.TutorialID);
        }
        else
        {
            Debug.Log($"Waiting for event: {currentTutorialStep.TutorialID}");
        }
    }

    private void HandleGameplayEvent(ETutorialEvent eventKey)
    {
        if(currentStepIndex != (int)eventKey)
        {
            Debug.LogWarning($"순서에 맞지 않습니다. currentStepIndex : {currentStepIndex}, eventKey : {eventKey}");
            return; 
        }

        if(completedTutorial[(int)eventKey]) {
            Debug.Log($"이미 수행한 튜토리얼입니다. : {eventKey}");
            return;
        }


        completedTutorial[(int)eventKey] = true;

        PlayVoice(currentTutorialStep.AudioData);

        Debug.Log($"Tutorial Step {currentStepIndex + 1} completed by event: {eventKey}");
        AdvanceTutorial();
    }

    private void AdvanceTutorial()
    {
        currentStepIndex++;
        LoadCurrentStep();
    }

    private void PlayVoice(WwiseAudioData audioData)
    {
        if(currentTutorialStep == null) return;
        if(soundEmitter == null) return;

        Debug.Log($"Playing tutorial voice: {audioData.wwiseEvents[0].Name}");
        soundEmitter.PlaySequence(audioData);
    }
}