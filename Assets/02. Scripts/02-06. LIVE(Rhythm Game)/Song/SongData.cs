using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임에 수록된 곡의 메타데이터를 저장하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class SongData
{
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

    // Json 직렬화 연동성을 고려하여 Dictionary 형태로 구성하며, 
    // 직렬화를 위해 별도 List wrapping 구조를 적용할 수도 있습니다.
    private Dictionary<EDifficulty, ChartMetadata> _charts = new Dictionary<EDifficulty, ChartMetadata>();

    public string SongId { get => _songId; set => _songId = value; }
    public string Title { get => _title; set => _title = value; }
    public float Bpm { get => _bpm; set => _bpm = value; }
    public string AudioFilePath { get => _audioFilePath; set => _audioFilePath = value; }
    public string Composer { get => _composer; set => _composer = value; }
    public float Duration { get => _duration; set => _duration = value; }
    public string CoverImagePath { get => _coverImagePath; set => _coverImagePath = value; }

    public Dictionary<EDifficulty, ChartMetadata> Charts => _charts;
}
