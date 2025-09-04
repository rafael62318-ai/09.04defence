using UnityEngine;

public class UnitPlacer : MonoBehaviour
{
    [SerializeField] private LayerMask placeableLayers;
   

    /// <summary>
    /// 지정된 위치에 터렛 설치를 시도합니다.
    /// </summary>
    /// <param name="turretPrefab">설치할 터렛의 프리팹</param>
    /// <param name="rayOrigin">레이캐스트 시작 위치</param>
    /// <param name="rayDir">레이캐스트 방향</param>
    /// <returns>설치 성공 여부</returns>
    public bool TryPlaceOnSlot(GameObject turretPrefab, TurretSlot slot)
    {
        // 1. 프리팹이 유효한지 확인
        if (turretPrefab == null || slot == null || slot.isOccupied) return false;

        // 2. 프리팹에서 비용 정보를 가져옴
        var costComponent = turretPrefab.GetComponent<PurchaseCost>();
        if (costComponent == null) return false;
        int buildCost = costComponent.buildCost;
        

        // 3. 비용을 지불할 수 있는지 확인
        if (ResourceManager.Instance == null || !ResourceManager.Instance.CanAfford(buildCost))
        {
            return false;
        }
            
        
        // 5. 터렛 생성 및 비용 차감
        Instantiate(turretPrefab, slot.transform.position, slot.transform.rotation); 
        ResourceManager.Instance.TrySpend(buildCost);
        slot.PlaceTurret(); // 슬롯을 점유 상태로 만듭니다.
        return true;
    }


}