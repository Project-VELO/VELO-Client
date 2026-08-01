using System.Collections.Generic;

/// <summary>
/// 채보의 노트를 레인별 시간순 대기열로 나눠 들고, 아직 판정되지 않은 노트를 골라 주는 클래스입니다.
///
/// 레인마다 "다음에 판정될 노트"의 위치만 앞으로 밀어 두므로, 입력이 들어올 때도 만료를 훑을 때도
/// 목록 전체를 뒤지지 않고 맨 앞만 봅니다. 그 덕분에 매 프레임 호출해도 탐색과 GC 할당이 없습니다.
/// 한 번 꺼낸 노트는 대기열에서 빠지므로 같은 노트에 여러 입력이 들어와도 최초 입력만 판정됩니다(3-I-2).
/// </summary>
public class LiveNoteQueue
{
    private readonly List<NoteData>[] _laneNotes = new List<NoteData>[LiveLane.COUNT];
    private readonly int[] _laneHeads = new int[LiveLane.COUNT];

    public int TotalNoteCount { get; private set; }
    public int LastNoteTimeMs { get; private set; }

    public LiveNoteQueue()
    {
        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            _laneNotes[i] = new List<NoteData>();
        }
    }

    /// <summary>
    /// 남은 노트가 하나도 없는지 여부입니다. 곡 종료 판단에 사용합니다.
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            for (int i = 0; i < LiveLane.COUNT; i++)
            {
                if (_laneHeads[i] < _laneNotes[i].Count)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// 채보의 노트를 대기열로 싣습니다.
    /// startTimeMs를 지정하면 그보다 앞선 노트는 아예 싣지 않습니다. 곡 중간부터 시작하는 채보 에디터의
    /// 테스트 플레이에서 이미 지나간 노트가 첫 프레임에 한꺼번에 BAD로 쏟아지는 것을 막기 위함입니다.
    /// </summary>
    public void InitNotes(ChartData chart, int startTimeMs = 0)
    {
        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            _laneNotes[i].Clear();
            _laneHeads[i] = 0;
        }

        TotalNoteCount = 0;
        LastNoteTimeMs = 0;

        if (ReferenceEquals(chart, null))
        {
            return;
        }

        foreach (NoteData note in chart.Notes)
        {
            if (!LiveLane.IsValid(note.Lane) || note.TimeMs < startTimeMs)
            {
                continue;
            }

            _laneNotes[note.Lane - LiveLane.FIRST].Add(note);
            TotalNoteCount++;

            if (note.TimeMs > LastNoteTimeMs)
            {
                LastNoteTimeMs = note.TimeMs;
            }
        }

        // 채보 파일의 노트 순서는 편집한 순서라 시간순이 아닐 수 있으므로, 대기열로 쓰기 전에 정렬합니다.
        for (int i = 0; i < LiveLane.COUNT; i++)
        {
            _laneNotes[i].Sort(CompareByTime);
        }
    }

    /// <summary>
    /// 해당 레인에서 지금 입력이 닿는 노트를 꺼냅니다.
    /// 만료된 노트는 CollectExpired가 미리 걷어 가므로, 맨 앞 노트만 확인하면 충분합니다.
    /// </summary>
    public bool TryTake(int lane, int songTimeMs, out NoteData note, out int errorMs)
    {
        note = null;
        errorMs = 0;

        if (!LiveLane.IsValid(lane))
        {
            return false;
        }

        int laneIndex = lane - LiveLane.FIRST;
        List<NoteData> notes = _laneNotes[laneIndex];
        int head = _laneHeads[laneIndex];

        if (head >= notes.Count)
        {
            return false;
        }

        NoteData candidate = notes[head];
        int candidateErrorMs = songTimeMs - candidate.TimeMs;

        if (candidateErrorMs < -LiveJudgementRule.JUDGEABLE_WINDOW_MS || candidateErrorMs > LiveJudgementRule.JUDGEABLE_WINDOW_MS)
        {
            return false;
        }

        _laneHeads[laneIndex] = head + 1;
        note = candidate;
        errorMs = candidateErrorMs;

        return true;
    }

    /// <summary>
    /// 유효 입력 시간이 끝난 노트를 모두 걷어 냅니다. 호출한 쪽이 이들을 BAD로 처리합니다(3-I-2).
    /// </summary>
    public void CollectExpired(int songTimeMs, List<NoteData> expiredNotes)
    {
        for (int laneIndex = 0; laneIndex < LiveLane.COUNT; laneIndex++)
        {
            List<NoteData> notes = _laneNotes[laneIndex];
            int head = _laneHeads[laneIndex];

            while (head < notes.Count && songTimeMs - notes[head].TimeMs > LiveJudgementRule.JUDGEABLE_WINDOW_MS)
            {
                expiredNotes.Add(notes[head]);
                head++;
            }

            _laneHeads[laneIndex] = head;
        }
    }

    /// <summary>
    /// 남은 노트를 시각과 무관하게 모두 걷어 냅니다.
    /// 마지막 노트가 곡 끝에 바짝 붙어 있으면 유효 입력 시간이 끝나기 전에 곡이 먼저 끝나므로,
    /// 완주 시점에 이 노트들을 마저 BAD로 확정해야 정확도의 분모와 판정 개수가 어긋나지 않습니다.
    /// </summary>
    public void CollectRemaining(List<NoteData> remainingNotes)
    {
        for (int laneIndex = 0; laneIndex < LiveLane.COUNT; laneIndex++)
        {
            List<NoteData> notes = _laneNotes[laneIndex];

            for (int i = _laneHeads[laneIndex]; i < notes.Count; i++)
            {
                remainingNotes.Add(notes[i]);
            }

            _laneHeads[laneIndex] = notes.Count;
        }
    }

    private static int CompareByTime(NoteData left, NoteData right)
    {
        return left.TimeMs.CompareTo(right.TimeMs);
    }
}
