using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스케줄 행의 완료 표시를 강제로 켜고 끄는 개발용 토글입니다.
///
/// 완료 상태는 세이브의 진행도를 그대로 따라가므로(UI_ScheduleItemToday.Refresh), 눈으로 확인하려면
/// 그 스케줄을 실제로 끝내야 합니다. 버튼 아트를 손볼 때마다 그러기는 어려워 강제 전환을 둡니다.
///
/// 목록이 다시 그려지면 진짜 상태로 되돌아갑니다. 세이브를 건드리지 않으므로 확인이 끝나면 그만입니다.
/// </summary>
public class UI_ScheduleCompleteDebugToggle : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _toggleButton;

    /// <summary>
    /// 아래의 바로가기 버튼을 모두 뒤집을 기준점입니다. 스케줄 목록의 뿌리를 물립니다.
    /// </summary>
    [SerializeField]
    private GameObject _scheduleRoot;

    private bool _isCompleted;

    private void Awake()
    {
        // 정식 빌드에 개발용 토글이 따라가면 안 됩니다. 에디터와 개발 빌드에서만 남깁니다.
        if (!Debug.isDebugBuild)
        {
            gameObject.SetActive(false);
            return;
        }

        if (_toggleButton != null)
        {
            _toggleButton.onClick.AddListener(ToggleCompleted);
        }
    }

    private void OnDestroy()
    {
        if (_toggleButton != null)
        {
            _toggleButton.onClick.RemoveListener(ToggleCompleted);
        }
    }

    private void ToggleCompleted()
    {
        if (_scheduleRoot == null)
        {
            return;
        }

        _isCompleted = !_isCompleted;

        // 꺼진 행까지 포함해 훑습니다. 스케줄이 비어 감춰진 행도 아트를 함께 확인해야 합니다.
        UI_ScheduleShortcutButton[] buttons =
            _scheduleRoot.GetComponentsInChildren<UI_ScheduleShortcutButton>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetCompleted(_isCompleted);
        }
    }
}
