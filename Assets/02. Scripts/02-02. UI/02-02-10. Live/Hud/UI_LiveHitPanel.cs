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

    private CancellationTokenSource _hideCancellation;

    private void OnDestroy()
    {
        CancelHide();
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
    /// </summary>
    public void RefreshJudgement(EJudgement judgement)
    {
        SetJudgementSprite(GetJudgementSprite(judgement));

        CancelHide();
        _hideCancellation = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        HideAfterDelayAsync(_hideCancellation.Token).Forget();
    }

    public void ClearJudgement()
    {
        CancelHide();
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

    private void CancelHide()
    {
        if (_hideCancellation == null)
        {
            return;
        }

        _hideCancellation.Cancel();
        _hideCancellation.Dispose();
        _hideCancellation = null;
    }
}
