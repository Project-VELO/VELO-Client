using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;

public class UI_ScaleAnimator : MonoBehaviour
{
    /// <summary>
    /// 크기를 움직일 대상입니다. 비워 두면 자기 자신을 움직입니다.
    ///
    /// 팝업 루트에는 화면 전체를 덮는 배경 블러가 패널과 나란히 붙어 있습니다.
    /// 루트를 줄이면 블러까지 함께 줄어 화면 가장자리가 비어 보이므로,
    /// 블러를 즉시 깔아야 하는 팝업은 여기에 패널만 지정합니다.
    ///
    /// UI_Popup이 루트에서 이 컴포넌트를 찾으므로(TryGetComponent) 컴포넌트 자체는
    /// 루트에 두고, 움직일 대상만 바꿉니다.
    /// </summary>
    [SerializeField]
    private Transform _target;

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

    private Transform Target => _target != null ? _target : transform;

    /// <summary>
    /// 취소되면 트윈을 함께 죽입니다(TweenCancelBehaviour.Kill).
    /// 팝업이 연출 도중 파괴될 때 트윈만 살아남으면 이미 사라진 transform의 스케일을 계속 건드립니다.
    /// </summary>
    public async UniTask PlayOpenAsync(CancellationToken cancellationToken)
    {
        Transform target = Target;
        target.localScale = _startScale;
        Tween tween = target.DOScale(_targetScale, _openDuration)
            .SetEase(_openEase)
            .SetUpdate(true);

        await tween.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
    }

    /// <summary>
    /// 취소 시 동작은 PlayOpenAsync와 같습니다.
    /// </summary>
    public async UniTask PlayCloseAsync(CancellationToken cancellationToken)
    {
        Transform target = Target;
        target.localScale = _targetScale;
        Tween tween = target.DOScale(_startScale, _closeDuration)
            .SetEase(_closeEase)
            .SetUpdate(true);

        await tween.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
    }
}
