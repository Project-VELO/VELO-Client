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
        if (line.BackgroundId != _previousBackgroundId)
        {
            _previousBackgroundId = line.BackgroundId;
            _effectPlayer.ResetForNewScene();
        }

        // 화면 연출은 배경과 인물을 갈아 끼운 뒤에 겁니다.
        // 흔들림과 줌이 무대 컨테이너를 만지므로, 안에 든 그림이 먼저 제자리를 잡아야 합니다.
        _effectPlayer.Play(line.EffectId);

        // 소리는 화면 연출과 별도로 흐릅니다. BGM은 다음 지시까지 이어지므로 줄이 넘어가도 끊지 않습니다.
        _audioPlayer.Play(line.BgmId, line.SfxId);

        // 앞 줄의 토큰이 아직 남아 있을 수 있습니다. 팝업 없이 다음 줄로 넘어간 경로가 그렇습니다.
        // 여기서 걷지 않으면 줄마다 CancellationTokenSource가 하나씩 쌓입니다.
        Skip();

        // 씬 언로드와 건너뛰기 두 취소원을 하나로 묶습니다.
        _typingCts = CancellationTokenSource.CreateLinkedTokenSource(_sceneToken);
        TypeAsync(line.Text, line.TextSpeed, _typingCts.Token).Forget();
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

    private async UniTaskVoid TypeAsync(string text, ETextSpeed speed, CancellationToken cancellationToken)
    {
        await _typewriter.TypeAsync(text, speed, cancellationToken);

        OnLineCompleted?.Invoke();
    }
}
