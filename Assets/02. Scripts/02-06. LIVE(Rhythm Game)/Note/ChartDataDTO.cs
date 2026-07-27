using System;
using System.Collections.Generic;

/// <summary>
/// 전체 채보(노트 목록) 데이터를 저장하는 불변(Readonly) DTO 클래스입니다.
/// </summary>
public class ChartDataDTO
{
    public readonly IReadOnlyList<NoteDataDTO> Notes;

    public ChartDataDTO(ChartData source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "DTO의 소스 데이터는 null일 수 없습니다.");
        }

        var sourceNotes = source.Notes;
        if (sourceNotes == null)
        {
            Notes = Array.Empty<NoteDataDTO>();
            return;
        }

        var tempNotes = new List<NoteDataDTO>(sourceNotes.Count);
        foreach (var note in sourceNotes)
        {
            if (note != null)
            {
                tempNotes.Add(note.ToDTO());
            }
        }
        Notes = tempNotes.AsReadOnly();
    }
}
