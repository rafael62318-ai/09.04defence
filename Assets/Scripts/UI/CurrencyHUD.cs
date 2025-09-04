using UnityEngine;
using TMPro;

public class CurrencyHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    void Start()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnGoldChanged += HandleChanged;
            HandleChanged(ResourceManager.Instance.Gold); // 초기값 반영
        }
        else
        {
            Debug.LogWarning("[CurrencyHUD] ResourceManager.Instance 없음");
        }
    }

    void OnDestroy()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnGoldChanged -= HandleChanged;
    }

    void HandleChanged(int value)
    {
        Debug.Log($"[CurrencyHUD] HUD 업데이트: {value}");
        if (goldText != null)
            goldText.text = $"Total {value}";
    }
}
