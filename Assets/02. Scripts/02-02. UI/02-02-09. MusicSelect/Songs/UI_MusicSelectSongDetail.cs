using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 선택된 곡의 상세 정보(BPM, 길이, 노트 수)를 표시합니다.
/// 노트 수는 난이도에 따라 달라지므로 채보 요약과 함께 갱신됩니다.
/// </summary>
public class UI_MusicSelectSongDetail : MonoBehaviour, ILanguageRefreshable
{
    private const string NOTE_COUNT_LABEL = "노트 수";

    private const string EMPTY_VALUE_TEXT = "-";
    private const string UNKNOWN_LENGTH_TEXT = "--:--";

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _bpmText;

    [SerializeField]
    private TMP_Text _lengthText;

    [SerializeField]
    private TMP_Text _noteCountText;

    /// <summary>마지막에 받은 값입니다. 언어가 바뀌면 이 값으로 글자를 다시 씁니다.</summary>
    private SongData _song;

    private LiveChartSummary _summary;

    public void RefreshSong(SongData song, LiveChartSummary summary)
    {
        _song = song;
        _summary = summary;

        if (ReferenceEquals(song, null))
        {
            Clear();
            return;
        }

        _bpmText.text = $"BPM {Mathf.RoundToInt(song.Bpm)}";
        _lengthText.text = FormatLength(song.Duration);
        _noteCountText.text = summary.HasChart
            ? $"{UiText.Localize(NOTE_COUNT_LABEL)} {summary.NoteCount}"
            : $"{UiText.Localize(NOTE_COUNT_LABEL)} {EMPTY_VALUE_TEXT}";
    }

    /// <summary>
    /// 언어를 고른 자리에서 부릅니다. "노트 수 120"처럼 숫자를 끼워 만든 글자는 번역표에 없어
    /// 훑기가 되돌릴 짝을 찾지 못합니다. 받아 둔 값으로 다시 씁니다.
    /// </summary>
    public void RefreshLanguage()
    {
        if (ReferenceEquals(_song, null))
        {
            Clear();
            return;
        }

        RefreshSong(_song, _summary);
    }

    public void Clear()
    {
        _song = null;

        _bpmText.text = $"BPM {EMPTY_VALUE_TEXT}";
        _lengthText.text = UNKNOWN_LENGTH_TEXT;
        _noteCountText.text = $"{UiText.Localize(NOTE_COUNT_LABEL)} {EMPTY_VALUE_TEXT}";
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
}
