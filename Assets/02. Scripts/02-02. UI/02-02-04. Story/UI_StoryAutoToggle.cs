using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 대사 상자 우상단의 AUTO 버튼입니다. 누를 때마다 자동 진행을 켜고 끕니다.
///
/// 켜짐과 꺼짐을 그림 두 장으로 갈아 끼웁니다. 색만 낮추면 밝은 배경 위에서 어느 쪽이 켜진 상태인지
/// 읽히지 않아, 디자인이 두 판을 따로 그려 주었습니다.
///
/// 자동 진행 자체(4초 뒤 다음 줄)는 StoryAutoPlay가 맡습니다. 이 버튼은 값을 바꾸고 알릴 뿐입니다.
/// </summary>
public class UI_StoryAutoToggle : MonoBehaviour
{
    public Action<bool> OnChanged;

    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _button;

    [SerializeField]
    private Image _image;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _onSprite;

    [SerializeField]
    private Sprite _offSprite;

    public bool IsOn => StoryAutoPlaySetting.IsOn;

    private void Awake()
    {
        _button.onClick.AddListener(Toggle);
    }

    // 다른 회차에서 바꾼 값이 있을 수 있어 켤 때마다 지금 값을 다시 비춥니다.
    private void OnEnable()
    {
        Refresh();
    }

    private void Toggle()
    {
        // 선택된 채로 두면 다음 Enter가 대사를 넘기는 대신 이 버튼을 다시 눌러 AUTO가 꺼집니다.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        StoryAutoPlaySetting.Set(!IsOn);
        Refresh();
        OnChanged?.Invoke(IsOn);
    }

    private void Refresh()
    {
        _image.sprite = IsOn ? _onSprite : _offSprite;
    }
}
