using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // OrderBy (정렬) 기능을 사용하기 위해 필요합니다.

public class TurretController : MonoBehaviour
{
    // --- 터렛의 종류와 타겟팅 방식을 인스펙터에서 선택할 수 있도록 Enum으로 정의합니다 ---

    // TurretType은 SetTurretStats() 함수에서 각기 다른 능력치를 설정하는 기준이 됩니다.
    public enum TurretType { Basic, LongRange, ShortRange, Slow }
    // TargetingType은 SortTargets() 함수에서 어떤 적을 먼저 공격할지 결정하는 기준이 됩니다.
    public enum TargetingType { Nearest, Random, LowestHP }

    [Header("터렛 기본 설정")]
    public TurretType turretType = TurretType.Basic;
    public TargetingType targetingType = TargetingType.Nearest;

    [Header("필수 컴포넌트 연결")]
    [Tooltip("실제로 회전할 터렛의 포신이나 상체 부분입니다.")]
    public Transform lookAtObj;
    [Tooltip("발사체가 생성될 발사구 위치입니다.")]
    public Transform shootElement;
    
    [Header("게임 로직 설정")]
    [Tooltip("공격할 대상의 태그입니다.")]
    public string targetTag = "Enemy";
    [Tooltip("적을 향해 회전하는 속도입니다.")]
    public float rotationSpeed = 5f;
    [Tooltip("발사할 발사체의 프리팹입니다.")]
    public GameObject projectilePrefab;
    
    [Header("터렛 능력치 (인스펙터에서 직접 수정 가능)")]
    // 이 값들은 Start()에서 SetTurretStats()가 호출되면 타입에 맞는 값으로 덮어씌워질 수 있습니다.
    // 또는 SetTurretStats()를 사용하지 않고 이 값을 직접 수정하여 커스텀 터렛을 만들 수도 있습니다.
    public float range = 10f;
    public float fireRate = 1f;
    public int damage = 10;
    public float abilityValue = 0f; // 슬로우 터렛의 감속 수치 등 특수 능력에 사용됩니다.

    [Header("타겟 탐지 방식 설정")]
    [Tooltip("체크하면 TurretTrigger 스크립트를 통해 타겟을 받고, 체크 해제하면 스스로 주변의 적을 탐지합니다.")]
    public bool useTrigger = false;

    // --- 내부적으로 관리되는 변수들 ---
    private List<Transform> targets = new List<Transform>(); // 현재 사정거리 내에 있는 적들의 목록입니다.
    private float homeY; // 터렛이 바라보는 기본 Y축 회전 값입니다. (적이 없을 때 돌아갈 위치)
    private bool isShooting; // 현재 발사 코루틴이 실행 중인지 확인하는 스위치 (중복 실행 방지)
    private float shootDelay; // 발사 속도(fireRate)를 기반으로 계산된 실제 발사 간격(초)입니다.
    
    // 게임 오브젝트가 시작될 때 한번 호출됩니다.
    void Start()
    {
        // 적이 없을 때 돌아갈 기본 방향을 저장합니다.
        if (lookAtObj != null)
            homeY = lookAtObj.localRotation.eulerAngles.y;
        
        // 초당 발사 횟수(fireRate)를 실제 발사 딜레이 시간(shootDelay)으로 변환합니다.
        // 예: fireRate가 2이면, 1초에 2번 쏘므로 딜레이는 0.5초가 됩니다.
        shootDelay = 1f / Mathf.Max(0.0001f, fireRate);

        // 터렛 타입에 맞는 기본 능력치를 설정합니다. (선택 사항)
        // SetTurretStats(); 
    }
    
