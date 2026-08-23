using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스케줄 행의 바로가기 버튼입니다. 라벨·배경·클릭 차단이 완료 여부 하나로 함께 움직여
/// 한 컴포넌트로 묶었습니다.
///
/// 완료된 행도 버튼을 없애지 않고 자리만 지킵니다. 행 높이가 변하면 목록이 흔들립니다.
///
/// 완료 시 색이 변하지 않아야 하므로(디자인 요구) 이 클래스는 ColorBlock을 건드리지 않습니다.
/// 대신 프리팹에서 Button의 Disabled Color를 불투명 흰색으로 두어 해결합니다. Transition을
/// None으로 바꾸는 방법도 있지만 그러면 눌림 피드백까지 함께 사라집니다.
/// </summary>
public class UI_ScheduleShortcutButton : MonoBehaviour
{
    public Action OnClicked;

    private const string SHORTCUT_LABEL = "바로가기";
    private const string COMPLETED_LABEL = "완료";

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    /// <summary>
    /// 버튼 위에 얹는 글자입니다. 비워 둘 수 있습니다.
    ///
    /// 사무실은 배경 그림에 "바로가기"·"완료"가 이미 그려져 있어 여기를 비웁니다. 물려 두면
    /// 같은 글자가 그림 위에 한 번 더 찍혀 겹칩니다. 홈은 글자 없는 배경을 써서 여기로 찍습니다.
    /// </summary>
    [SerializeField]
    private TMP_Text _label;

    [SerializeField]
    private Image _background;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _defaultSprite;

    /// <summary>
    /// 완료 상태의 배경입니다. 아트가 들어온 화면에서만 채웁니다.
    /// 비어 있으면 배경을 그대로 두고 라벨과 클릭 차단만으로 완료를 표현합니다.
    /// </summary>
    [SerializeField]
    private Sprite _completedSprite;

    private void Awake()
    {
        _button.onClick.AddListener(NotifyClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(NotifyClicked);
    }

    /// <summary>
    /// interactable을 끄는 것으로 클릭 차단과 마우스오버 강조 억제가 함께 해결됩니다.
    /// </summary>
    public void SetCompleted(bool isCompleted)
    {
        _button.interactable = !isCompleted;

        RefreshLabel(isCompleted);
        RefreshBackground(isCompleted);
    }

    private void RefreshLabel(bool isCompleted)
    {
        if (_label == null)
        {
            return;
        }

        _label.text = isCompleted ? COMPLETED_LABEL : SHORTCUT_LABEL;
    }

    private void RefreshBackground(bool isCompleted)
    {
        if (_completedSprite == null)
        {
            return;
        }

        _background.sprite = isCompleted ? _completedSprite : _defaultSprite;
    }

    private void NotifyClicked()
    {
        OnClicked?.Invoke();
    }
}
