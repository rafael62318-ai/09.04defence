using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // 정렬(OrderBy) 기능을 위해 추가합니다.

public class TurretController : MonoBehaviour
{
    public enum TurretType { Basic, LongRange, ShortRange, Slow }
    public enum TargetingType { Nearest, Random, LowestHP }

    [Header("Turret Settings")]
    public TurretType turretType = TurretType.Basic;
    public TargetingType targetingType = TargetingType.Nearest;

    [Header("Component References")]
    public Transform lookAtObj;
    public Transform shootElement;
    
    [Header("Game Logic")]
    public string targetTag = "Enemy";
    public float rotationSpeed = 5f;
    public GameObject projectilePrefab;
    
    // 이 변수들은 이제 인스펙터에서 직접 수정할 수 있습니다.
    public float range = 10f;
    public float fireRate = 1f;
    public int damage = 10;
    public float abilityValue = 0f;

    // TurretTrigger를 사용할지 여부를 결정하는 변수
    [Header("Trigger Settings")]
    public bool useTrigger = false;

    private List<Transform> targets = new List<Transform>();
    private float homeY;
    private bool isShooting;
    private float shootDelay;
    
    void Start()
    {
        if (lookAtObj != null)
            homeY = lookAtObj.localRotation.eulerAngles.y;
        
        shootDelay = 1f / Mathf.Max(0.0001f, fireRate);
    }
    
    void Update()
    {
        // useTrigger가 false일 때만 스스로 타겟을 찾습니다.
        if (!useTrigger)
        {
            FindTargets();
        }

        // 리스트에 있는 타겟 중 파괴된(null) 것을 자동으로 제거합니다.
        targets.RemoveAll(item => item == null);

        if (targets.Count > 0)
        {
            AimAtTarget(targets[0]); 
            
            if (!isShooting)
            {
                StartCoroutine(ShootCoroutine());
            }
        }
        else
        {
            isShooting = false;
            ReturnToHomeRotation();
        }
    }

    // 외부(TurretTrigger)에서 타겟을 추가하는 함수
    public void AddTarget(Transform newTarget)
    {
        if (!targets.Contains(newTarget))
        {
            targets.Add(newTarget);
            SortTargets(); // 새 타겟이 추가됐으니 우선순위에 따라 정렬
        }
    }

    // 외부(TurretTrigger)에서 타겟을 제거하는 함수
    public void RemoveTarget(Transform targetToRemove)
    {
        targets.Remove(targetToRemove);
    }
    
    public Transform GetCurrentTarget()
    {
        if (targets.Count > 0)
        {
            return targets[0];
        }
        return null;
    }

    void FindTargets()
    {
        targets.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(targetTag))
            {
                targets.Add(hitCollider.transform);
            }
        }
        SortTargets(); // 타겟을 찾은 후 우선순위에 따라 정렬
    }

    void SortTargets()
    {
        switch (targetingType)
        {
            case TargetingType.Nearest:
                targets = targets.OrderBy(t => Vector3.Distance(transform.position, t.position)).ToList();
                break;
            case TargetingType.LowestHP:
                targets = targets.OrderBy(t => t.GetComponent<EnemyHealth>()?.GetCurrentHP() ?? int.MaxValue).ToList();
                break;
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

    public void SetTurretStats()
    {
        switch (turretType)
        {
            case TurretType.Basic:
                range = 10f;
                fireRate = 1f / 2f;
                damage = 10;
                break;
            case TurretType.LongRange:
                range = 15f;
                fireRate = 1f / 3f;
                damage = 6;
                break;
            case TurretType.ShortRange:
                range = 8f;
                fireRate = 1f / 3f;
                damage = 15;
                break;
            case TurretType.Slow:
                range = 10f;
                fireRate = 1f / 10f;
                damage = 2;
                abilityValue = 0.5f;
                break;
        }
    }
    
    int GetMaxTargets()
    {
        switch (turretType)
        {
            case TurretType.Basic:
                return 1;
            case TurretType.LongRange:
            case TurretType.ShortRange:
                return 3;
            case TurretType.Slow:
                return 3;
        }
        return 1;
    }
    
    void AimAtTarget(Transform t)
    {
        if (lookAtObj == null || t == null) return;
        Vector3 direction = t.position - lookAtObj.position;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookAtObj.rotation = Quaternion.Slerp(lookAtObj.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    
    void ReturnToHomeRotation()
    {
        if (lookAtObj == null) return;
        Quaternion home = Quaternion.Euler(lookAtObj.localRotation.eulerAngles.x, homeY, lookAtObj.localRotation.eulerAngles.z);
        lookAtObj.rotation = Quaternion.Slerp(lookAtObj.rotation, home, Time.deltaTime * rotationSpeed);
    }
    
    IEnumerator ShootCoroutine()
    {
        isShooting = true;
        while (targets.Count > 0)
        {
            AimAtTarget(targets[0]);

            // 공격 시점에 타겟 리스트를 복사하여 안전하게 사용
            List<Transform> currentTargets = new List<Transform>(targets);
            int targetsToAttack = GetMaxTargets();
            
            for (int i = 0; i < Mathf.Min(targetsToAttack, currentTargets.Count); i++)
            {
                Transform t = currentTargets[i];
                if (t != null && projectilePrefab != null && shootElement != null)
                {
                    GameObject newBullet = Instantiate(projectilePrefab, shootElement.position, shootElement.rotation);
                    var projectileScript = newBullet.GetComponent<Projectile>();
                    if (projectileScript != null)
                    {
                        projectileScript.damage = damage;
                        projectileScript.slowAmount = (turretType == TurretType.Slow) ? abilityValue : 0;
                        projectileScript.target = t;
                    }
                }
            }
            
            yield return new WaitForSeconds(shootDelay);
        }
        isShooting = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}