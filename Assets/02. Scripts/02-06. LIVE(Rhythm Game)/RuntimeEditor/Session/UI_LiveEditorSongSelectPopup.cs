using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 등록된 곡 목록을 보여주고 하나를 고르게 하는 팝업입니다.
/// 채보 생성 흐름과 채보 불러오기 흐름이 같은 팝업을 공유하며, 제목만 흐름에 맞게 바꿔 표시합니다.
/// </summary>
public class UI_LiveEditorSongSelectPopup : UI_Popup
{
    public Action<string> OnSongSelected;
    public Action OnBackClicked;

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_LiveEditorListView _listView;

    [SerializeField]
    private TMP_Text _titleText;

    [SerializeField]
    private TMP_Text _emptyMessageText;

    [SerializeField]
    private Button _backButton;

    private readonly List<string> _songIds = new List<string>();

    protected override void Awake()
    {
        base.Awake();
        _backButton.onClick.AddListener(NotifyBack);
    }

    /// <summary>
    /// 팝업을 열기 직전에 호출되어 곡 목록을 다시 채웁니다.
    /// </summary>
    public void RefreshSongs(string title, List<string> songIds)
    {
        _titleText.text = title;

        _listView.ReleaseItems();
        _songIds.Clear();

        if (songIds != null)
        {
            _songIds.AddRange(songIds);
        }

        _emptyMessageText.gameObject.SetActive(_songIds.Count == 0);

        for (int i = 0; i < _songIds.Count; i++)
        {
            UI_LiveEditorListItem item = _listView.AcquireItem();
            if (item == null)
            {
                break;
            }

            item.SetItem(i, _songIds[i], true);
            item.OnItemClicked = NotifySongSelected;
        }
    }

    private void NotifySongSelected(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= _songIds.Count)
        {
            return;
        }

        OnSongSelected?.Invoke(_songIds[itemIndex]);
    }

    private void NotifyBack()
    {
        OnBackClicked?.Invoke();
    }
}
