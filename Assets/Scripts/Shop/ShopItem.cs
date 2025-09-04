using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

/// <summary>
/// 상점에 전시되는 아이템. 
/// Hover 시 UI 표시, Grab 시 결제 → 운반용 프리팹(CarriedTurret) 생성.
/// 전시용은 XRSimpleInteractable, 운반용은 XRGrabInteractable로 설정 가능.
/// </summary>
public class ShopItem : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TurretDefinition turret;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPointForCarry; // 비워두면 손 위치/자기 위치 사용

    [Header("UI (Optional)")]
    [SerializeField] private ShopHoverUI hoverUI;
    [SerializeField] private GameObject toastNotEnoughGold;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip buyClip;
    [SerializeField] private AudioClip denyClip;

    // XRBaseInteractable은 Grab/Simple 둘 다 상속받음
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        if (interactable == null)
        {
            Debug.LogError("[ShopItem] XRBaseInteractable 계열 컴포넌트가 필요합니다. (XRSimpleInteractable 또는 XRGrabInteractable)");
            return;
        }

        // Hover 이벤트
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);

        // Select(집기) 이벤트
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDestroy()
    {
        if (interactable == null) return;

        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnEnable()
    {
        if (hoverUI != null) hoverUI.Bind(turret);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (hoverUI != null)
        {
            hoverUI.Bind(turret);
            hoverUI.Show();
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (hoverUI != null) hoverUI.Hide();
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!ResourceManager.Instance.TrySpend(turret.price))
        {
            if (sfx && denyClip) sfx.PlayOneShot(denyClip);
            SendHaptic(args.interactorObject, 0.4f, 0.15f);
            ShowToastOnce(toastNotEnoughGold, 1.0f);

            if (hoverUI != null) hoverUI.Refresh();
            return;
        }

        if (sfx && buyClip) sfx.PlayOneShot(buyClip);

        // 운반용 프리팹 생성
        var interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;

        Transform hand = null;
        if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor direct) hand = direct.transform;
        else if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ray) hand = ray.transform;

        Vector3 pos = spawnPointForCarry ? spawnPointForCarry.position :
                      hand ? hand.position : (transform.position + Vector3.up * 0.1f);
        Quaternion rot = spawnPointForCarry ? spawnPointForCarry.rotation :
                         hand ? hand.rotation : Quaternion.identity;

        var proxy = Instantiate(turret.carriedPrefab, pos, rot);
        var carried = proxy.GetComponent<CarriedTurret>();
        if (carried == null) carried = proxy.AddComponent<CarriedTurret>();
        carried.InitFromShop(this, turret);

        // 손에 강제 그랩
        var grab = proxy.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab != null && interactor != null && grab.interactionManager != null)
        {
            grab.interactionManager.SelectEnter(
                interactor as UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor,
                grab as UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable
            );
        }

        if (hoverUI != null) hoverUI.Refresh();
    }

    public void Refund()
    {
        ResourceManager.Instance.Add(turret.price);
        if (hoverUI != null) hoverUI.Refresh();
    }

    private async void ShowToastOnce(GameObject go, float secs)
    {
        if (!go) return;
        go.SetActive(true);
        await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(secs * 1000));
        if (go) go.SetActive(false);
    }

    private void SendHaptic(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor, float amplitude, float duration)
    {
        try
        {
            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor di && di.xrController != null)
                di.xrController.SendHapticImpulse(amplitude, duration);
            else if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ri && ri.xrController != null)
                ri.xrController.SendHapticImpulse(amplitude, duration);
        }
        catch { }
    }
}
