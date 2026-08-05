using System.Collections.Generic;

/// <summary>
/// 선택된 노트들의 레인을 좌우 대칭 이동하는 커맨드입니다.
/// GHOST 노트 전용인 6번 레인은 대칭 규칙에서 제외하고, 1~5번 레인만 대칭(1↔5, 2↔4, 3 고정)합니다.
/// </summary>
public class MirrorCommand : ICommand
{
    private readonly List<NoteData> _targets;
    private readonly List<int> _previousLanes;

    public MirrorCommand(List<NoteData> targets)
    {
        _targets = targets;
        _previousLanes = new List<int>(targets.Count);
        foreach (NoteData note in targets)
        {
            _previousLanes.Add(note.Lane);
        }
    }

    public void Execute()
    {
        foreach (NoteData note in _targets)
        {
            note.Lane = MirrorLane(note.Lane);
        }
    }

    public void Undo()
    {
        for (int i = 0; i < _targets.Count; i++)
        {
            _targets[i].Lane = _previousLanes[i];
        }
    }

    private static int MirrorLane(int lane)
    {
        if (lane == 6)
        {
            return lane;
        }
        return 6 - lane;
    }
}
