using UnityEngine;

[DisallowMultipleComponent]
public class PurchaseCost : MonoBehaviour
{
    [Header("비용 정보")]
    [Tooltip("터렛을 처음 건설할 때 필요한 비용")]
    public int buildCost = 50; // <--- 이 부분을 public으로 변경!

    [Tooltip("각 레벨로 업그레이드할 때 필요한 비용 배열")]
    public int[] upgradeCosts;

    // 현재 터렛의 레벨 (0레벨 = 기본 상태)
    public int CurrentLevel { get; private set; } = 0;

    // 업그레이드 가능한 최대 레벨
    public int MaxLevel => upgradeCosts != null ? upgradeCosts.Length : 0;

    /// <summary>
    /// 터렛을 더 업그레이드할 수 있는지 확인합니다.
    /// </summary>
    public bool CanUpgrade()
    {
        return CurrentLevel < MaxLevel;
    }

    /// <summary>
    /// 다음 레벨 업그레이드에 필요한 비용을 반환합니다.
    /// 업그레이드할 수 없는 경우 -1을 반환합니다.
    /// </summary>
    public int GetNextUpgradeCost()
    {
        if (!CanUpgrade())
        {
            return -1; // 업그레이드 불가
        }
        return upgradeCosts[CurrentLevel];
    }

    /// <summary>
    /// 터렛의 레벨을 1 증가시킵니다.
    /// 성공하면 true, 실패(최대 레벨 도달)하면 false를 반환합니다.
    /// </summary>
    public bool TryIncreaseLevel()
    {
        if (!CanUpgrade())
        {
            return false;
        }
        CurrentLevel++;
        return true;
    }
}