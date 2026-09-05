using System;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 대사 한 줄을 화면에 올리고 타이핑이 끝날 때까지를 관리합니다.
///
/// 재생 컨트롤러에서 떼어낸 이유는 취소 토큰의 수명 때문입니다.
/// 줄마다 토큰을 새로 만들고 NEXT나 팝업으로 끊고 다시 만드는 일이 상태 분기와 섞이면,
/// 어느 경로에서 Dispose가 빠졌는지 눈으로 좇기 어려워집니다. 이 클래스 안에서만 오갑니다.
/// </summary>
public class StoryLinePlayer
{
    /// <summary>
    /// 타이핑이 끝났을 때 알립니다. 취소로 끝난 경우에도 마지막 글자까지 채워진 뒤 호출됩니다.
    /// </summary>
    public Action OnLineCompleted;

    private readonly UI_Story _ui;
    private readonly StoryTypewriter _typewriter;
    private readonly StoryEffectPlayer _effectPlayer;
    private readonly StoryAudioPlayer _audioPlayer;
    private readonly CancellationToken _sceneToken;

    private CancellationTokenSource _typingCts;

    /// <summary>
    /// 앞 줄의 배경입니다. 장면이 바뀌는 순간을 알아내려고 들고 있습니다.
    /// 대본은 배경을 "바뀌는 줄에만" 적지만 읽는 시점에 모든 줄로 펼쳐지므로,
    /// 값이 있는지가 아니라 앞 줄과 다른지를 봐야 전환을 집어낼 수 있습니다.
    /// </summary>
    private string _previousBackgroundId;

    public StoryLinePlayer(UI_Story ui, Func<float> getSecondsPerCharacter, CancellationToken sceneToken)
    {
        _ui = ui;
        _sceneToken = sceneToken;
        _typewriter = new StoryTypewriter(ui.DialogBox.BodyText, getSecondsPerCharacter);
        _effectPlayer = new StoryEffectPlayer(ui.EffectLayer, sceneToken);
        _audioPlayer = new StoryAudioPlayer(ui.AudioBinder, sceneToken);
    }

    /// <summary>
    /// 배경·화자·본문을 한 줄분으로 갈아 끼우고 타이핑을 시작합니다.
    /// </summary>
    public void Play(StoryLineData line)
    {
        _ui.Stage.SetBackground(line.BackgroundId);
        _ui.Stage.SetSpeakers(line);
        _ui.DialogBox.Refresh(line);

        // 장면이 바뀌면 앞 장면의 암전·필터·줌을 걷습니다.
        // 이 줄이 자기 연출을 갖고 있으면 걷어 낸 뒤에 새로 걸리므로, 암전으로 시작하는 장면도 그대로 동작합니다.
        //
        // 컷씬은 예외입니다. 앞 컷이 암전으로 끝나고 다음 컷이 그 어둠에서 떠오르는 식으로 이어지는데,
        // 여기서 덮개를 걷으면 그 사이에 그림이 한 프레임 드러나 전환이 끊겨 보입니다.
        // 컷은 등장 연출이 시작 상태를 스스로 정하므로 걷을 필요도 없습니다.
        bool isCut = StoryCutRunner.IsCut(line);
        bool isSceneChanged = line.BackgroundId != _previousBackgroundId;
        _previousBackgroundId = line.BackgroundId;

        if (isSceneChanged && !isCut)
        {
            _effectPlayer.ResetForNewScene();
        }

        _ui.EffectLayer.SetLetterbox(isCut);

        // 화면 연출은 배경과 인물을 갈아 끼운 뒤에 겁니다.
        // 흔들림과 줌이 무대 컨테이너를 만지므로, 안에 든 그림이 먼저 제자리를 잡아야 합니다.
        //
        // 등장과 카메라를 나란히 겁니다. 덮개와 무대는 서로 다른 채널이라 함께 흐릅니다
        // ("페이드인하며 천천히 줌인"). 등장이 먼저인 것은 컷이 나타나는 순간부터 카메라가 움직이기 때문입니다.
        _effectPlayer.Play(line.EntryEffectId);
        _effectPlayer.Play(line.EffectId);

        // 소리는 화면 연출과 별도로 흐릅니다. BGM은 다음 지시까지 이어지므로 줄이 넘어가도 끊지 않습니다.
        _audioPlayer.Play(line.BgmId, line.SfxId);

        // 앞 줄의 토큰이 아직 남아 있을 수 있습니다. 팝업 없이 다음 줄로 넘어간 경로가 그렇습니다.
        // 여기서 걷지 않으면 줄마다 CancellationTokenSource가 하나씩 쌓입니다.
        Skip();

        // 씬 언로드와 건너뛰기 두 취소원을 하나로 묶습니다.
        _typingCts = CancellationTokenSource.CreateLinkedTokenSource(_sceneToken);
        TypeAsync(line, _typingCts.Token).Forget();
    }