    // 매 프레임마다 호출됩니다.
    void Update()
    {
        // useTrigger가 false일 때만 (스스로 탐지하는 방식일 때만) 매 프레임 적을 찾습니다.
        if (!useTrigger)
        {
            FindTargets();
        }

        // targets 리스트에서 파괴된 (null이 된) 적들을 자동으로 제거합니다.
        targets.RemoveAll(item => item == null);

        // 공격할 타겟이 1명 이상 있다면
        if (targets.Count > 0)
        {
            // 정렬된 목록의 첫 번째 적을 조준합니다.
            AimAtTarget(targets[0]); 
            
            // 발사 코루틴이 실행 중이 아니라면, 발사를 시작합니다.
            if (!isShooting)
            {
                StartCoroutine(ShootCoroutine());
            }
        }
        else // 공격할 타겟이 없다면
        {
            isShooting = false; // 발사 상태를 해제합니다.
            ReturnToHomeRotation(); // 기본 방향으로 돌아갑니다.
        }
    }

    // TurretTrigger 스크립트가 호출하여 타겟 리스트에 적을 추가하는 함수입니다.
    public void AddTarget(Transform newTarget)
    {
        if (!targets.Contains(newTarget))
        {
            targets.Add(newTarget);
            SortTargets(); // 새 타겟이 추가됐으니 우선순위에 따라 다시 정렬합니다.
        }
    }

    // TurretTrigger 스크립트가 호출하여 타겟 리스트에서 적을 제거하는 함수입니다.
    public void RemoveTarget(Transform targetToRemove)
    {
        targets.Remove(targetToRemove);
    }
    
    // 현재 조준 중인 타겟을 외부에서 알 수 있도록 반환해주는 함수입니다.
    public Transform GetCurrentTarget()
    {
        if (targets.Count > 0)
            return targets[0];
        
        return null;
    }

    // 터렛의 사정거리(range) 내에 있는 모든 적을 찾아 리스트에 추가하는 함수입니다.
    void FindTargets()
    {
        targets.Clear(); // 이전 목록을 초기화합니다.
        // 터렛 위치를 중심으로 range만큼의 반경 안에 들어온 모든 Collider를 찾습니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        foreach (var hitCollider in hitColliders)
        {
            // 찾은 Collider의 태그가 우리가 설정한 targetTag와 일치하면
            if (hitCollider.CompareTag(targetTag))
            {
                // targets 리스트에 추가합니다.
                targets.Add(hitCollider.transform);
            }
        }
        SortTargets(); // 찾은 타겟들을 우선순위에 따라 정렬합니다.
    }

    // targetingType에 따라 targets 리스트를 정렬하는 함수입니다.
    void SortTargets()
    {
        switch (targetingType)
        {
            // 가장 가까운 순서로 정렬합니다.
            case TargetingType.Nearest:
                targets = targets.OrderBy(t => Vector3.Distance(transform.position, t.position)).ToList();
                break;
            // 체력이 가장 낮은 순서로 정렬합니다.
            case TargetingType.LowestHP:
                // ?? 연산자: GetComponent<EnemyHealth>()가 null이면 int.MaxValue(엄청 큰 값)를 반환하여 null인 대상을 맨 뒤로 보냅니다.
                targets = targets.OrderBy(t => t.GetComponent<EnemyHealth>()?.GetCurrentHP() ?? int.MaxValue).ToList();
                break;
            // 리스트를 무작위로 섞습니다. (Fisher-Yates 알고리즘)
            case TargetingType.Random:
                int n = targets.Count;
                while (n > 1)
                {
                    n--;
                    int k = Random.Range(0, n + 1);
                    Transform value = targets[k];
                    targets[k] = targets[n];
                    targets[n] = value;
                }
                break;
        }
    }

    // 터렛 타입에 따라 기본 능력치를 설정해주는 함수입니다. (주로 초기 설정용)
    public void SetTurretStats()
    {
        switch (turretType)
        {
            case TurretType.Basic:
                range = 10f; fireRate = 0.5f; damage = 10;
                break;
            case TurretType.LongRange:
                range = 15f; fireRate = 1f/3f; damage = 6;
                break;
            case TurretType.ShortRange:
                range = 8f; fireRate = 1f/3f; damage = 15;
                break;
            case TurretType.Slow:
                range = 10f; fireRate = 0.1f; damage = 2; abilityValue = 0.5f;
                break;
        }
    }
    
