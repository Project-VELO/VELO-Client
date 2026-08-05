using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;

public class UI_ScaleAnimator : MonoBehaviour
{
    [SerializeField]
    private float _openDuration = 0.3f;

    [SerializeField]
    private float _closeDuration = 0.2f;

    [SerializeField]
    private Ease _openEase = Ease.OutBack;

    [SerializeField]
    private Ease _closeEase = Ease.InBack;

    [SerializeField]
    private Vector3 _startScale = new Vector3(0.5f, 0.5f, 0.5f);

    [SerializeField]
    private Vector3 _targetScale = Vector3.one;

    /// <summary>
    /// 취소되면 트윈을 함께 죽입니다(TweenCancelBehaviour.Kill).
    /// 팝업이 연출 도중 파괴될 때 트윈만 살아남으면 이미 사라진 transform의 스케일을 계속 건드립니다.
    /// </summary>
    public async UniTask PlayOpenAsync(CancellationToken cancellationToken)
    {
        transform.localScale = _startScale;
        Tween tween = transform.DOScale(_targetScale, _openDuration)
            .SetEase(_openEase)
            .SetUpdate(true);

        await tween.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
    }

    /// <summary>
    /// 취소 시 동작은 PlayOpenAsync와 같습니다.
    /// </summary>
    public async UniTask PlayCloseAsync(CancellationToken cancellationToken)
    {
        transform.localScale = _targetScale;
        Tween tween = transform.DOScale(_startScale, _closeDuration)
            .SetEase(_closeEase)
            .SetUpdate(true);

        await tween.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
    }
}
