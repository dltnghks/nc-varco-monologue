using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Controls an enemy that patrols between two points,
/// waiting at a central location, and periodically raises a DangerDetected event.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Patrol Points")]
    [Tooltip("The first point in the patrol route.")]
    [SerializeField] private Transform pointA;
    [Tooltip("The second point in the patrol route.")]
    [SerializeField] private Transform pointB;
    [Tooltip("The central point where the enemy waits.")]
    [SerializeField] private Transform centerPoint;

    [Header("Patrol Timings")]
    [Tooltip("Time in seconds to travel from a point to the center.")]
    [SerializeField] private float travelTime = 15.0f;
    [Tooltip("Time in seconds to wait at the center point.")]
    [SerializeField] private float waitAtCenterTime = 3.0f;

    [Header("Event Settings")]
    [Tooltip("The event channel for general game events.")]
    [SerializeField] private GameEventChannel gameEventChannel;
    [SerializeField] private EGameEvent OnEnemyGameEvent;
    [SerializeField] private EGameEvent OnEnemyScreamGameEvent;
    [Tooltip("The event channel for enemy-specific commands.")]
    [SerializeField] private EnemyEventChannel enemyEventChannel;

    [Tooltip("The interval in seconds to raise the DangerDetected event.")]
    [SerializeField] private float dangerEventInterval = 2.0f;

    [Header("Sound Data")]
    [SerializeField] private WwiseAudioData screamAudioData;
    [SerializeField] private WwiseAudioData ambientAudioData;
    
    private Sequence patrolSequence;
    private bool isAmbientSoundMuted = false;

    private void OnEnable()
    {
        if (enemyEventChannel != null)
        {
            enemyEventChannel.OnEventRaised += OnEnemyEvent;
        }
    }

    private void OnDisable()
    {
        if (enemyEventChannel != null)
        {
            enemyEventChannel.OnEventRaised -= OnEnemyEvent;
        }
    }

    private void Start()
    {
        if (pointA == null || pointB == null || centerPoint == null)
        {
            Debug.LogError("[Enemy] Patrol points are not fully assigned. Disabling script.", this);
            enabled = false;
            return;
        }

        if (gameEventChannel == null || enemyEventChannel == null)
        {
            Debug.LogError("[Enemy] Event Channels are not assigned. Disabling script.", this);
            enabled = false;
            return;
        }

        StartPatrol();
        StartCoroutine(SoundLoop());
    }

    private void Update()
    {
        // Pause and resume the enemy's movement and actions based on the global interaction block state.
        if (GameManager.Instance != null && patrolSequence != null && patrolSequence.IsActive())
        {
            if (GameManager.Instance.IsInteractionBlocked)
            {
                if (patrolSequence.IsPlaying())
                {
                    patrolSequence.Pause();
                }
            }
            else
            {
                if (!patrolSequence.IsPlaying())
                {
                    patrolSequence.Play();
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Kill any running tweens to prevent errors when the object is destroyed.
        patrolSequence?.Kill();
        transform.DOKill();
    }
    
    /// <summary>
    /// Handles commands sent through the EnemyEventChannel.
    /// </summary>
    private void OnEnemyEvent(EEnemyEvent enemyEvent)
    {
        switch (enemyEvent)
        {
            case EEnemyEvent.Center:
                Debug.Log("[Enemy] Received Center command. Moving to center point.");
                // Stop the patrol and move to the center.
                patrolSequence?.Kill();
                transform.DOMove(centerPoint.position, travelTime / 2).SetEase(Ease.OutQuad);
                gameEventChannel.RaiseEvent(OnEnemyGameEvent);
                break;
            case EEnemyEvent.Scream:
                Debug.Log("[Enemy] Received Scream command. Screaming!");
                gameEventChannel.RaiseEvent(OnEnemyScreamGameEvent);
                // Placeholder for scream logic (e.g., play sound, animation)
                break;
        }
    }
    
    /// <summary>
    /// Builds and starts the looping patrol sequence.
    /// </summary>
    private void StartPatrol()
    {
        transform.position = pointA.position;

        patrolSequence = DOTween.Sequence();
        patrolSequence.Append(transform.DOMove(centerPoint.position, travelTime).SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    gameEventChannel.RaiseEvent(OnEnemyGameEvent);
                    if (screamAudioData != null)
                    {
                        AudioManager.Instance.PlayOneShot(screamAudioData, transform.position);
                        isAmbientSoundMuted = true;
                    }
                }))
            .AppendInterval(waitAtCenterTime)
            .Append(transform.DOMove(pointB.position, travelTime).SetEase(Ease.Linear)
                .OnStart(() => isAmbientSoundMuted = false))
            .Append(transform.DOMove(centerPoint.position, travelTime).SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    gameEventChannel.RaiseEvent(OnEnemyGameEvent);
                    if (screamAudioData != null)
                    {
                        AudioManager.Instance.PlayOneShot(screamAudioData, transform.position);
                        isAmbientSoundMuted = true;
                    }
                }))
            .AppendInterval(waitAtCenterTime)
            .Append(transform.DOMove(pointA.position, travelTime).SetEase(Ease.Linear)
                .OnStart(() => isAmbientSoundMuted = false))
            .SetLoops(-1);
    }
    
    /// <summary>
    /// Coroutine to periodically raise the DangerDetected event, but only when player interaction is not blocked.
    /// </summary>
    private IEnumerator DangerEventRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(dangerEventInterval);

            // Only raise event if interaction is not blocked
            if (GameManager.Instance != null && !GameManager.Instance.IsInteractionBlocked)
            {
                if (gameEventChannel != null)
                {
                    Debug.Log("[Enemy] Raising DangerDetected event.");
                    gameEventChannel.RaiseEvent(EGameEvent.DangerDetected);
                }
            }
        }
    }


    private IEnumerator SoundLoop()
    {
        while (true)
        {
            if (!isAmbientSoundMuted && !GameManager.Instance.IsInteractionBlocked)
            {
                AudioManager.Instance.PlayOneShot(ambientAudioData, transform.position);
            }
            yield return new WaitForSeconds(2f);
        }
    }
}
