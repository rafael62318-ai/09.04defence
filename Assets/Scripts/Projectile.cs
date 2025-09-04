using UnityEngine;

public class Projectile : MonoBehaviour
{
    // TurretController가 설정해주는 변수들
    public Transform target;
    public int damage;
    public float slowAmount; // 0이면 둔화 없음, 0.5는 50% 둔화

    [Header("총알 설정")]
    public float speed = 20f;
    public float slowDuration = 2f; // 둔화 지속시간

    void Update()
    {
        if (target != null)
        {
            // 타겟을 향해 이동
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // 타겟을 바라보도록 총알 회전
            transform.LookAt(target);
        }
        else
        {
            // 타겟이 사라지면 총알도 파괴
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 충돌한 대상이 나의 타겟이 맞는지 확인
        if (other.transform == target)
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);

                if (slowAmount > 0)
                {
                    enemyHealth.ApplySlow(slowAmount, slowDuration);
                }
            }

            Destroy(gameObject);
        }
    }
}