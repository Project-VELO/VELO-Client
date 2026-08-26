using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 결과 화면 왼쪽의 곡 요약입니다. 커버·난이도·곡명과 등급·점수·클리어 표시를 맡습니다.
///
/// 화면 총괄(UI_LiveResult)에서 떼어낸 이유는 클리어 여부에 따라 갈리는 표시가 여기에 몰려 있기 때문입니다.
/// 실패하면 등급도 점수도 없고 그 자리에 FAILED 배너가 대신 들어옵니다. 그 분기를 한곳에 모아 둡니다.
/// </summary>
public class UI_LiveResultSummaryPanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Song")]
    [SerializeField]
    private Image _coverImage;

    [SerializeField]
    private Image _difficultyImage;

    [SerializeField]
    private TMP_Text _titleText;

    [Foldout("Hierarchy")]
    [Header("Score")]
    [SerializeField]
    private TMP_Text _scoreText;

    [SerializeField]
    private UI_RankIcon _rankIcon;

    /// <summary>
    /// 클리어했을 때만 보여 주는 것들입니다. 등급·SCORE 글자·점수·CLEAR 배너가 여기에 듭니다.
    /// </summary>
    [SerializeField]
    private List<GameObject> _clearOnlyObjects = new List<GameObject>();

    /// <summary>
    /// 실패했을 때 그 자리를 대신하는 배너입니다.
    /// </summary>
    [SerializeField]
    private GameObject _failedBanner;

    [Foldout("Project")]
    [Header("Difficulty")]
    [SerializeField]
    private SerializableDictionary<EDifficulty, Sprite> _difficultySprites =
        new SerializableDictionary<EDifficulty, Sprite>();

    private readonly SongCoverLoader _coverLoader = new SongCoverLoader();

    // 커버는 씬에 매이지 않는 리소스라, 화면을 떠날 때 캐시가 직접 정리해 주어야 드나들 때마다 쌓이지 않습니다.
    private void OnDestroy()
    {
        _coverLoader.Clear();
    }

    public void RefreshResult(LiveResultData result)
    {
        SetClearOnlyVisible(result.IsClear);

        if (_failedBanner != null)
        {
            _failedBanner.SetActive(!result.IsClear);
        }

        SetText(_scoreText, result.Score.ToString("N0"));
        SetDifficulty(result.Difficulty);

        bool hasSong = LiveSongCatalog.Instance.TryGetSong(result.SongId, out SongData song);
        SetText(_titleText, hasSong ? song.Title : result.SongId);

        if (_rankIcon != null)
        {
            _rankIcon.RefreshRank(result.Rank);
        }

        SetCover(null);

        if (hasSong)
        {
            LoadCoverAsync(song, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    private void SetClearOnlyVisible(bool isVisible)
    {
        for (int i = 0; i < _clearOnlyObjects.Count; i++)
        {
            if (_clearOnlyObjects[i] != null)
            {
                _clearOnlyObjects[i].SetActive(isVisible);
            }
        }
    }

    private void SetDifficulty(EDifficulty difficulty)
    {
        if (_difficultyImage == null)
        {
            return;
        }

        bool hasSprite = _difficultySprites.TryGetValue(difficulty, out Sprite sprite) && sprite != null;

        _difficultyImage.sprite = hasSprite ? sprite : null;
        _difficultyImage.enabled = hasSprite;
    }

    /// <summary>
    /// 커버가 없는 곡도 정상이므로 컴포넌트를 꺼 빈 사각형을 남기지 않습니다.
    /// </summary>
    private void SetCover(Sprite cover)
    {
        if (_coverImage == null)
        {
            return;
        }

        _coverImage.sprite = cover;
        _coverImage.enabled = cover != null;
    }

    private async UniTaskVoid LoadCoverAsync(SongData song, CancellationToken cancellationToken)
    {
        SetCover(await _coverLoader.LoadCoverAsync(song, cancellationToken));
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text == null)
        {
            return;
        }

        text.text = value;
    }
}
