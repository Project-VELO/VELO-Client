using System;
using UnityEngine;

/// <summary>
/// 곡 진행 중 특정 시점부터 적용되는 BPM 변경 정보를 저장하는 가변 데이터 모델 클래스입니다.
/// </summary>
[Serializable]
public class BpmChangeData
{
    [SerializeField]
    private int _timeMs;

    [SerializeField]
    private double _bpm;

    public int TimeMs { get => _timeMs; set => _timeMs = value; }
    public double Bpm { get => _bpm; set => _bpm = value; }
}
