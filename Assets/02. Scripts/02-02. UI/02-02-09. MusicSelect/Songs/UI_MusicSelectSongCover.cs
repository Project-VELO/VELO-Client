using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 곡 선택 화면에서 고른 곡의 커버를 크게 보여 줍니다.
///
/// 목록 행의 작은 커버(UI_MusicSelectSongListItem)와 달리 한 번에 한 장만 띄우므로,
/// 곡을 빠르게 옮겨 다닐 때 늦게 도착한 이전 곡의 커버가 덮어쓰지 않도록 세대 번호로 걸러 냅니다.
/// </summary>
public class UI_MusicSelectSongCover : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _coverImage;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _placeholderCover;

    private SongCoverLoader _coverLoader;

    // 커버는 비동기로 도착합니다. 도착 전에 다른 곡을 고르면 낡은 결과를 버려야 합니다.
    private int _refreshGeneration;

    /// <summary>
    /// 커버 캐시는 곡 선택 화면이 통째로 소유합니다(UI_MusicSelectSelection).
    /// 목록과 같은 캐시를 써야 같은 곡을 두 번 읽지 않습니다.
    /// </summary>
    public void Init(SongCoverLoader coverLoader)
    {
        _coverLoader = coverLoader;
    }

    public void RefreshSong(SongData song)
    {
        _refreshGeneration++;

        SetCover(null);

        if (ReferenceEquals(song, null))
        {
            return;
        }

        LoadCoverAsync(song, _refreshGeneration, this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void Clear()
    {
        RefreshSong(null);
    }

    private async UniTaskVoid LoadCoverAsync(SongData song, int generation, CancellationToken cancellationToken)
    {
        Sprite cover = await _coverLoader.LoadCoverAsync(song, cancellationToken);

        if (generation != _refreshGeneration)
        {
            return;
        }

        SetCover(cover);
    }

    /// <summary>
    /// 커버가 없는 곡도 정상이므로 자리를 비우지 않고 기본 그림을 둡니다.
    /// 기본 그림마저 없으면 컴포넌트를 꺼 빈 사각형을 남기지 않습니다.
    /// </summary>
    private void SetCover(Sprite cover)
    {
        if (_coverImage == null)
        {
            return;
        }

        Sprite sprite = cover != null ? cover : _placeholderCover;

        _coverImage.sprite = sprite;
        _coverImage.enabled = sprite != null;
    }
}
