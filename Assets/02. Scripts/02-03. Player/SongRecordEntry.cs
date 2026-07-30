using System;
using UnityEngine;

/// <summary>
/// PlayerData의 곡 기록 딕셔너리를 JSON으로 직렬화하기 위한 항목 구조체입니다.
/// JsonUtility는 Dictionary를 직렬화하지 못하므로 목록 형태로 감싸 저장합니다.
/// </summary>
[Serializable]
public struct SongRecordEntry
{
    [SerializeField]
    private string _key;

    [SerializeField]
    private SongRecord _record;

    public string Key { get => _key; set => _key = value; }
    public SongRecord Record { get => _record; set => _record = value; }
}