    // 터렛 타입에 따라 동시에 공격할 수 있는 최대 타겟 수를 반환하는 함수입니다.
    int GetMaxTargets()
    {
        switch (turretType)
        {
            case TurretType.Basic:
                return 1;
            case TurretType.LongRange:
            case TurretType.ShortRange:
            case TurretType.Slow:
                return 3; // 기본 터렛 외에는 모두 3명 동시 공격
        }
        return 1;
    }
    
    // 타겟을 향해 터렛을 회전시키는 함수입니다. (우리가 함께 수정한 최종 버전)
    void AimAtTarget(Transform t)
    {
        if (lookAtObj == null || t == null)
            return;

        // 시작점과 끝점을 바꿔서 방향을 180도 뒤집습니다. (모델이 반대로 되어있는 문제 해결)
        Vector3 direction = lookAtObj.position - t.position;

        // Y축(상하) 회전을 막아서 수평으로만 회전하도록 고정합니다.
        direction.y = 0;

        // 계산된 방향으로 바라보는 회전 값을 구합니다.
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        
        // 현재 회전 값에서 목표 회전 값으로 부드럽게 회전시킵니다.
        lookAtObj.rotation = Quaternion.Slerp(lookAtObj.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    
    // 적이 없을 때 터렛을 원래 방향으로 되돌리는 함수입니다.
    void ReturnToHomeRotation()
    {
        if (lookAtObj == null) return;
        Quaternion home = Quaternion.Euler(lookAtObj.localRotation.eulerAngles.x, homeY, lookAtObj.localRotation.eulerAngles.z);
        lookAtObj.rotation = Quaternion.Slerp(lookAtObj.rotation, home, Time.deltaTime * rotationSpeed);
    }
    
    // 실제로 발사체를 생성하고 발사 딜레이를 관리하는 코루틴 함수입니다.
    IEnumerator ShootCoroutine()
    {
        isShooting = true; // 발사 시작 스위치를 켭니다.
        // 사정거리 내에 타겟이 있는 동안 계속 반복합니다.
        while (targets.Count > 0)
        {
            // 첫 번째 타겟을 계속 조준합니다.
            AimAtTarget(targets[0]);

            // 동시에 공격할 타겟 리스트를 안전하게 복사합니다.
            List<Transform> currentTargets = new List<Transform>(targets);
            int targetsToAttack = GetMaxTargets();
            
            // 동시에 공격할 수 있는 최대 타겟 수만큼 반복하여 발사체를 생성합니다.
            for (int i = 0; i < Mathf.Min(targetsToAttack, currentTargets.Count); i++)
            {
                Transform t = currentTargets[i];
                if (t != null && projectilePrefab != null && shootElement != null)
                {
                    // 발사구(shootElement)의 위치와 방향으로 발사체 프리팹을 복제(생성)합니다.
                    GameObject newBullet = Instantiate(projectilePrefab, shootElement.position, shootElement.rotation);
                    var projectileScript = newBullet.GetComponent<Projectile>();
                    if (projectileScript != null)
                    {
                        // 생성된 발사체에게 능력치와 타겟 정보를 전달합니다.
                        projectileScript.damage = damage;
                        projectileScript.slowAmount = (turretType == TurretType.Slow) ? abilityValue : 0;
                        projectileScript.target = t;
                    }
                }
            }
            
            // 설정된 발사 딜레이(shootDelay)만큼 기다립니다.
            yield return new WaitForSeconds(shootDelay);
        }
        isShooting = false; // 타겟이 모두 사라지면 발사 종료 스위치를 끕니다.
    }

    // 유니티 에디터의 씬(Scene) 뷰에서만 보이는 터렛의 사정거리(range)를 시각적으로 표시합니다.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}