using UnityEngine;

public class Enemy : MonoBehaviour
{
    // EnemyHealth가 이 변수를 제어할 수 있도록 public으로 유지합니다.
    public float moveSpeed = 5f;

    // 외부(WaveSpawn)에서 웨이포인트 배열을 받을 변수
    [HideInInspector]
    public Transform[] waypoints;

    // 현재 목표 웨이포인트의 인덱스
    private int currentWaypointIndex = 0;

    void Setup(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentWaypointIndex = 0;

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("웨이포인트가 할당되지 않았습니다!", this.gameObject);
            enabled = false;
            return;
        }
        // 시작 위치를 첫 웨이포인트로 설정하고 다음 지점을 바라봅니다.
        transform.position = waypoints[0].position;
        if (waypoints.Length > 1)
        {
            transform.LookAt(waypoints[1]);
        }
    }

    void Update()
    {
        // 마지막 웨이포인트에 도달하면 소멸
        if (waypoints == null || currentWaypointIndex >= waypoints.Length) return;

        // 목표 웨이포인트를 향해 이동
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, moveSpeed * Time.deltaTime);

        // 목표 웨이포인트에 도착하면 다음 목표 설정
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex < waypoints.Length)
            {
                transform.LookAt(waypoints[currentWaypointIndex]);
            }
        }
    }

    // 다음 웨이포인트를 바라보게 하는 함수
    void LookAtNextWaypoint()
    {
        if (currentWaypointIndex < waypoints.Length)
        {
            transform.LookAt(waypoints[currentWaypointIndex]);
        }
    }

    // 최종 목적지에 도착했을 때 처리
    void ReachDestination()
    {
        // 여기에 본진 체력 감소 등의 로직을 추가할 수 있습니다.
        Destroy(gameObject);
    }
}