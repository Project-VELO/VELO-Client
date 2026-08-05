using System.Collections.Generic;

/// <summary>
/// 대본에서 지금 출력 중인 줄을 가리키는 커서입니다.
///
/// 기획서 3-F-5가 중간 이탈 시 lineId를 저장하지 않는다고 정했으므로 이 값은 씬 안에서만 삽니다.
/// 세이브에 남기지 않는 이유가 그것이며, 재감상은 항상 첫 줄부터 시작합니다.
/// </summary>
public class StoryLineCursor
{
    private IReadOnlyList<StoryLineData> _lines;
    private int _index;

    public StoryLineData Current => _lines[_index];

    public bool HasNext => _index + 1 < _lines.Count;

    /// <summary>
    /// 로그 팝업이 "전체 대사"를 보여 줘야 해서(기획서 6.7) 커서가 원본 목록을 그대로 내줍니다.
    /// </summary>
    public IReadOnlyList<StoryLineData> Lines => _lines;

    public StoryLineCursor(IReadOnlyList<StoryLineData> lines)
    {
        _lines = lines;
        _index = 0;
    }

    public bool MoveNext()
    {
        if (!HasNext)
        {
            return false;
        }

        _index++;
        return true;
    }
}
