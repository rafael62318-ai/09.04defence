using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class CarriedTurret : MonoBehaviour
{
    [Header("Placement")]
    [SerializeField] private LayerMask placementMask;      // 설치 구역 레이어
    [SerializeField, Min(0.05f)] private float checkRadius = 0.4f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private TurretDefinition def;
    private ShopItem fromShop;
    private bool installed = false;

    public void InitFromShop(ShopItem shop, TurretDefinition definition)
    {
        fromShop = shop;
        def = definition;
    }

    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectExited.AddListener(OnReleased);
    }

    private void OnDestroy()
    {
        if (grab) grab.selectExited.RemoveListener(OnReleased);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (installed) return;

        var zone = FindPlacementZone();
        if (zone != null && zone.CanPlace())
        {
            Vector3 pos = zone.GetPlacePosition(transform.position) + def.placeOffset;
            Quaternion rot = Quaternion.Euler(def.placeRotationEuler);

            var placed = Instantiate(def.placedTurretPrefab, pos, rot);
            zone.MarkPlaced(placed);

            installed = true;
            Destroy(gameObject);
        }
        else
        {
            // 설치 실패 → 환불 후 파기
            if (fromShop != null) fromShop.Refund();
            Destroy(gameObject);
        }
    }

    private PlacementZone FindPlacementZone()
    {
        // 설치 구역 탐색(트리거 포함)
        var hits = Physics.OverlapSphere(transform.position, checkRadius, placementMask, QueryTriggerInteraction.Collide);
        foreach (var h in hits)
        {
            var z = h.GetComponentInParent<PlacementZone>();
            if (z != null) return z;
        }
        return null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, checkRadius);
    }
#endif
}
