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
    private readonly CancellationToken _sceneToken;

    private CancellationTokenSource _typingCts;

    public StoryLinePlayer(UI_Story ui, Func<float> getSecondsPerCharacter, CancellationToken sceneToken)
    {
        _ui = ui;
        _sceneToken = sceneToken;
        _typewriter = new StoryTypewriter(ui.DialogBox.BodyText, getSecondsPerCharacter);
    }

    /// <summary>
    /// 배경·화자·본문을 한 줄분으로 갈아 끼우고 타이핑을 시작합니다.
    /// </summary>
    public void Play(StoryLineData line)
    {
        _ui.Stage.SetBackground(line.BackgroundId);
        _ui.Stage.SetSpeakers(line);
        _ui.DialogBox.RefreshSpeaker(line);
        _ui.DialogBox.RefreshBodyStyle(line.TextStyleId);

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

    private async UniTaskVoid TypeAsync(string text, ETextSpeed speed, CancellationToken cancellationToken)
    {
        await _typewriter.TypeAsync(text, speed, cancellationToken);

        OnLineCompleted?.Invoke();
    }
}
