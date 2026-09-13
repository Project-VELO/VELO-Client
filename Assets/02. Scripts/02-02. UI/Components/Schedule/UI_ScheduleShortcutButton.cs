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

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    /// <summary>
    /// 버튼 위에 얹는 글자입니다. 비워 둘 수 있습니다.
    /// 글자가 배경 그림에 이미 그려져 있는 화면에서 물려 두면 같은 글자가 한 번 더 찍혀 겹칩니다.
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

    /// <summary>
    /// 라벨 글자입니다. 한국어로 적어 두면 UiText가 지금 언어로 바꿔 줍니다.
    ///
    /// 그림 대신 글자로 두는 것은 그림에 글자가 구워져 있으면 언어를 바꿔도 한글이 남기 때문입니다.
    /// </summary>
    [Foldout("Settings")]
    [SerializeField]
    private string _shortcutLabel = "바로가기";

    [SerializeField]
    private string _completedLabel = "완료";

    /// <summary>
    /// 바로가기 상태에서 라벨을 끌어올리는 양입니다.
    ///
    /// 바로가기 배경 그림은 아래쪽에 그림자가 깔려 있어 실제 몸통이 위로 치우쳐 있습니다
    /// (112x61 중 세로 0~52, 몸통 중심 26 vs rect 중심 30). 라벨을 rect 한가운데 두면
    /// 글자가 몸통보다 4만큼 내려가 보입니다. 완료 배경은 몸통이 정중앙이라 보정하지 않습니다.
    /// </summary>
    [Foldout("Settings")]
    [SerializeField]
    private float _shortcutLabelOffsetY = 4f;

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

    /// <summary>
    /// 글자는 상자 안에서 가운데로 놓이므로 그림을 쓰던 때처럼 폭을 다시 잴 필요가 없습니다.
    /// 대신 바로가기와 완료의 글자 수가 달라 상자를 넘지 않도록 프리팹에서 자동 크기 조절을 켜 둡니다.
    /// </summary>
    private void RefreshLabel(bool isCompleted)
    {
        if (_label == null)
        {
            return;
        }

        string label = isCompleted ? _completedLabel : _shortcutLabel;

        _label.enabled = !string.IsNullOrEmpty(label);

        if (string.IsNullOrEmpty(label))
        {
            return;
        }

        _label.text = UiText.Localize(label);

        Vector2 position = _label.rectTransform.anchoredPosition;
        position.y = isCompleted ? 0f : _shortcutLabelOffsetY;
        _label.rectTransform.anchoredPosition = position;
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
