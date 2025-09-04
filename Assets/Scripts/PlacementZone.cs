using UnityEngine;

/// <summary>
/// 포탑을 설치할 수 있는 바닥/구역.
/// - 설치 가능 여부 체크
/// - 설치 위치(anchor) 제공
/// - 설치되면 카운트 증가
/// - (선택) 운반용 포탑이 가까이 오면 하이라이트
/// </summary>
public class PlacementZone : MonoBehaviour
{
    [Header("Placement")]
    [SerializeField] private Transform anchor;        // 설치 기준점 (없으면 자기 transform)
    [SerializeField] private bool allowMultiple = false;
    [SerializeField, Min(1)] private int maxCount = 1;

    [Header("Visuals (Optional)")]
    [SerializeField] private GameObject highlightObj; // 설치 가능 시 표시용 오브젝트(Outline, Glow 등)

    private int placedCount = 0;

    private void Awake()
    {
        if (!anchor) anchor = transform;
        if (highlightObj) highlightObj.SetActive(false);
    }

    /// <summary>이 구역에 설치 가능한지?</summary>
    public bool CanPlace()
    {
        if (allowMultiple) return true;
        return placedCount < maxCount;
    }

    /// <summary>설치될 위치 반환</summary>
    public Vector3 GetPlacePosition(Vector3 fallback)
    {
        return anchor ? anchor.position : fallback;
    }

    /// <summary>포탑 설치 완료 처리</summary>
    public void MarkPlaced(GameObject placedTurret)
    {
        placedCount++;
        // 필요하면 설치된 포탑을 Zone 자식으로 관리
        // placedTurret.transform.SetParent(transform);

        if (highlightObj) highlightObj.SetActive(false);
    }

    // ===== (선택) 운반용 포탑이 가까이 올 때 하이라이트 표시 =====
    private void OnTriggerEnter(Collider other)
    {
         Debug.Log($"[PlacementZone] Enter: {other.name}");
        if (!highlightObj) return;
        if (other.GetComponent<CarriedTurret>())
            highlightObj.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!highlightObj) return;
        if (other.GetComponent<CarriedTurret>())
            highlightObj.SetActive(false);
    }
    
}
