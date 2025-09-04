using UnityEngine;
using System;

/// <summary>
/// 게임 내 자원(골드) 관리 싱글톤.
/// - 시작 골드 세팅 및 씬 유지 선택
/// - 결제/환불/증가 시 이벤트 브로드캐스트
/// - 상점/설치 시스템과의 시그니처 호환( AddGold + Add 래퍼 )
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Startup")]
    [SerializeField, Min(0)] private int startGold = 0;   // 시작 골드
    [Header("Persist Across Scenes?")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    /// <summary>현재 골드</summary>
    public int Gold { get; private set; }

    // HUD/로그/이펙트 등에서 구독해 사용
    public event Action<int> OnGoldChanged;   // 현재 총골드 변경 시
    public event Action<int> OnGoldGained;    // 골드 획득량
    public event Action<int> OnGoldSpent;     // 골드 소비량

    /// <summary>보유 골드로 결제 가능한지 여부</summary>
    public bool CanAfford(int amount) => amount <= Gold;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        Gold = Mathf.Max(0, startGold);
        OnGoldChanged?.Invoke(Gold); // HUD 초기화
    }

    /// <summary>
    /// 골드 증가(획득/환불).
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        Gold += amount;
        Debug.Log($"[ResourceManager] 골드 획득: {amount}, 총합: {Gold}");

        OnGoldGained?.Invoke(amount);
        OnGoldChanged?.Invoke(Gold);
    }

    /// <summary>
    /// 상점 예시 코드와의 시그니처 호환을 위한 편의 래퍼.
    /// (기존 예시에선 Add(...)를 사용했으므로 그대로 붙여도 동작)
    /// </summary>
    public void Add(int amount) => AddGold(amount);

    /// <summary>
    /// 결제 시도. 성공 시 차감 후 이벤트 발생.
    /// </summary>
    public bool TrySpend(int amount)
    {
        if (amount <= 0 || amount > Gold) return false;

        Gold -= amount;
        Debug.Log($"[ResourceManager] 골드 소비: {amount}, 총합: {Gold}");

        OnGoldSpent?.Invoke(amount);
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
}
