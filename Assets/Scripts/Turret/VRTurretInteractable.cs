using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// VR 환경에서 터렛이 선택되었음을 감지하는 스크립트입니다.
/// 이 스크립트는 기존의 TurretSelector.cs를 완벽하게 대체합니다.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Upgradeable))]
public class VRTurretInteractable : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;
    private Upgradeable upgradeable;
    private TurretUIController turretUIController;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        upgradeable = GetComponent<Upgradeable>();
        
        // 씬에 있는 TurretUIController를 찾아 연결합니다.
        turretUIController = FindFirstObjectByType<TurretUIController>();
    }

    void OnEnable()
    {
        // VR 컨트롤러가 이 터렛을 '선택'했을 때 OnSelect_Entered 함수를 호출하도록 이벤트를 구독합니다.
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelect_Entered);
        }
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelect_Entered);
        }
    }

    /// <summary>
    /// VR 컨트롤러의 레이저에 의해 이 터렛이 선택되었을 때 호출되는 함수입니다.
    /// </summary>
    private void OnSelect_Entered(SelectEnterEventArgs args)
    {
        if (turretUIController != null && upgradeable != null)
        {
            Debug.Log($"VR 터렛 선택됨: {gameObject.name}");
            // TurretUIController에게 이 터렛이 선택되었다고 알립니다.
            turretUIController.SelectTurret(upgradeable);
        }
    }
}