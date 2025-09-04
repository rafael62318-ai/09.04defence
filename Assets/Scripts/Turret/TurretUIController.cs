using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TurretUIController : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    [SerializeField] private UnitPlacer placer; // 터렛 설치 로직을 담당하는 컴포넌트
    [SerializeField] private GameObject upgradePanel;

    [Header("VR 상호작용 설정")]
    [Tooltip("터렛 설치/UI 조작에 사용할 VR 컨트롤러의 Ray Interactor")]
    [SerializeField] private XRRayInteractor rayInteractor;

    [Header("미리보기 설정")]
    [Tooltip("미리보기 터렛에 적용할 반투명 머티리얼")]
    [SerializeField] private Material previewMaterial;

    private GameObject turretPrefabToPlace;
    private GameObject turretPreviewInstance; //미리보기 터렛을 담을 변수

    // 현재 선택되어 업그레이드/판매 등을 할 수 있는 터렛
    private Upgradeable selectedTurret;
  

    void Start()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    public void PrepareToPlaceTurret(GameObject turretPrefab)
    {
        // 이전에 선택한 미리보기가 있다면 삭제
        if (turretPreviewInstance != null)
        {
            Destroy(turretPreviewInstance);
        }

        turretPrefabToPlace = turretPrefab;
        
        // 미리보기 인스턴스 생성
        turretPreviewInstance = Instantiate(turretPrefab);
        
        // 미리보기가 실제로 공격하지 못하도록 기능적인 스크립트들을 비활성화
        if(turretPreviewInstance.GetComponent<TurretController>() != null)
            turretPreviewInstance.GetComponent<TurretController>().enabled = false;
        if(turretPreviewInstance.GetComponent<Collider>() != null)
            turretPreviewInstance.GetComponent<Collider>().enabled = false;
        
        // 모든 렌더러를 찾아 반투명 머티리얼로 교체 (선택 사항이지만 추천)
        if (previewMaterial != null)
        {
            foreach (var renderer in turretPreviewInstance.GetComponentsInChildren<Renderer>())
            {
                renderer.material = previewMaterial;
            }
        }

        Debug.Log($"{turretPrefab.name} 설치 준비 완료.");
    }


    public void ClearPlacement()
    {
        if (turretPrefabToPlace != null)
        {
            Destroy(turretPreviewInstance);
        }
        turretPreviewInstance = null;
        turretPrefabToPlace = null;
    }

    void Update()
    {
        if (turretPrefabToPlace == null || turretPreviewInstance == null) return;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            turretPreviewInstance.transform.position = hit.point;

            if (rayInteractor.uiPressInput.ReadWasPerformedThisFrame())
            {
                TurretSlot slot = hit.collider.GetComponent<TurretSlot>();

                if (slot != null && placer != null)
                {
                    bool placed = placer.TryPlaceOnSlot(turretPrefabToPlace, slot);

                    if (placed)
                    {
                        Debug.Log($"{turretPrefabToPlace.name} 포탑을 슬롯에 설치 성공");
                        ClearPlacement();
                    }
                    else
                    {
                        Debug.Log("포탑 설치 실패(골드 부족 or 이미 점유된 슬롯)");
                    }
                }
            }
        }
    }

    
    public void SelectTurret(Upgradeable turret)
    {
        selectedTurret = turret;

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
        }
    }

    /// <summary>
    /// 업그레이드 UI 버튼에 연결되는 함수
    /// </summary>
    public void UpgradeSelectedTurret()
    {
        if (selectedTurret != null)
        {
            selectedTurret.TryUpgrade();
        }
    }
}