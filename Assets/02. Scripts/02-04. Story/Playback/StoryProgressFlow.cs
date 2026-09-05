using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// 대본을 첫 줄부터 마지막 줄까지 밀고 나가는 상태 기계입니다.
///
/// 같은 NEXT 클릭이라도 출력 중이면 "즉시 전체 출력", 출력이 끝났으면 "다음 대사"로 갈라지고(기획서 6.3),
/// 컷씬으로 흐르는 줄은 사람이 아니라 컷 길이가 진행을 정합니다. 이 분기를 한곳에 모아 둡니다.
///
/// 화면 총괄(StoryPlaybackController)에서 떼어낸 이유는 그쪽이 팝업과 이탈을 맡기 때문입니다.
/// 어디까지 읽었는지를 재는 일과 화면을 어떻게 드나드는지는 서로 바뀌는 이유가 다릅니다.
/// </summary>
public class StoryProgressFlow : IDisposable
{
    /// <summary>
    /// 마지막 줄까지 끝났을 때 알립니다. 완료 처리와 목록 복귀는 화면 총괄이 맡습니다.
    /// </summary>
    public Action OnFinished;

    private readonly StoryLineCursor _cursor;
    private readonly StoryLinePlayer _linePlayer;
    private readonly StoryCutRunner _cutRunner;

    private EStoryPlaybackState _state = EStoryPlaybackState.TYPING;

    public StoryProgressFlow(UI_Story ui, IReadOnlyList<StoryLineData> lines,
        Func<float> getSecondsPerCharacter, CancellationToken sceneToken)
    {
        _cursor = new StoryLineCursor(lines);

        _linePlayer = new StoryLinePlayer(ui, getSecondsPerCharacter, sceneToken);
        _linePlayer.OnLineCompleted = OnLineCompleted;

        _cutRunner = new StoryCutRunner(sceneToken);
        _cutRunner.OnCutExiting = PlayCutExitEffect;
        _cutRunner.OnCutElapsed = MoveToNextLine;
    }

    /// <summary>
    /// 대사가 다 나온 뒤 다음으로 넘길 수 있게 되기까지의 시간입니다.
    ///
    /// 마지막 글자가 찍히는 순간에 이미 눌러 둔 손가락이 그대로 다음 줄로 넘겨 버리면,
    /// 방금 나온 문장을 읽지 못한 채 화면이 바뀝니다. 읽을 틈을 두려고 잠깐 잠급니다.
    /// </summary>
    private const float NEXT_LOCK_SECONDS = 0.5f;

    /// <summary>
    /// 이 줄의 마지막 글자가 찍힌 시각입니다. 화면이 멈춰도 흘러야 하므로 스케일 없는 시간을 씁니다.
    /// </summary>
    private float _completedAt = float.NegativeInfinity;

    public StoryLineCursor Cursor => _cursor;

    public void Begin()
    {
        PlayCurrentLine();
    }

    /// <summary>
    /// 기획서 6.3의 NEXT 3단계입니다.
    /// 출력 중 → 즉시 전체 출력 / 출력 완료 → 다음 대사 / 마지막 대사 → 완료 후 목록 복귀.
    /// PAUSED와 FINISHING에서는 아무 반응도 하지 않습니다(기획서 6.4, 3-L).
    /// </summary>
    public void Next()
    {
        if (_state != EStoryPlaybackState.TYPING && _state != EStoryPlaybackState.WAITING_NEXT)
        {
            return;
        }

        // 대사가 아직 뜨지 않았습니다. 컷씬은 그림이 자리를 잡을 동안 글자를 늦춰 내는데,
        // 이때 누른 건너뛰기는 채울 글자가 없어 빈 화면만 남깁니다.
        if (_state == EStoryPlaybackState.TYPING && !_linePlayer.HasTextStarted)
        {
            return;
        }

        // 다 나온 지 얼마 되지 않았습니다. 마지막 글자와 같이 눌린 손가락이 그대로 넘기지 않게 잠급니다.
        if (_state == EStoryPlaybackState.WAITING_NEXT
            && Time.unscaledTime - _completedAt < NEXT_LOCK_SECONDS)
        {
            return;
        }

        // 글자가 없는 컷은 채울 것이 없어 누르는 즉시 넘어갑니다. 컷은 그림과 소리가 함께 흐르는
        // 한 덩어리라, 읽을 문장이 없으면 1단계를 두어 봐야 헛누름이 됩니다.
        //
        // 글자가 있는 컷은 보통 줄과 같이 두 번에 나눕니다. 한 번에 넘기면 읽던 문장이 잘려 나가고,
        // 컷 길이는 읽는 속도를 모르는 값이라 사람이 다 읽었는지를 대신 판단할 수 없습니다.
        if (StoryCutRunner.IsCut(_cursor.Current) && string.IsNullOrEmpty(_cursor.Current.Text))
        {
            MoveToNextLine();
            return;
        }

        if (_state == EStoryPlaybackState.TYPING)
        {
            _state = EStoryPlaybackState.WAITING_NEXT;
            _linePlayer.Skip();
            return;
        }

        MoveToNextLine();
    }

