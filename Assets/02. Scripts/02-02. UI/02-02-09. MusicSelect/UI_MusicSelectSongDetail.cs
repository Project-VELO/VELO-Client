using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 선택된 곡의 상세 정보(앨범 이미지, 곡명, BPM, 길이, 노트 수)를 표시합니다.
/// 노트 수는 난이도에 따라 달라지므로 채보 요약과 함께 갱신됩니다.
/// </summary>
public class UI_MusicSelectSongDetail : MonoBehaviour
{
    private const string EMPTY_VALUE_TEXT = "-";
    private const string UNKNOWN_LENGTH_TEXT = "--:--";

    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _coverImage;

    [SerializeField]
    private TMP_Text _songNameText;

    [SerializeField]
    private TMP_Text _bpmText;

    [SerializeField]
    private TMP_Text _lengthText;

    [SerializeField]
    private TMP_Text _noteCountText;

    [Foldout("Project")]
    [SerializeField]
    private Sprite _placeholderCover;

    private SongCoverLoader _coverLoader;

    // 커버 로드가 끝나기 전에 다른 곡으로 옮겨갈 수 있어, 낡은 결과를 걸러내기 위한 갱신 세대값입니다.
    private int _refreshGeneration;

    public void Init(SongCoverLoader coverLoader)
    {
        _coverLoader = coverLoader;
    }

    public void RefreshSong(SongData song, LiveChartSummary summary)
    {
        _refreshGeneration++;

        if (ReferenceEquals(song, null))
        {
            Clear();
            return;
        }

        _songNameText.text = string.IsNullOrEmpty(song.Title) ? song.SongId : song.Title;
        _bpmText.text = $"BPM {Mathf.RoundToInt(song.Bpm)}";
        _lengthText.text = FormatLength(song.Duration);
        _noteCountText.text = summary.HasChart ? $"노트 수 {summary.NoteCount}" : $"노트 수 {EMPTY_VALUE_TEXT}";

        _coverImage.sprite = _placeholderCover;
        LoadCoverAsync(song, _refreshGeneration, this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void Clear()
    {
        // 어느 경로로 비우든 진행 중이던 커버 로드가 방금 초기화한 화면을 덮어쓰지 않도록 세대를 넘깁니다.
        _refreshGeneration++;

        _songNameText.text = EMPTY_VALUE_TEXT;
        _bpmText.text = $"BPM {EMPTY_VALUE_TEXT}";
        _lengthText.text = UNKNOWN_LENGTH_TEXT;
        _noteCountText.text = $"노트 수 {EMPTY_VALUE_TEXT}";
        _coverImage.sprite = _placeholderCover;
    }

    /// <summary>
    /// 곡 길이는 채보 에디터에서 음원을 한 번 열어야 기록되므로, 아직 값이 없는 곡은 미상으로 표시합니다.
    /// </summary>
    private string FormatLength(float durationSeconds)
    {
        if (durationSeconds <= 0f)
        {
            return UNKNOWN_LENGTH_TEXT;
        }

        int totalSeconds = Mathf.RoundToInt(durationSeconds);
        return $"{totalSeconds / 60:D2}:{totalSeconds % 60:D2}";
    }

    private async UniTaskVoid LoadCoverAsync(SongData song, int generation, CancellationToken cancellationToken)
    {
        Sprite cover = await _coverLoader.LoadCoverAsync(song, cancellationToken);

        if (generation != _refreshGeneration || cover == null)
        {
            return;
        }

        _coverImage.sprite = cover;
    }
}
