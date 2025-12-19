using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Drone : MonoBehaviour
{
    [Header("Tutorial Event Channel")]
    [SerializeField] private TutorialEventChannel tutorialEventChannel;
    [SerializeField] private ETutorialEvent OnPlayerAimingAtDrone_E;
    private Rigidbody rb;

    [Header("Position Offset")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -3);

    [Header("Movement Settings")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private float wallDistance = 1.5f; // Distance to keep from walls





    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    public void Update()
    {
        transform.LookAt(playerTransform);
    }

    public void FixedUpdate()
    {
        HandleMovement();
        IsPlayerAimingAtDrone();
    }

    private void HandleMovement()
    {
        // 1. Define the ideal target position based on the player's orientation and offset.
        Vector3 idealPosition = playerTransform.position + offset;
        
        // 2. Check for obstacles. Cast a ray from the player towards the drone's ideal position.
        Vector3 directionFromPlayer = idealPosition - playerTransform.position;
        Ray ray = new Ray(playerTransform.position, directionFromPlayer.normalized);
        
        Vector3 targetPosition = idealPosition;

        // If the ray hits an obstacle within the desired offset distance...
        if (Physics.Raycast(ray, out RaycastHit hit, directionFromPlayer.magnitude, obstacleLayerMask))
        {
            // ...set the new target position to be slightly in front of the obstacle.
            targetPosition = hit.point - directionFromPlayer.normalized * wallDistance;
        }

        Vector3 dir = (targetPosition-transform.position).normalized;

        // Idle 상태
        if(Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            return;
        }

        rb.MovePosition(transform.position + dir * followSpeed * Time.deltaTime);
    }

    // 플레이어가 드론을 바라보는지 확인하는 함수
    public bool IsPlayerAimingAtDrone()
    {
        // 플레이어의 정면 방향 벡터
        Vector3 playerForward = playerTransform.forward;
        // 플레이어에서 드론으로 향하는 방향 벡터
        Vector3 directionToDrone = transform.position - playerTransform.position;

        // Y축(높이) 차이를 무시하기 위해 두 벡터를 XZ 평면에 투영합니다.
        playerForward.y = 0;
        directionToDrone.y = 0;

        // 두 벡터 사이의 각도를 계산합니다.
        float angle = Vector3.Angle(playerForward, directionToDrone);

        // 각도가 5도 미만이면 플레이어가 드론을 바라보는 것으로 간주합니다.
        if(angle < 5f)
        {
            tutorialEventChannel.RaiseEvent(OnPlayerAimingAtDrone_E);
            return true;
        }

        return false;
    }
}
