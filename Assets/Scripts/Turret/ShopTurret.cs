using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ShopTurret : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("이 견본을 집었을 때 실제로 설치될 터렛의 프리팹")]
    public GameObject realTurretPrefab;

    private TurretUIController placementController;
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        placementController = FindFirstObjectByType<TurretUIController>();
        if (placementController == null)
        {
            Debug.LogError("씬에 TurretUIController가 없습니다!");
        }
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.RemoveListener(StartPlacing);
    }

    private void StartPlacing(SelectEnterEventArgs args)
    {
        if (placementController != null)
        {
            placementController.PrepareToPlaceTurret(realTurretPrefab);
        }
    }
}
