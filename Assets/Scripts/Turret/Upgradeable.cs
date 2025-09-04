using UnityEngine;
using System;

public class Upgradeable : MonoBehaviour
{
    public event Action<int> OnUpgraded; // 신규 레벨

    private PurchaseCost cost;

    void Awake() => cost = GetComponent<PurchaseCost>();

    public bool TryUpgrade()
    {
        if (cost == null || ResourceManager.Instance == null) return false;
        if (!cost.CanUpgrade()) return false;

        int price = cost.GetNextUpgradeCost();
        if (!ResourceManager.Instance.TrySpend(price)) return false;

        cost.TryIncreaseLevel();
        OnUpgraded?.Invoke(cost.CurrentLevel);
        return true;
    }
}
