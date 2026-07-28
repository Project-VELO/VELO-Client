using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 채보(노트 목록) 데이터를 저장하는 가변 데이터 모델 클래스입니다.
/// TimeMs 계열 필드는 모두 OffsetMs가 이미 반영된, 오디오 파일 0초 기준 절대 재생 시각(ms)을 의미합니다.
/// </summary>
[Serializable]
public class ChartData
{
    private const int DEFAULT_BEATS_PER_BAR = 4;

    [SerializeField]
    private string _songId;

    [SerializeField]
    private double _baseBpm;

    [SerializeField]
    private int _offsetMs;

    [SerializeField]
    private int _beatsPerBar = DEFAULT_BEATS_PER_BAR;

    [SerializeField]
    private List<BpmChangeData> _bpmChanges = new List<BpmChangeData>();

    [SerializeField]
    private List<NoteData> _notes = new List<NoteData>();

    public string SongId { get => _songId; set => _songId = value; }
    public double BaseBpm { get => _baseBpm; set => _baseBpm = value; }
    public int OffsetMs { get => _offsetMs; set => _offsetMs = value; }

    // _beatsPerBar 필드가 없던 시절에 저장된 채보 JSON은 역직렬화 시 0이 되므로, 게터에서 기본 박자로 보정합니다.
    public int BeatsPerBar
    {
        get => _beatsPerBar > 0 ? _beatsPerBar : DEFAULT_BEATS_PER_BAR;
        set => _beatsPerBar = Mathf.Max(1, value);
    }

    public List<BpmChangeData> BpmChanges => _bpmChanges;
    public List<NoteData> Notes => _notes;

    public ChartDataDTO ToDTO()
    {
        return new ChartDataDTO(this);
    }
}
