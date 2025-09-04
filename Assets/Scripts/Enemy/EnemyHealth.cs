using UnityEngine;
using System.Collections;
using System;

// 이 컴포넌트는 Enemy 스크립트가 같은 게임 오브젝트에 반드시 존재해야 함을 강제합니다.
[RequireComponent(typeof(Enemy))]
public class EnemyHealth : MonoBehaviour
{
    // --- 인스펙터에서 직접 설정하는 변수들 ---

    [Header("능력치 설정")]
    [Tooltip("적의 최대 체력입니다.")]
    [SerializeField] private int maxHP = 100;
    [Tooltip("적의 방어력입니다. 받는 피해량에서 이 수치만큼 차감됩니다.")]
    [SerializeField] private int defense = 0;
    [Tooltip("적이 죽었을 때 플레이어에게 지급할 골드입니다.")]
    [SerializeField] private int dropGold = 4;

    // --- 내부적으로 관리되는 변수들 ---
    private int currentHP; // 현재 체력을 추적하는 변수입니다.

    [Header("UI 설정")]
    [Tooltip("적 머리 위에 표시될 체력 UI 프리팹")]
    public GameObject healthUiPrefab;
    [Tooltip("체력 UI가 표시될 Y축 높이")]
    public float healthUiYOffset = 2f;

    [Header("골드 드롭 효과")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject goldTextPrefab;

    // --- 이벤트 ---
    // 체력 변경 시 외부(주로 EnemyHPUI)에 알려주는 신호입니다.
    // Action<int, int>는 int형 데이터 2개를 외부에 전달한다는 의미입니다. (currentHP, maxHP)
    public event Action<int, int> OnHealthChanged;

    // --- 내부 참조 변수 ---
    private Enemy enemy; // 같은 게임 오브젝트에 있는 Enemy 스크립트의 참조를 저장합니다.
    private Coroutine slowCoroutine; // 현재 실행 중인 둔화 효과 코루틴을 저장하여 중복 실행을 방지합니다.
    private float baseSpeed; // 둔화 효과 계산을 위한 원래 속도를 한 번만 저장해두는 변수입니다.


    // 스크립트가 활성화될 때 가장 먼저 한번 호출됩니다.
    void Awake()
    {
        // 자기 자신에게 붙어있는 다른 컴포넌트를 찾는 작업은 Awake에서 하는 것이 안전합니다.
        enemy = GetComponent<Enemy>();
        
        // 인스펙터에서 설정한 최대 체력(maxHP)으로 현재 체력(currentHP)을 초기화합니다.
        currentHP = maxHP;
    }

    // Awake가 실행된 후, 게임의 첫 프레임이 업데이트되기 전에 한번 호출됩니다.
    void Start()
    {
        // Enemy 스크립트가 Awake에서 자신의 moveSpeed를 설정한 이후에 그 값을 가져와 baseSpeed로 저장합니다.
        if (enemy != null && enemy.TryGetComponent<Enemy>(out var enemyComponent))
        {
            baseSpeed = enemy.gameObject.GetComponent<Enemy>().MoveSpeed;
        }

        // 체력 바 UI 프리팹이 지정되었다면, 복제하여 생성하고 머리 위에 배치합니다.
        if (healthUiPrefab != null)
        {
            GameObject healthUiObject = Instantiate(healthUiPrefab, transform.position + new Vector3(0, healthUiYOffset, 0), Quaternion.identity, transform);
            // 생성된 UI에게 이 EnemyHealth 스크립트의 정보를 알려주어 초기화시킵니다.
            healthUiObject.GetComponent<EnemyHPUI>()?.Initialize(this);
        }
        
        // UI에 현재 체력을 처음으로 표시하기 위해 이벤트를 외부로 보냅니다.
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
    
    // 외부에서 현재 체력을 물어볼 때 값을 반환해주는 함수입니다.
    public int GetCurrentHP()
    {
        return currentHP;
    }
    
    // 외부(주로 Projectile.cs)에서 호출하여 이 적에게 피해를 주는 함수입니다.
    public void TakeDamage(int dmg)
    {
        // 이미 체력이 0 이하라면 (죽었다면) 아무것도 하지 않고 함수를 즉시 종료합니다.
        if (currentHP <= 0) return;

        // 방어력을 적용하여 최종 피해량을 계산합니다. Mathf.Max(1, ...)는 최소 1의 피해는 받도록 보장합니다.
        int finalDamage = Mathf.Max(1, dmg - defense);
        currentHP -= finalDamage;
        
        // 디버깅을 위해 콘솔에 피해량과 현재 체력을 출력합니다.
        Debug.Log($"{gameObject.name}이(가) {finalDamage}의 피해를 입었습니다. 현재 체력: {currentHP} / {maxHP}");
        
        // 체력 바 UI를 업데이트하기 위해 체력이 변경되었다는 신호를 보냅니다.
        OnHealthChanged?.Invoke(currentHP, maxHP);

        // 체력이 0 이하로 떨어졌는지 확인하고, 그렇다면 죽음 처리 함수를 호출합니다.
        if (currentHP <= 0)
        {
            Die();
        }
    }
    
    // 외부(주로 Projectile.cs)에서 호출하여 둔화 효과를 적용하는 함수입니다.
    public void ApplySlow(float slowFactor, float duration)
    {
        if (enemy == null) return;

        // 만약 이전에 적용된 둔화 효과가 아직 남아있다면, 이전 효과를 중지하고 새로운 효과로 덮어씌웁니다.
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
        slowCoroutine = StartCoroutine(SlowProcess(slowFactor, duration));
    }
    
    // 둔화 효과를 실제로 처리하는 코루틴(Coroutine)입니다. 시간을 두고 순차적으로 코드를 실행할 수 있게 해줍니다.
    private IEnumerator SlowProcess(float slowFactor, float duration)
    {
        // 원래 속도(baseSpeed)를 기준으로 현재 속도를 감소시킵니다.
        enemy.MoveSpeed = baseSpeed * (1 - slowFactor);

        // 지정된 시간(duration)만큼 기다립니다. 이 시간 동안에는 둔화 상태가 유지됩니다.
        yield return new WaitForSeconds(duration);

        // 지속 시간이 끝나면 원래 속도로 되돌립니다.
        enemy.MoveSpeed = baseSpeed;
        
        // 코루틴 실행이 끝났으므로 변수를 비워줍니다.
        slowCoroutine = null;
    }
    
    // 적이 죽었을 때 호출되는 함수입니다.
    private void Die()
    {
        // 디버깅을 위해 죽었다는 메시지를 콘솔에 출력합니다.
        Debug.Log(gameObject.name + "의 체력이 0이 되어 파괴됩니다.");

        // 리소스 매니저가 존재하고, 드롭 골드가 0보다 크다면 플레이어에게 골드를 지급합니다.
        if (ResourceManager.Instance != null && dropGold > 0)
            ResourceManager.Instance.AddGold(dropGold);

        // 골드 획득 관련 시각 효과 프리팹이 지정되었다면, 복제하여 생성합니다.
        if (coinPrefab != null)
            Instantiate(coinPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
        if (goldTextPrefab != null)
        {
            var obj = Instantiate(goldTextPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            obj.GetComponent<FloatingText>()?.SetText($"+{dropGold}");
        }

        // 이 게임 오브젝트를 씬에서 완전히 파괴(삭제)합니다.
        Destroy(gameObject);
    }
}