using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 현재 날짜를 감싸는 하이라이트 테두리입니다.
///
/// 주간 표의 칸이 아니라 표 밖의 단일 오브젝트입니다. 하루 마무리 때 "표는 고정한 채
/// 테두리만 다음 날짜 칸으로 슬라이드"(기획서 9.5)해야 하는데, 칸마다 테두리를 켜고 끄는
/// 구조로는 "사라졌다가 새로 나타나는 방식 금지"(기획서 16-7)를 만족할 수 없기 때문입니다.
/// </summary>
public class UI_OfficeDayHighlight : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _image;

    [Foldout("Settings")]
    [SerializeField]
    [Range(0.1f, 2f)]
    private float _slideDuration = 0.5f;

    [SerializeField]
    [Range(0.1f, 2f)]
    private float _pulseDuration = 0.5f;

    /// <summary>
    /// 점멸이 내려갈 최저 투명도입니다. 0이면 순간적으로 완전히 사라져 16-7의 금지 항목처럼 보입니다.
    /// </summary>
    [SerializeField]
    [Range(0.1f, 1f)]
    private float _pulseMinAlpha = 0.35f;

    /// <summary>
    /// 칸 크기에 더할 여백입니다. 테두리를 칸과 똑같은 크기로 그리면 획이 칸 안쪽으로 파고들어
    /// 스케줄 이름을 덮습니다. 주간 표의 칸 간격이 23이므로 좌우 각 10까지가 옆 칸을 침범하지 않는 한계입니다.
    /// </summary>
    [SerializeField]
    private Vector2 _sizePadding = new Vector2(20f, 0f);

    private RectTransform _rect;
    private Tween _pulseTween;
    private float _baseAlpha;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _baseAlpha = _image.color.a;
    }

    /// <summary>
    /// 대상 칸 위치로 즉시 이동합니다. 화면 진입·전체 갱신 시 사용합니다.
    /// </summary>
    public void SnapTo(RectTransform cell)
    {
        gameObject.SetActive(true);
        ApplySize(cell);
        _rect.position = GetWorldCenter(cell);
    }

    /// <summary>
    /// 다음 날짜 칸까지 슬라이드합니다(기획서 9.5, 약 0.5초·부드러운 시작과 끝).
    /// 월드 좌표로 움직입니다. 테두리와 칸의 부모가 달라 anchoredPosition으로는 좌표계가 어긋납니다.
    /// </summary>
    public async UniTask SlideToAsync(RectTransform cell, CancellationToken cancellationToken)
    {
        gameObject.SetActive(true);
        ApplySize(cell);

        // Kill이 아니라 KillAndCancelAwait입니다. Kill은 취소를 정상 완료로 처리해 await가 그냥 반환되므로,
        // 씬 언로드로 취소된 뒤에도 하루 마무리 시퀀스의 다음 단계가 파괴 중인 화면을 계속 건드립니다.
        // 취소 시 트윈을 끊는 것은 두 값이 동일하며, 다음 진입은 SnapTo로 다시 놓으므로 도착 보정은 필요 없습니다.
        await _rect.DOMove(GetWorldCenter(cell), _slideDuration)
            .SetEase(Ease.InOutQuad)
            .SetUpdate(true)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
    }

    /// <summary>
    /// 하루 마무리 가능 상태의 점멸 강조입니다(기획서 3-E-3-1의 2). 끄면 원래 투명도로 돌아갑니다.
    /// </summary>
    public void SetPulse(bool isOn)
    {
        StopPulse();

        if (!isOn)
        {
            return;
        }

        _pulseTween = _image.DOFade(_pulseMinAlpha, _pulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    /// <summary>
    /// 현재 날짜가 없을 때(주차 완료) 숨깁니다. 완료된 날짜는 하이라이트를 갖지 않습니다(기획서 3-E-3-1의 3).
    /// </summary>
    public void Hide()
    {
        StopPulse();
        gameObject.SetActive(false);
    }

    private void StopPulse()
    {
        if (_pulseTween != null && _pulseTween.IsActive())
        {
            _pulseTween.Kill();
        }

        _pulseTween = null;

        Color color = _image.color;
        color.a = _baseAlpha;
        _image.color = color;
    }

    private void OnDestroy()
    {
        if (_pulseTween != null && _pulseTween.IsActive())
        {
            _pulseTween.Kill();
        }
    }

    /// <summary>
    /// 칸 크기를 따라갑니다. 테두리의 앵커가 중앙점이어야 sizeDelta가 곧 실제 크기가 됩니다(프리팹에서 고정).
    /// </summary>
    private void ApplySize(RectTransform cell)
    {
        _rect.sizeDelta = cell.rect.size + _sizePadding;
    }

    private static Vector3 GetWorldCenter(RectTransform cell)
    {
        return cell.TransformPoint(cell.rect.center);
    }
}
