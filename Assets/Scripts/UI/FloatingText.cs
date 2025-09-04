using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float lifeTime = 1.5f;

    private TextMeshProUGUI tmp;

    void Awake()
    {
        tmp = GetComponentInChildren<TextMeshProUGUI>(true); // 비활성화 포함 검색
        if (tmp == null)
            Debug.LogError("[FloatingText] TextMeshProUGUI를 여전히 찾을 수 없습니다!");
        Destroy(gameObject, lifeTime);
    }

    public void SetText(string value)
    {
        if (tmp != null)
        {
            tmp.text = value;
            Debug.Log($"[FloatingText] 텍스트 변경됨: {value}");
        }
    }

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
}
