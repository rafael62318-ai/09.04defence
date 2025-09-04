using UnityEngine;
// UI를 사용하게 될 경우를 대비해 추가할 수 있습니다.
// using UnityEngine.UI; 

public class MainBaseHealth : MonoBehaviour
{
    // 'Instance'는 이 스크립트의 유일한 '대표' 주소입니다.
    // 다른 스크립트에서 MainBaseHealth.Instance 라고 쓰면 언제 어디서든 이 스크립트를 찾아올 수 있습니다. (싱글톤 패턴)
    public static MainBaseHealth Instance { get; private set; }

    [Header("본체 능력치 설정")]
    [Tooltip("본체의 최대 체력을 인스펙터에서 설정합니다.")]
    [SerializeField] private int maxHP = 1000;
    
    // 현재 체력을 내부적으로 저장하고 관리하는 변수입니다.
    private int currentHP;

    // 이 스크립트가 씬에 로드될 때 가장 먼저 실행됩니다.
    void Awake()
    {
        // 만약 씬에 이미 다른 MainBaseHealth의 Instance가 있다면, 이 오브젝트는 파괴합니다. (Instance는 항상 하나여야 함)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // Instance가 비어있다면, 자기 자신을 '대표'로 등록합니다.
            Instance = this;
        }
    }

    // Awake 다음에 한번 실행됩니다.
    void Start()
    {
        // 현재 체력을 최대 체력으로 초기화합니다.
        currentHP = maxHP;
        Debug.Log($"본진 체력 초기화 완료: {currentHP} / {maxHP}");
    }

    /// <summary>
    /// 외부(주로 Enemy.cs)에서 호출하여 본체에 피해를 입히는 함수입니다.
    /// </summary>
    /// <param name="damage">입은 피해량</param>
    public void TakeDamage(int damage)
    {
        // 체력이 0 이하면 더 이상 피해를 입지 않도록 합니다.
        if (currentHP <= 0) return;

        currentHP -= damage;
        Debug.Log($"본체가 {damage}의 피해를 입었습니다! 남은 체력: {currentHP}");

        // 여기에 체력 바 UI를 업데이트하는 로직을 추가할 수 있습니다.

        // 만약 체력이 0 이하로 떨어졌다면
        if (currentHP <= 0)
        {
            // 체력을 0으로 고정 (예: -10으로 표시되지 않도록)
            currentHP = 0;
            GameOver();
        }
    }

    // 게임 오버 처리를 담당하는 함수입니다.
    private void GameOver()
    {
        // 콘솔에 게임 오버 메시지를 띄웁니다.
        Debug.LogError("게임 오버! 본진이 파괴되었습니다.");
        
        // 여기에 게임 오버 UI를 화면에 표시하는 코드를 추가할 수 있습니다.
        
        // Time.timeScale을 0으로 만들면 게임 내 모든 시간(애니메이션, 물리 등)이 멈춥니다.
        Time.timeScale = 0f; 
    }
}