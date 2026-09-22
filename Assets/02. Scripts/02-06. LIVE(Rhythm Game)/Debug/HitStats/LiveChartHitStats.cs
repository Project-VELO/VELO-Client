using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 채보 하나(곡 + 난이도)의 플레이테스트 판정 통계입니다. 파일 하나가 이 객체 하나입니다.
/// 곡과 난이도는 노트마다 반복하지 않고 여기에 한 번만 둡니다.
///
/// JsonUtility는 Dictionary를 직렬화하지 못하므로 노트 기록은 List로 두고, 조회용 사전은 기록기가 따로 만듭니다.
/// </summary>
[Serializable]
public class LiveChartHitStats
{
    /// <summary>
    /// 필드를 추가하면 올립니다. JsonUtility에는 필드 별칭이 없으므로 기존 필드 이름은 바꾸지 않습니다.
    /// </summary>
    public const int CURRENT_SCHEMA_VERSION = 1;

    // 기본값을 0으로 두고 저장할 때만 채웁니다. "{}"처럼 빈 JSON도 파싱에는 성공하므로, 저장을 거쳤는지로 걸러 냅니다.
    [SerializeField]
    private int _schemaVersion;

    [SerializeField]
    private string _songId;

    [SerializeField]
    private EDifficulty _difficulty;

    [SerializeField]
    private int _playCount;

    [SerializeField]
    private List<LiveNoteHitStat> _notes = new List<LiveNoteHitStat>();

    public int SchemaVersion => _schemaVersion;
    public string SongId => _songId;
    public EDifficulty Difficulty => _difficulty;

    /// <summary>
    /// 판정이 하나 이상 기록된 판의 수입니다. 재시작한 판도 한 판으로 셉니다.
    /// </summary>
    public int PlayCount => _playCount;

    public List<LiveNoteHitStat> Notes => _notes;

    public void InitChart(string songId, EDifficulty difficulty)
    {
        _songId = songId;
        _difficulty = difficulty;
        _playCount = 0;
        _notes.Clear();
    }

    public void SetSchemaVersion(int schemaVersion)
    {
        _schemaVersion = schemaVersion;
    }

    public void IncreasePlayCount()
    {
        _playCount++;
    }
}
