using System;
using UnityEngine;

/// <summary>
/// 개별 노트의 타이밍, 위치 및 유형 정보를 저장하는 가변 데이터 모델 클래스입니다.
/// </summary>
[Serializable]
public class NoteData
{
    [SerializeField]
    private string _noteId;

    [SerializeField]
    private int _timeMs;

    [SerializeField]
    private int _lane = 1;

    [SerializeField]
    private ENoteType _noteType = ENoteType.NORMAL;

    [SerializeField]
    private int _holdDurationMs = 0;

    public string NoteId { get => _noteId; set => _noteId = value; }
    public int TimeMs { get => _timeMs; set => _timeMs = value; }
    public int Lane 
    { 
        get => _lane; 
        set => _lane = Mathf.Clamp(value, 1, 6); 
    }
    public ENoteType NoteType { get => _noteType; set => _noteType = value; }
    public int HoldDurationMs { get => _holdDurationMs; set => _holdDurationMs = value; }

    public NoteDataDTO ToDTO()
    {
        return new NoteDataDTO(this);
    }
}
