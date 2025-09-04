using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Enemy))]
public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int maxHP = 100;
    private int currentHP;

    [Header("UI 설정")]
    [Tooltip("적 머리 위에 표시될 체력 UI 프리팹")]
    public GameObject healthUiPrefab;
    [Tooltip("체력 UI가 표시될 Y축 높이")]
    public float healthUiYOffset = 2f;

    [Header("골드 드롭")]
    [SerializeField] private int dropGold = 4;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject goldTextPrefab;

    public event Action<int, int> OnHealthChanged;

    private Enemy enemy;
    private Coroutine slowCoroutine;
    private float baseSpeed; // 적의 '진짜' 원래 속도를 저장할 변수

    void Awake()
    {
        currentHP = maxHP;
        enemy = GetComponent<Enemy>();

        // Awake에서 적의 원래 속도를 한번만 저장합니다.
        if (enemy != null)
        {
            baseSpeed = enemy.moveSpeed;
        }
    }

    void Start()
    {
        if (healthUiPrefab != null)
        {
            GameObject healthUiObject = Instantiate(healthUiPrefab, transform.position + new Vector3(0, healthUiYOffset, 0), Quaternion.identity, transform);
            healthUiObject.GetComponent<EnemyHPUI>()?.Initialize(this);
        }
        
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
    
    public int GetCurrentHP()
    {
        return currentHP;
    }
    
    public void TakeDamage(int dmg)
    {
        if (currentHP <= 0) return;

        currentHP -= dmg;
        
        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }
    
    public void ApplySlow(float slowFactor, float duration)
    {
        if (enemy == null) return;

        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
        slowCoroutine = StartCoroutine(SlowProcess(slowFactor, duration));
    }
    
    private IEnumerator SlowProcess(float slowFactor, float duration)
    {
    
        // 현재 속도가 아닌, 처음에 저장해둔 baseSpeed를 기준으로 속도를 감소시킵니다.
        enemy.moveSpeed = baseSpeed * (1 - slowFactor);

        yield return new WaitForSeconds(duration);

        // 둔화가 끝나면 현재 속도가 아닌, 원래 속도(baseSpeed)로 되돌립니다.
        enemy.moveSpeed = baseSpeed;
     
        
        slowCoroutine = null;
    }
    
    private void Die()
    {
        if (ResourceManager.Instance != null && dropGold > 0)
            ResourceManager.Instance.AddGold(dropGold);

        if (coinPrefab != null)
            Instantiate(coinPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
        if (goldTextPrefab != null)
        {
            var obj = Instantiate(goldTextPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            obj.GetComponent<FloatingText>()?.SetText($"+{dropGold}");
        }

        Destroy(gameObject);
    }
}