using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopHoverUI : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject rootPanel; // 전체 패널 루트(켜기/끄기)

    [Header("Colors")]
    [SerializeField] private Color canColor = new Color(0.45f, 1f, 0.45f, 1f);
    [SerializeField] private Color noColor = new Color(1f, 0.45f, 0.45f, 1f);

    private TurretDefinition boundDef;

    private void Awake()
    {
        if (!rootPanel) rootPanel = gameObject; // 지정 안했으면 자기 자신
        HideImmediate();
    }

    private void OnEnable()
    {
        ResourceManager.Instance.OnGoldChanged += OnGoldChanged;
    }

    private void OnDisable()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    public void Bind(TurretDefinition def)
    {
        boundDef = def;
        Refresh();
    }

    private void OnGoldChanged(int currentGold)
    {
        // 보유 골드 UI만 갱신해도 되지만 상태까지 갱신
        Refresh();
    }

    public void Refresh()
    {
        if (!boundDef) return;

        if (nameText)  nameText.text  = boundDef.displayName;
        if (priceText) priceText.text = $"{boundDef.price} G";
        if (goldText)  goldText.text  = $"{ResourceManager.Instance.Gold} G";
        if (iconImage) iconImage.sprite = boundDef.icon;

        bool can = ResourceManager.Instance.CanAfford(boundDef.price);
        if (statusText)
        {
            statusText.text  = can ? "구매 가능" : "골드 부족";
            statusText.color = can ? canColor : noColor;
        }
    }

    public void Show()
    {
        if (rootPanel) rootPanel.SetActive(true);
        // 필요 시 간단한 페이드/스케일 애니메이션 추가 가능
        Refresh();
    }

    public void Hide()
    {
        if (!rootPanel) return;
        // 간단한 페이드아웃이 필요하면 코루틴/Animator 연계
        rootPanel.SetActive(false);
    }

    private void HideImmediate()
    {
        if (rootPanel) rootPanel.SetActive(false);
    }
}
