using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VInspector;

/// <summary>
/// "하루 마무리" 배너입니다. 페이드 인 → 잠깐 유지 → 페이드 아웃으로 끝나는 자동 연출이며
/// 클릭을 요구하지 않습니다 — 주 7회 반복되는 전환이라 확인 클릭을 끼우면 흐름이 끊깁니다.
/// (연출 형태는 기획 미확정 항목이라 팝업으로 바뀔 수 있습니다. 문구·시간은 인스펙터에서 조절합니다.)
/// </summary>
public class UI_OfficeDayFinishBanner : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private CanvasGroup _canvasGroup;

    [Foldout("Settings")]
    [SerializeField]
    [Range(0.05f, 1f)]
    private float _fadeDuration = 0.2f;

    [SerializeField]
    [Range(0f, 2f)]
    private float _holdDuration = 0.4f;

    public async UniTask PlayAsync(CancellationToken cancellationToken)
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 0f;

        try
        {
            await FadeAsync(1f, cancellationToken);
            await UniTask.Delay(TimeSpan.FromSeconds(_holdDuration), true, PlayerLoopTiming.Update, cancellationToken);
            await FadeAsync(0f, cancellationToken);
        }
        finally
        {
            // 취소돼도 배너가 화면에 남지 않게 합니다. 파괴 중이면 건드리지 않습니다.
            if (this != null)
            {
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// KillAndCancelAwait는 취소 시 트윈을 끊고 await도 함께 취소시킵니다.
    /// 기본값 Kill은 취소를 정상 완료로 처리하므로, 씬 언로드 중에도 PlayAsync가 다음 줄로 진행됩니다.
    /// </summary>
    private async UniTask FadeAsync(float target, CancellationToken cancellationToken)
    {
        await _canvasGroup.DOFade(target, _fadeDuration)
            .SetUpdate(true)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
    }
}
