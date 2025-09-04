using UnityEngine;
using System.Collections; // 코루틴(시간차 공격)을 사용하기 위해 필요합니다.

public class Enemy : MonoBehaviour
{
    // --- 인스펙터에서 직접 설정하는 변수들 ---
    [Header("능력치 설정")]
    [Tooltip("적의 기본 이동 속도입니다.")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("적이 초당 본체에 가하는 공격력입니다.")]
    [SerializeField] private int attackDamage = 10;
    
    // --- Public 프로퍼티 ---
    // 외부 스크립트(EnemyHealth)가 이 적의 속도를 안전하게 제어할 수 있도록 만들어둔 '공식 통로'입니다.
    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = value; }
    }

    // --- 내부적으로 관리되는 변수들 ---
    private bool hasReachedEnd = false; // 경로 끝에 도달했는지 확인하는 스위치입니다. true가 되면 이동을 멈춥니다.

    [Header("경로 설정")]
    [Tooltip("적이 따라갈 웨이포인트(경로)들. WaveManager가 자동으로 설정해줍니다.")]
    [HideInInspector] // 이 변수는 WaveManager가 코드로 제어하므로, 인스펙터에 노출시키지 않습니다.
    public Transform[] waypoints;
    private int currentWaypointIndex = 0; // 현재 목표로 하는 웨이포인트의 순번입니다.

    // Update 함수는 매 프레임마다 계속 실행됩니다.
    void Update()
    {
        // 만약 hasReachedEnd 스위치가 켜졌다면(경로 끝에 도달했다면), 더 이상 이동 코드를 실행하지 않고 즉시 함수를 종료합니다.
        if (hasReachedEnd) 
            return;

        // 마지막 웨이포인트까지 통과했는지 확인합니다.
        // currentWaypointIndex가 전체 웨이포인트 개수(waypoints.Length)와 같거나 커지면 모든 경로를 통과한 것입니다.
        if (waypoints == null || currentWaypointIndex >= waypoints.Length)
        {
            hasReachedEnd = true; // 도달 스위치를 켜서 다시는 이동 코드가 실행되지 않도록 합니다.
            StartCoroutine(AttackBase()); // 본체 공격 코루틴을 시작합니다.
            return; // 이동 로직을 중단합니다.
        }

        // 현재 위치에서 목표 웨이포인트 위치를 향해, MoveSpeed의 속도로 조금씩 이동시킵니다.
        // Time.deltaTime을 곱해주는 이유는 컴퓨터 성능과 상관없이 일정한 속도로 움직이게 하기 위함입니다.
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, MoveSpeed * Time.deltaTime);

        // 현재 위치와 목표 웨이포인트 사이의 거리가 0.1f 미만으로 매우 가까워졌다면 (도착했다면)
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
        {
            // 다음 웨이포인트를 목표로 설정하기 위해 순번을 1 증가시킵니다.
            currentWaypointIndex++;
            // 아직 다음 웨이포인트가 남아있다면
            if (currentWaypointIndex < waypoints.Length)
            {
                // 그 다음 웨이포인트를 바라보도록 적의 방향을 회전시킵니다.
                transform.LookAt(waypoints[currentWaypointIndex]);
            }
        }
    }

    // 본체를 반복적으로 공격하는 코루틴 함수입니다.
    IEnumerator AttackBase()
    {
        // 이 루프는 적이 죽기 전까지 무한 반복됩니다.
        while (true)
        {
            // 씬 어딘가에 MainBaseHealth의 '대표'(Instance)가 존재하는지 확인합니다.
            if (MainBaseHealth.Instance != null)
            {
                // 자신의 공격력(attackDamage)만큼 본체에 피해를 입힙니다.
                MainBaseHealth.Instance.TakeDamage(attackDamage);
            }
            
            // 1초 동안 기다렸다가 다시 루프의 처음으로 돌아가 공격을 반복합니다.
            yield return new WaitForSeconds(1f);
        }
    }
}