using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 곡 시작과 일시정지 재개 직전의 3초 카운트다운입니다(기획서 3-I-9, SCREEN-009 12.6).
/// 카운트다운 동안 입력을 막는 책임은 호출하는 컨트롤러가 가집니다.
/// </summary>
public class LiveCountdown : MonoBehaviour
{
    private const int COUNTDOWN_SECONDS = 3;

    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Live _liveUI;

    public async UniTask PlayAsync(CancellationToken cancellationToken)
    {
        UI_LiveCountdownPanel panel = _liveUI.CountdownPanel;

        for (int remainingSeconds = COUNTDOWN_SECONDS; remainingSeconds > 0; remainingSeconds--)
        {
            if (panel != null)
            {
                panel.SetCount(remainingSeconds);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken);
        }

        if (panel != null)
        {
            panel.SetVisible(false);
        }
    }
}
