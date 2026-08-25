using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 컷씬 한 컷이 화면에 머무는 시간을 재고, 다 되면 알립니다(연출 명세의 컷 길이).
///
/// 보통 줄은 다 읽었다는 신호를 사람이 NEXT로 주지만, 컷씬은 연출이 진행을 정합니다.
/// 그 차이를 재생 컨트롤러의 상태 분기에 섞지 않고 여기로 떼어 두었습니다.
/// 컨트롤러는 "컷이면 시작하고, 끝났다고 하면 넘긴다"만 알면 됩니다.
/// </summary>
public class StoryCutRunner : IDisposable
{
    /// <summary>
    /// 컷 길이가 다 찼을 때 알립니다. NEXT로 건너뛴 경우에는 부르지 않습니다.
    /// 건너뛰기는 컨트롤러가 이미 다음 컷으로 넘기고 있어, 여기서도 알리면 두 칸이 넘어갑니다.
    /// </summary>
    public Action OnCutElapsed;

    /// <summary>
    /// 컷을 빠져나갈 연출을 걸 때가 되면 알립니다.
    ///
    /// 컷이 끝난 뒤에 걸면 다음 컷이 이미 들어와 있어 전환이 보이지 않습니다. 명세도
    /// "5.5초 부근에서 암전"처럼 전환을 컷 길이 안에 둡니다. 얼마나 앞당길지는
    /// 그 연출이 effects.json에 적어 둔 길이가 그대로 답이 되므로 따로 받지 않습니다.
    /// </summary>
    public Action OnCutExiting;

    private readonly CancellationToken _sceneToken;

    private CancellationTokenSource _cutCts;

    public StoryCutRunner(CancellationToken sceneToken)
    {
        _sceneToken = sceneToken;
    }

    /// <summary>
    /// 이 줄이 컷씬으로 흐르는 줄인지입니다. 길이가 없는 줄은 지금까지처럼 NEXT를 기다립니다.
    /// </summary>
    public static bool IsCut(StoryLineData line)
    {
        return !ReferenceEquals(line, null) && 0f < line.CutSeconds;
    }

    /// <summary>
    /// 컷 길이를 재기 시작합니다. 앞 컷이 아직 돌고 있으면 끊고 새로 시작합니다.
    /// </summary>
    public void Start(StoryLineData line)
    {
        Cancel();

        if (!IsCut(line))
        {
            return;
        }

        _cutCts = CancellationTokenSource.CreateLinkedTokenSource(_sceneToken);
        RunAsync(line, _cutCts.Token).Forget();
    }

    /// <summary>
    /// 재던 시간을 버립니다. NEXT로 건너뛰거나 팝업이 열릴 때 부릅니다.
    /// </summary>
    public void Cancel()
    {
        if (_cutCts == null)
        {
            return;
        }

        _cutCts.Cancel();
        _cutCts.Dispose();
        _cutCts = null;
    }

    public void Dispose()
    {
        Cancel();
    }

    private async UniTaskVoid RunAsync(StoryLineData line, CancellationToken cancellationToken)
    {
        float exitSeconds = GetExitSeconds(line.ExitEffectId, line.CutSeconds);

        try
        {
            await DelayAsync(line.CutSeconds - exitSeconds, cancellationToken);
            OnCutExiting?.Invoke();

            await DelayAsync(exitSeconds, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        OnCutElapsed?.Invoke();
    }

    /// <summary>
    /// 전환 연출이 화면을 덮는 데 걸리는 시간입니다. 없거나 컷 길이보다 길면 0으로 봅니다.
    /// 컷보다 긴 전환은 컷이 시작하자마자 덮으라는 뜻이 되어, 그림이 한 번도 보이지 않습니다.
    /// </summary>
    private float GetExitSeconds(string exitEffectId, float cutSeconds)
    {
        if (string.IsNullOrEmpty(exitEffectId)
            || !MasterDataProvider.Instance.TryGetEffect(exitEffectId, out StoryEffectData effect))
        {
            return 0f;
        }

        return Mathf.Clamp(effect.DurationSeconds, 0f, cutSeconds);
    }

    /// <summary>
    /// 연출은 게임 시간이 멈춰도 흘러야 합니다. 팝업이 시간 배율을 건드려도 컷 길이는 그대로여야 합니다.
    /// </summary>
    private UniTask DelayAsync(float seconds, CancellationToken cancellationToken)
    {
        if (seconds <= 0f)
        {
            return UniTask.CompletedTask;
        }

        return UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.UnscaledDeltaTime,
            cancellationToken: cancellationToken);
    }
}
