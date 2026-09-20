using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class UI_LiveHitPanel : MonoBehaviour
{
    [Header("Display")]
    [Tooltip("판정 로고가 화면에 머무는 시간(초)입니다.")]
    [SerializeField]
    private float _displaySeconds = 0.4f;

    [Foldout("Hierarchy")]
    [Header("Components")]
    [SerializeField]
    private Image _judgementImage;

    /// <summary>
    /// 판정이 뜰 때마다 로고를 절반 크기에서 원래 크기로 빠르게 키우는 연출입니다.
    /// 로고는 그림만 갈아 끼우며 같은 자리에 머물러, 연속으로 같은 판정이 나면 바뀐 줄 모르고 지나칩니다.
    /// 매번 다시 커지게 해서 판정이 새로 들어왔다는 것이 눈에 띄게 합니다.
    /// </summary>
    [SerializeField]
    private UI_ScaleAnimator _popAnimator;

    [Foldout("Project")]
    [Header("Judgement Sprites")]
    [SerializeField]
    private Sprite _perfectSprite;

    [SerializeField]
    private Sprite _greatSprite;

    [SerializeField]
    private Sprite _goodSprite;

    [SerializeField]
    private Sprite _badSprite;

    private CancellationTokenSource _displayCancellation;

    private void OnDestroy()
    {
        CancelDisplay();
    }

    /// <summary>
    /// 판정마다 로고 가로 폭이 다르므로(PERFECT 315, GREAT 253, GOOD 238, BAD 181)
    /// 스프라이트를 갈아 끼울 때 표시 크기도 원본 크기로 맞춥니다.
    /// 빈 스프라이트를 그대로 두면 흰 사각형이 남으므로 표시 자체를 끕니다.
    /// </summary>
    public void SetJudgementSprite(Sprite sprite)
    {
        _judgementImage.sprite = sprite;
        _judgementImage.enabled = sprite != null;

        if (sprite == null)
        {
            return;
        }

        _judgementImage.SetNativeSize();
    }

    /// <summary>
    /// 판정 로고를 띄우고 잠시 뒤 지웁니다. 다음 노트가 곧바로 판정되면 타이머를 새로 걸어 그림만 갈아 끼웁니다.
    ///
    /// 앞선 연출을 취소하고 다시 재생하므로, 커지는 도중에 다음 판정이 나도 중간 크기에서 이어지지 않고
    /// 처음 크기부터 다시 커집니다(재생 시작에서 시작 크기를 넣습니다).
    /// </summary>
    public void RefreshJudgement(EJudgement judgement)
    {
        SetJudgementSprite(GetJudgementSprite(judgement));

        CancelDisplay();
        _displayCancellation = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

        _popAnimator.PlayOpenAsync(_displayCancellation.Token).Forget();
        HideAfterDelayAsync(_displayCancellation.Token).Forget();
    }

    public void ClearJudgement()
    {
        CancelDisplay();
        SetJudgementSprite(null);
    }

    private async UniTaskVoid HideAfterDelayAsync(CancellationToken cancellationToken)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(_displaySeconds), DelayType.UnscaledDeltaTime, cancellationToken: cancellationToken);
        SetJudgementSprite(null);
    }

    private Sprite GetJudgementSprite(EJudgement judgement)
    {
        switch (judgement)
        {
            case EJudgement.PERFECT:
                return _perfectSprite;

            case EJudgement.GREAT:
                return _greatSprite;

            case EJudgement.GOOD:
                return _goodSprite;

            default:
                return _badSprite;
        }
    }

    /// <summary>
    /// 커지는 연출과 지우는 타이머를 한 토큰으로 묶어 함께 끊습니다.
    /// 취소하면 트윈도 함께 죽으므로(TweenCancelBehaviour.Kill) 사라진 로고의 크기를 계속 건드리지 않습니다.
    /// </summary>
    private void CancelDisplay()
    {
        if (_displayCancellation == null)
        {
            return;
        }

        _displayCancellation.Cancel();
        _displayCancellation.Dispose();
        _displayCancellation = null;
    }
}
