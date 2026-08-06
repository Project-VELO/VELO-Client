using System;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 곡 시작과 일시정지 재개 직전의 3초 카운트다운입니다(기획서 3-I-9, SCREEN-009 12.6).
/// 카운트다운 동안 입력을 막는 책임은 호출하는 컨트롤러가 가집니다.
///
/// 유니티 이벤트 메서드를 쓰지 않으므로 컴포넌트가 아니라 컨트롤러가 생성해 쓰는 일반 클래스입니다.
/// 취소 토큰은 소유한 컨트롤러가 자신의 생명주기 토큰을 넘깁니다.
/// </summary>
public class LiveCountdown
{
    private const int COUNTDOWN_SECONDS = 3;

    private readonly UI_LiveCountdownPanel _panel;

    public LiveCountdown(UI_LiveCountdownPanel panel)
    {
        _panel = panel;
    }

    public async UniTask PlayAsync(CancellationToken cancellationToken)
    {
        for (int remainingSeconds = COUNTDOWN_SECONDS; 0 < remainingSeconds; remainingSeconds--)
        {
            if (_panel != null)
            {
                _panel.SetCount(remainingSeconds);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken);
        }

        if (_panel != null)
        {
            _panel.SetVisible(false);
        }
    }
}
