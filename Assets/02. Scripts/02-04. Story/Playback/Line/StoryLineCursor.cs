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
    /// 대본 원본입니다. 로그 팝업이 지나온 구간만 잘라 쓰므로 목록을 그대로 내줍니다.
    /// </summary>
    public IReadOnlyList<StoryLineData> Lines => _lines;

    /// <summary>
    /// 지금까지 지나온 대사의 개수입니다. 출력 중인 줄까지 셉니다.
    ///
    /// 팝업을 여는 시점에는 재생 컨트롤러가 이미 그 줄을 끝까지 채워 두므로(TryPauseForPopup),
    /// 현재 줄을 포함해도 아직 읽지 않은 문장이 노출되지 않습니다.
    /// </summary>
    public int ReadCount => _index + 1;

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
