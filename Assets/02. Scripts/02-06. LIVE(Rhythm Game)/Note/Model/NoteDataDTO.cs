using System;

/// <summary>
/// 개별 노트의 타이밍, 위치 및 유형 정보를 저장하는 불변(Readonly) DTO 클래스입니다.
/// </summary>
public class NoteDataDTO
{
    public readonly string NoteId;
    public readonly int TimeMs;
    public readonly int Lane;
    public readonly ENoteType NoteType;
    public readonly int HoldDurationMs;

    public NoteDataDTO(NoteData source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "DTO의 소스 데이터는 null일 수 없습니다.");
        }

        NoteId = source.NoteId;
        TimeMs = source.TimeMs;
        Lane = source.Lane;
        NoteType = source.NoteType;
        HoldDurationMs = source.HoldDurationMs;
    }
}
