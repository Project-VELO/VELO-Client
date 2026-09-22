/// <summary>
/// 노트 하나에 대해 확정된 판정입니다. 판정 등급만으로는 노트가 이르게 찍혔는지 늦게 찍혔는지 알 수 없으므로,
/// 확정 경로와 입력 오차를 함께 묶어 LiveJudgementProcessor.OnNoteJudged로 전달합니다.
///
/// 판정마다 만들어 넘기므로 힙 할당이 없도록 구조체로 둡니다.
/// </summary>
public readonly struct LiveNoteJudgement
{
    public NoteData Note { get; }
    public EJudgement Judgement { get; }
    public ELiveJudgementCause Cause { get; }

    /// <summary>
    /// 입력 시각 − 노트 시각(ms)입니다. 음수면 이르게, 양수면 늦게 누른 것입니다. HasTimingError가 false면 의미가 없습니다.
    /// </summary>
    public int ErrorMs { get; }

    public bool HasTimingError => Cause != ELiveJudgementCause.MISS;

    public LiveNoteJudgement(NoteData note, EJudgement judgement, ELiveJudgementCause cause, int errorMs)
    {
        Note = note;
        Judgement = judgement;
        Cause = cause;
        ErrorMs = errorMs;
    }
}