    /// <summary>
    /// 남은 글자를 즉시 채웁니다. NEXT 1단계와 팝업 열기에 씁니다(기획서 6.3, 6.4).
    /// 채우는 것은 StoryTypewriter의 finally가 하므로 여기서는 끊기만 합니다.
    /// </summary>
    public void Skip()
    {
        if (_typingCts == null)
        {
            return;
        }

        _typingCts.Cancel();
        _typingCts.Dispose();
        _typingCts = null;
    }

    /// <summary>
    /// 화면을 떠날 때 타이핑과 화면 연출을 모두 걷습니다.
    ///
    /// Skip과 나눈 이유는 Skip이 "글자만 즉시 채워라"는 지시이기 때문입니다.
    /// NEXT를 누르거나 팝업을 열 때마다 흔들림까지 멈추면, 읽는 동안 유지되어야 할 연출이 끊깁니다.
    /// </summary>
    public void Dispose()
    {
        Skip();
        _effectPlayer.Dispose();
        _audioPlayer.Dispose();
    }

    private async UniTaskVoid TypeAsync(StoryLineData line, CancellationToken cancellationToken)
    {
        // 컷씬은 그림이 먼저 자리를 잡고 글이 얹힙니다. 지연을 두지 않는 보통 줄은 곧바로 출력합니다.
        // 지연 중에 끊겼습니다. 타이핑에 들어가지 않아 StoryTypewriter의 finally가 돌지 않으므로
        // 여기서 직접 채우고 줄이 끝났다고 알립니다. 그러지 않으면 대사가 끝내 나오지 않은 채
        // 다음 누름에 넘어가, 누른 사람에게는 한 번에 넘어간 것처럼 보입니다.
        if (0f < line.TextDelaySeconds && !await DelayTextAsync(line.TextDelaySeconds, cancellationToken))
        {
            _typewriter.Fill(line.Text);
            OnLineCompleted?.Invoke();
            return;
        }

        await _typewriter.TypeAsync(line.Text, line.TextSpeed, cancellationToken);

        OnLineCompleted?.Invoke();
    }

    /// <summary>
    /// 대사가 뜨기 전의 빈 시간입니다. 건너뛰기로 끊기면 false를 돌려 타이핑을 시작하지 않습니다.
    ///
    /// 끊긴 뒤에도 글자는 채워져야 하므로 여기서 직접 채웁니다. 타이핑에 들어간 뒤라면
    /// StoryTypewriter의 finally가 맡지만, 아직 시작하지 않은 상태에서는 아무도 채우지 않습니다.
    /// </summary>
    private async UniTask<bool> DelayTextAsync(float seconds, CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.UnscaledDeltaTime,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 컷이 끝나며 다음으로 넘어갈 때의 연출을 겁니다. 컨트롤러가 커서를 옮기기 직전에 부릅니다.
    /// </summary>
    public void PlayExitEffect(StoryLineData line)
    {
        _effectPlayer.Play(line.ExitEffectId);
    }
}
