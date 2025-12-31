using UnityEngine;

public class NoiseTrap : MonoBehaviour, IInteractObject
{

    [Header("이벤트 채널")]
    public GameEventChannel gameEventChannel;

    [Header("Audio Data")]
    [SerializeField] private WwiseAudioData noiseSound; // 상호작용할 때 나는 소리

    public void OnTrap()
    {
        if (gameEventChannel != null)
        {
            gameEventChannel.RaiseEvent(EGameEvent.PlayerDamaged);
        }
        
        if (noiseSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(noiseSound, transform.position);
        }
    
        Destroy(gameObject);
    }

    public bool CanInteraction()
    {
        return true;
    }

    public void Interaction()
    {
        if (!CanInteraction())
        {
            return;
        }

        OnTrap();
    }

    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Warning!");
        if(collision.gameObject.tag == "Player")
        {
            OnTrap();
        }
    }

    // This function is called by the Unity editor to draw gizmos in the Scene view.
    private void OnDrawGizmos()
    {
        // Collider 컴포넌트를 가져옵니다.
        SphereCollider trapCollider = GetComponent<SphereCollider>();
        if (trapCollider != null)
        {
            // 기즈모의 색상을 붉은색으로 설정합니다.
            Gizmos.color = Color.red;
            // 콜라이더의 월드 공간 바운드를 와이어 큐브로 그립니다.
            Gizmos.DrawSphere(trapCollider.bounds.center, trapCollider.radius);
        }
    }
}