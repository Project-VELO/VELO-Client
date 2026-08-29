using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임에 수록된 곡의 메타데이터를 저장하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class SongData : ISerializationCallbackReceiver
{
    [Serializable]
    private struct ChartEntry
    {
        public EDifficulty Difficulty;
        public ChartMetadata Metadata;
    }

    [SerializeField]
    private string _songId;

    [SerializeField]
    private string _title;

    [SerializeField]
    private float _bpm;

    [SerializeField]
    private string _audioFilePath;

    [SerializeField]
    private string _composer;

    [SerializeField]
    private float _duration = 0f;

    [SerializeField]
    private string _coverImagePath;

    /// <summary>
    /// 곡 목록에 놓이는 순서입니다. 작을수록 앞에 옵니다.
    /// 폴더명 사전순으로는 기획이 정한 곡 순서를 표현할 수 없어 파일이 직접 정합니다.
    /// 값이 없는 곡은 0이 되어 뒤로 밀립니다(LiveSongCatalogLoader.CompareSongOrder).
    /// </summary>
    [SerializeField]
    private int _order;

    /// <summary>
    /// 아직 플레이할 수 없는 곡입니다. 목록에는 자리를 지키되 고를 수 없게 표시합니다.
    /// 채보가 없어 진입이 막히는 것과 달리, 이쪽은 "준비 중"임을 의도적으로 보여 주는 표시입니다.
    /// </summary>
    [SerializeField]
    private bool _isLocked;

    // Dictionary는 JsonUtility로 직렬화되지 않으므로, List<ChartEntry>로 감싸 직렬화하고
    // ISerializationCallbackReceiver를 통해 런타임에는 Dictionary로 접근할 수 있도록 동기화합니다.
    [SerializeField]
    private List<ChartEntry> _chartEntries = new List<ChartEntry>();

    private Dictionary<EDifficulty, ChartMetadata> _charts = new Dictionary<EDifficulty, ChartMetadata>();

    public string SongId { get => _songId; set => _songId = value; }
    public string Title { get => _title; set => _title = value; }
    public float Bpm { get => _bpm; set => _bpm = value; }
    public string AudioFilePath { get => _audioFilePath; set => _audioFilePath = value; }
    public string Composer { get => _composer; set => _composer = value; }
    public float Duration { get => _duration; set => _duration = value; }
    public string CoverImagePath { get => _coverImagePath; set => _coverImagePath = value; }
    public int Order { get => _order; set => _order = value; }
    public bool IsLocked { get => _isLocked; set => _isLocked = value; }

    public Dictionary<EDifficulty, ChartMetadata> Charts => _charts;

    // 아래 두 값은 곡이 어느 폴더에서 읽혔는지를 런타임에 주입받는 값입니다.
    // 자동 구현 프로퍼티라 JsonUtility 직렬화 대상이 아니므로, 곡의 소속 챕터는 파일이 아니라
    // 수록 공간의 폴더 구조가 계속 유일한 출처로 남습니다.
    public string ChapterId { get; set; }
    public string FolderPath { get; set; }

    public void OnBeforeSerialize()
    {
        _chartEntries.Clear();
        foreach (var pair in _charts)
        {
            _chartEntries.Add(new ChartEntry { Difficulty = pair.Key, Metadata = pair.Value });
        }
    }

    public void OnAfterDeserialize()
    {
        _charts.Clear();
        foreach (var entry in _chartEntries)
        {
            _charts[entry.Difficulty] = entry.Metadata;
        }
    }
}
