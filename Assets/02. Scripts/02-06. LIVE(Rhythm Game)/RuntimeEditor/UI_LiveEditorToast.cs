using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 편집 결과를 짧게 알리는 문구를 띄우고, 정해진 시간이 지나면 스스로 감춥니다.
/// 팝업과 달리 편집을 멈추지 않아야 하므로 클릭을 받지 않고 확인 버튼도 두지 않습니다.
/// </summary>
public class UI_LiveEditorToast : MonoBehaviour
{
    [Header("Duration")]
    [Tooltip("문구를 띄워 두는 시간(초)입니다.")]
    // 0이 되면 남은 시간을 세는 조건에 걸려 문구가 사라지지 않으므로, 애초에 그 값을 넣을 수 없게 막습니다.
    [Min(0.1f)]
    [SerializeField]
    private float _duration = 2f;

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _messageText;

    private float _remainingTime;

    private void Awake()
    {
        if (_messageText != null)
        {
            // 문구가 트랙 위에 겹쳐 있어, 떠 있는 동안 노트 배치 클릭을 삼킬 수 있습니다.
            // 씬에서 꺼 두어도 오브젝트를 다시 만들면 기본값으로 되살아나는 설정이라 코드에서 못박습니다.
            _messageText.raycastTarget = false;
        }

        Hide();
    }

    private void Update()
    {
        if (_remainingTime <= 0f)
        {
            return;
        }

        // 표시 시간은 재생 상태와 무관해야 하므로 timeScale에 영향받지 않는 실제 경과 시간을 씁니다.
        _remainingTime -= Time.unscaledDeltaTime;

        if (_remainingTime <= 0f)
        {
            Hide();
        }
    }

    /// <summary>
    /// 문구를 띄우고 표시 시간을 처음부터 다시 셉니다. 이미 떠 있는 동안 호출하면 시간만 늘어납니다.
    /// </summary>
    public void Show(string message)
    {
        if (_messageText == null)
        {
            Debug.LogWarning($"[UI_LiveEditorToast] 문구를 표시할 텍스트가 연결되지 않아 '{message}'를 띄우지 못했습니다!");
            return;
        }

        _messageText.text = message;
        _messageText.enabled = true;
        _remainingTime = _duration;
    }

    /// <summary>
    /// 오브젝트가 아니라 텍스트만 끕니다. 오브젝트를 끄면 남은 시간을 셀 Update가 돌지 않기 때문입니다.
    /// </summary>
    private void Hide()
    {
        _remainingTime = 0f;

        if (_messageText != null)
        {
            _messageText.enabled = false;
        }
    }
}
