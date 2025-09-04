using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHPUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private EnemyHealth ownerEnemyHealth;

    // EnemyHealth가 호출하여 자신을 알려주는 초기화 함수
    public void Initialize(EnemyHealth owner)
    {
        ownerEnemyHealth = owner;
        // 주인의 OnHealthChanged 이벤트가 발생하면, 나의 UpdateUI 함수를 실행하도록 등록
        ownerEnemyHealth.OnHealthChanged += UpdateUI;
    }

    // EnemyHealth로부터 신호를 받아 UI를 업데이트하는 함수
    private void UpdateUI(int currentHealth, int maxHealth)
    {
        // 슬라이더가 있다면 값을 업데이트
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }

        // 텍스트가 있다면 내용을 업데이트
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }

    // 이 UI 오브젝트가 파괴될 때 호출
    private void OnDestroy()
    {
        // 주인이 존재하면, 등록해두었던 이벤트 연결을 해제 (메모리 누수 방지)
        if (ownerEnemyHealth != null)
        {
            ownerEnemyHealth.OnHealthChanged -= UpdateUI;
        }
    }
}