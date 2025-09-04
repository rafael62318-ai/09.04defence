using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Turret Definition")]
public class TurretDefinition : ScriptableObject
{
    public string displayName;
    [Min(0)] public int price;

    public Sprite icon;                  // UI/RenderTexture용
    public GameObject placedTurretPrefab; // 실제 설치되어 작동할 프리팹
    public GameObject carriedPrefab;      // 손에 쥐는 운반용 프리팹(Proxy)
    public GameObject ghostPrefab;        // 배치 미리보기(선택) - 없어도 됨

    [Header("Placement")]
    public Vector3 placeOffset;          // 설치 시 오프셋(바닥 높이 보정)
    public Vector3 placeRotationEuler;   // 설치 기본 회전
}
