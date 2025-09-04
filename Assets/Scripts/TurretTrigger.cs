using UnityEngine;

[RequireComponent(typeof(Collider))] // 이 스크립트는 Collider가 꼭 필요함을 명시합니다.
public class TurretTrigger : MonoBehaviour
{
    // 연결할 메인 터렛 컨트롤러
    public TurretController turretController;

    void Start()
    {
        // 이 컴포넌트의 Collider를 반드시 Trigger로 설정합니다.
        GetComponent<Collider>().isTrigger = true;
        
        // turretController가 Inspector 창에서 할당되지 않았다면 경고 메시지를 출력합니다.
        if (turretController == null)
        {
            Debug.LogError("TurretController가 TurretTrigger에 할당되지 않았습니다!", this.gameObject);
        }
    }

    // 물체가 트리거 안으로 들어왔을 때 호출됩니다.
    void OnTriggerEnter(Collider other)
    {
        // turretController가 있고 들어온 물체의 태그가 일치하면,
        if (turretController != null && other.CompareTag(turretController.targetTag))
        {
            // TurretController에게 타겟 추가를 '요청'합니다.
            turretController.AddTarget(other.transform);
        }
    }

    // 물체가 트리거 밖으로 나갔을 때 호출됩니다.
    void OnTriggerExit(Collider other)
    {
        // turretController가 있고 나간 물체의 태그가 일치하면,
        if (turretController != null && other.CompareTag(turretController.targetTag))
        {
            // TurretController에게 타겟 제거를 '요청'합니다.
            turretController.RemoveTarget(other.transform);
        }
    }
}