    /// <summary>
    /// 팝업을 열기 전에 진행을 멈춥니다(기획서 6.4).
    ///
    /// 출력 중이었다면 남은 글자를 즉시 채우고 멈춥니다. 중간 글자에서 얼려 두면 팝업을 닫은 뒤
    /// 남은 글자를 어떤 속도로 이어 갈지가 새 규격이 되므로, 줄 단위로만 멈추게 고정합니다.
    /// 커서는 움직이지 않으므로 닫으면 같은 줄에서 이어집니다.
    /// </summary>
    public bool TryPause()
    {
        if (_state != EStoryPlaybackState.TYPING && _state != EStoryPlaybackState.WAITING_NEXT)
        {
            return false;
        }

        _state = EStoryPlaybackState.PAUSED;
        _linePlayer.Skip();
        _cutRunner.Cancel();
        return true;
    }

    /// <summary>
    /// 팝업을 닫고 진행을 되돌립니다.
    ///
    /// 컷은 남은 시간이 아니라 길이를 처음부터 다시 잽니다. 멈춘 시점을 기억해 이어 붙이려면
    /// 등장 연출이 어디까지 갔는지도 함께 되살려야 하는데, 그 상태를 들고 다닐 만큼
    /// 감상 중 팝업이 잦지 않습니다. 컷을 처음부터 다시 보는 편이 어중간하게 이어지는 것보다 낫습니다.
    /// </summary>
    public void Resume()
    {
        if (_state != EStoryPlaybackState.PAUSED)
        {
            return;
        }

        _state = EStoryPlaybackState.WAITING_NEXT;

        if (StoryCutRunner.IsCut(_cursor.Current))
        {
            PlayCurrentLine();
        }
    }

    /// <summary>
    /// 더 이상 진행하지 않는 상태로 못 박습니다. 화면을 떠나기로 정해진 뒤의 입력을 막습니다.
    /// </summary>
    public void Finish()
    {
        _state = EStoryPlaybackState.FINISHING;
        _cutRunner.Cancel();
    }

    public void Dispose()
    {
        _linePlayer.Dispose();
        _cutRunner.Dispose();
    }

    private void PlayCurrentLine()
    {
        _state = EStoryPlaybackState.TYPING;
        _linePlayer.Play(_cursor.Current);

        // 컷씬은 사람이 아니라 연출이 진행을 정합니다. 길이가 없는 줄에서는 아무 일도 하지 않습니다.
        _cutRunner.Start(_cursor.Current);
    }

    private void PlayCutExitEffect()
    {
        _linePlayer.PlayExitEffect(_cursor.Current);
    }

    /// <summary>
    /// 커서를 옮기고 다음 줄을 재생합니다.
    /// 컷 길이가 다 찼을 때와 NEXT를 눌렀을 때가 같은 길로 들어옵니다.
    /// </summary>
    private void MoveToNextLine()
    {
        _cutRunner.Cancel();

        if (_cursor.MoveNext())
        {
            PlayCurrentLine();
            return;
        }

        Finish();
        OnFinished?.Invoke();
    }

    /// <summary>
    /// 타이핑이 끝나기 전에 팝업이 열렸거나 화면을 벗어났다면 상태를 되돌리지 않습니다.
    /// </summary>
    private void OnLineCompleted()
    {
        // 건너뛰기로 채운 경우에는 이미 WAITING_NEXT입니다. 잠금 시작 시각은 두 경우 모두 지금입니다.
        _completedAt = Time.unscaledTime;

        if (_state == EStoryPlaybackState.TYPING)
        {
            _state = EStoryPlaybackState.WAITING_NEXT;
        }
    }
}
