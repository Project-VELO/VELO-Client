using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 채보(노트 목록) 데이터를 저장하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class ChartData
{
    [SerializeField]
    private List<NoteData> _notes = new List<NoteData>();

    public List<NoteData> Notes => _notes;
}
