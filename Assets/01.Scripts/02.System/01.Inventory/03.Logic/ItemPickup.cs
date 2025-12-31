using System.Collections;
using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractObject
{
    [Header("데이터 연결")]
    public ItemData itemData; // 위에서 만든 'KeyCard_Red' SO를 여기에 드래그
    public InventoryObject playerInventory; // 'PlayerInventory' SO를 여기에 드래그

    [Header("이벤트 채널")]
    public TutorialEventChannel tutorialEventChannel;
    public ETutorialEvent OnPickUp_E;

    [Header("Audio Data")]
    [SerializeField] private WwiseAudioData pickupSound; // 아이템 획득 시 재생할 사운드
    [SerializeField] private WwiseAudioData ambientLoopSound; // 아이템 근처에 있을 때 지속적으로 재생될 사운드

    private WwiseSoundEmitter ambientEmitter; // 지속 사운드 재생을 위한 Emitter 참조

    private void Start()
    {
        // Debug.Log($"[ItemPickup Debug] Start called for {gameObject.name}.");

        // // 아이템이 씬에 나타날 때 지속적으로 재생될 사운드를 시작합니다.
        // if (ambientLoopSound == null)
        // {
        //     Debug.LogWarning($"[ItemPickup Debug] 'ambientLoopSound' is not assigned on {gameObject.name}. No ambient sound will be played.");
        //     return;
        // }

        // if (AudioManager.Instance == null)
        // {
        //     Debug.LogError($"[ItemPickup Debug] AudioManager.Instance is not available when Start() is called on {gameObject.name}. Check script execution order or if AudioManager exists in the scene.");
        //     return;
        // }

        // Debug.Log($"[ItemPickup Debug] Attempting to play attached ambient sound '{ambientLoopSound.name}' on {gameObject.name}.");
        // // PlayAttached는 이 게임오브젝트에 WwiseSoundEmitter를 추가/가져와서 사운드를 재생합니다.
        // ambientEmitter = AudioManager.Instance.PlayAttached(ambientLoopSound, gameObject);

        // if (ambientEmitter != null)
        // {
        //     Debug.Log($"[ItemPickup Debug] Successfully got or added a WwiseSoundEmitter on {gameObject.name}. Ambient sound should be playing.");
        // }
        // else
        // {
        //     Debug.LogError($"[ItemPickup Debug] Failed to get or add a WwiseSoundEmitter on {gameObject.name}. AudioManager.PlayAttached returned null. Check if '{ambientLoopSound.name}' (WwiseAudioData) has Wwise events assigned in its list.");
        // }

        StartCoroutine(SoundLoop());
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때, 혹시나 소리가 멈추지 않은 경우를 대비해 안전하게 멈춥니다.
        if (ambientEmitter != null)
        {
            ambientEmitter.Stop();
        }
    }

    // 외부(플레이어)에서 호출할 함수
    public void Pickup()
    {
        // 아이템을 줍기 전에 지속되던 사운드를 먼저 멈춥니다.
        if (ambientEmitter != null)
        {
            ambientEmitter.Stop();
        }

        playerInventory.AddItem(itemData, 1);

        if (tutorialEventChannel != null)
        {
            tutorialEventChannel.RaiseEvent(OnPickUp_E);
        }
        
        // 아이템 획득 사운드를 재생합니다.
        if (pickupSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(pickupSound, transform.position);
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

        Pickup();
    }

    private IEnumerator SoundLoop()
    {
        while(true){
            AudioManager.Instance.PlayOneShot(ambientLoopSound, transform.position);
            yield return new WaitForSeconds(2f);
        }

    }
}