using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// VR 컨트롤러의 트리거와 그립 입력에 따라 손 모델의 애니메이션을 제어하는 스크립트입니다.
/// 유니티의 새로운 Input System을 사용합니다.
/// </summary>
public class ControllerHandsManager : MonoBehaviour
{
    [Header("입력 액션 설정")]
    [Tooltip("트리거 입력을 담당하는 Input Action에 대한 참조입니다.")]
    public InputActionReference triggerActionReference;
    [Tooltip("그립 입력을 담당하는 Input Action에 대한 참조입니다.")]
    public InputActionReference gripActionReference;

    [Header("애니메이션 설정")]
    [Tooltip("손 모델의 애니메이션을 제어하는 Animator 컴포넌트입니다.")]
    public Animator handAnimator;

    private void Awake()
    {
        // 이 스크립트가 붙어있는 게임 오브젝트에서 Animator 컴포넌트를 자동으로 찾아옵니다.
        handAnimator = GetComponent<Animator>();

        // 입력 액션에 대한 이벤트 구독을 설정합니다.
        SetupInputActions();
    }

    /// <summary>
    /// 트리거와 그립 입력 액션에 대한 리스너(이벤트 핸들러)를 설정합니다.
    /// 이 액션들이 발생할 때, 손 애니메이션을 업데이트합니다.
    /// </summary>
    void SetupInputActions()
    {
        // 두 입력 액션이 인스펙터 창에 제대로 연결되었는지 확인합니다.
        if (triggerActionReference != null && gripActionReference != null)
        {
            // --- 트리거 액션 설정 ---
            // performed: 액션이 수행되었을 때 (버튼이 눌렸을 때)
            // ctx.ReadValue<float>()는 트리거를 누른 정도를 0.0에서 1.0 사이의 값으로 읽어옵니다.
            triggerActionReference.action.performed += ctx => UpdateHandAnimation("Trigger", ctx.ReadValue<float>());
            // canceled: 액션이 취소되었을 때 (버튼에서 손을 뗐을 때)
            // 손을 떼면 애니메이션을 원래 상태(0)로 되돌립니다.
            triggerActionReference.action.canceled += ctx => UpdateHandAnimation("Trigger", 0);
            
            // --- 그립 액션 설정 ---
            // 그립 버튼도 트리거와 동일한 방식으로 설정합니다.
            gripActionReference.action.performed += ctx => UpdateHandAnimation("Grip", ctx.ReadValue<float>());
            gripActionReference.action.canceled += ctx => UpdateHandAnimation("Grip", 0);
        }
        else
        {
            Debug.LogWarning("Input Action References가 인스펙터에 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// Animator의 float 파라미터 값을 업데이트하여 손 애니메이션을 변경합니다.
    /// </summary>
    /// <param name="parameterName">Animator의 파라미터 이름 (예: "Trigger", "Grip")</param>
    /// <param name="value">설정할 값 (보통 0.0에서 1.0 사이)</param>
    void UpdateHandAnimation(string parameterName, float value)
    {
        if (handAnimator != null)
        {
            // Animator의 float 파라미터 값을 변경합니다.
            // 이 파라미터는 Animator Controller의 블렌드 트리(Blend Tree)와 연결되어
            // 값에 따라 손가락을 펴거나 주먹을 쥐는 등의 애니메이션을 만듭니다.
            handAnimator.SetFloat(parameterName, value);
        }
    }

    // --- 입력 액션 활성화/비활성화 ---
    // 자원을 절약하고 성능을 최적화하기 위해, 이 오브젝트가 활성화될 때만 입력을 받도록 합니다.
    private void OnEnable()
    {
        // 이 GameObject가 활성화될 때 입력 액션을 활성화하여 입력을 받을 수 있게 합니다.
        triggerActionReference?.action.Enable();
        gripActionReference?.action.Enable();
    }

    private void OnDisable()
    {
        // 이 GameObject가 비활성화될 때 입력 액션을 비활성화하여 자원을 절약합니다.
        triggerActionReference?.action.Disable();
        gripActionReference?.action.Disable();
    }
}