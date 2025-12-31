using UnityEngine;

public class GameSceneRoom : MonoBehaviour
{
    [SerializeField] private GameEventChannel gameEventChannel;
    [SerializeField] private EGameEvent onStartGameEvent;
    
    private void Start()
    {
        gameEventChannel.RaiseEvent(onStartGameEvent);
    }
}
