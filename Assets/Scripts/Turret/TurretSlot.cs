using UnityEngine;
// 터렛을 설치할 수 있는 전용 공간(슬롯)의 상태를 관리
public class TurretSlot : MonoBehaviour
{
    // 이 슬롯이 현재 비어있는지 여부
    public bool isOccupied { get; private set; } = false;


    // 이 슬롯에 터렛을 배치합니다.
  
    public void PlaceTurret()
    {
        isOccupied = true;
    }

    // 터렛이 판매되었을 때 이 슬롯을 다시 비웁니다. (추후 확장용)
   
    public void ClearSlot()
    {
        isOccupied = false;
    }
}
