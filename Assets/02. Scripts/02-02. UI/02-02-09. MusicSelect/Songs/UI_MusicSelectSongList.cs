using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 곡 선택 화면의 곡 목록을 풀에서 꺼낸 행으로 채우고 반환하는 것을 전담합니다.
/// </summary>
public class UI_MusicSelectSongList : MonoBehaviour
{
    public Action<int> OnSongSelected;

    [Foldout("Hierarchy")]
    [SerializeField]
    private RectTransform _itemRoot;

    private readonly List<UI_MusicSelectSongListItem> _items = new List<UI_MusicSelectSongListItem>();

    private SongCoverLoader _coverLoader;

    // 커버 이미지는 비동기로 도착하므로, 도착 전에 목록이 갱신되었다면 낡은 결과를 버려야 합니다.
    private int _refreshGeneration;

    public int ItemCount => _items.Count;

    public void Init(SongCoverLoader coverLoader)
    {
        _coverLoader = coverLoader;
    }

    public void RefreshSongs(IReadOnlyList<SongData> songs, bool isSelectable)
    {
        ReleaseItems();
        _refreshGeneration++;

        if (ReferenceEquals(songs, null))
        {
            return;
        }

        for (int i = 0; i < songs.Count; i++)
        {
            UI_MusicSelectSongListItem item = AcquireItem();
            if (item == null)
            {
                break;
            }

            SongData song = songs[i];
            SongRecord bestRecord = PlayerDataProvider.Instance.GetBestSongRecord(song.SongId);

            item.SetItem(i, GetDisplayTitle(song), bestRecord, isSelectable, song.IsLocked);
            item.OnItemClicked = NotifySongSelected;
        }

        LoadCoversAsync(songs, _refreshGeneration, this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void SetSelectedIndex(int selectedIndex)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].SetSelected(i == selectedIndex);
        }
    }

    public void ReleaseItems()
    {
        foreach (UI_MusicSelectSongListItem item in _items)
        {
            item.ResetItem();
            PoolManager.Instance.Push(EPoolable.MusicSelectSongItem, item.gameObject);
        }

        _items.Clear();
    }

    private UI_MusicSelectSongListItem AcquireItem()
    {
        GameObject go = PoolManager.Instance.Pop(EPoolable.MusicSelectSongItem);
        if (go == null)
        {
            return null;
        }

        var item = go.GetComponent<UI_MusicSelectSongListItem>();
        item.transform.SetParent(_itemRoot, false);
        item.transform.SetAsLastSibling();
        _items.Add(item);

        return item;
    }

    private string GetDisplayTitle(SongData song)
    {
        return string.IsNullOrEmpty(song.Title) ? song.SongId : song.Title;
    }

    private async UniTaskVoid LoadCoversAsync(IReadOnlyList<SongData> songs, int generation, CancellationToken cancellationToken)
    {
        for (int i = 0; i < songs.Count; i++)
        {
            Sprite cover = await _coverLoader.LoadCoverAsync(songs[i], cancellationToken);

            if (generation != _refreshGeneration)
            {
                return;
            }

            if (i < _items.Count)
            {
                _items[i].SetCover(cover);
            }
        }
    }

    private void NotifySongSelected(int songIndex)
    {
        OnSongSelected?.Invoke(songIndex);
    }
}
