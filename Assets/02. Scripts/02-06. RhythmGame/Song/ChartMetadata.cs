using System;
using UnityEngine;

/// <summary>
/// 특정 난이도에 해당하는 채보(Chart)의 메타데이터를 저장하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class ChartMetadata
{
    [SerializeField]
    private string _chartFilePath;

    [SerializeField]
    private int _level = 1;

    [SerializeField]
    private int _totalNoteCount = 0;

    public string ChartFilePath { get => _chartFilePath; set => _chartFilePath = value; }
    public int Level { get => _level; set => _level = value; }
    public int TotalNoteCount { get => _totalNoteCount; set => _totalNoteCount = value; }
}
