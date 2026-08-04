using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스토리 목록의 세로 스크롤 위치를 다룹니다.
///
/// 홈에서 들어오면 최상단, 스케줄 바로가기로 들어오면 지정 회차까지 스크롤합니다(기획서 3-F-3-1).
/// 목록 구성과 스크롤 계산을 한 클래스에 두면 화면 클래스가 좌표 계산까지 떠안게 되어 분리했습니다.
/// </summary>
public class UI_SelectStoryScroller : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private ScrollRect _scrollRect;

    /// <summary>
    /// 이동에 걸리는 시간입니다. 기획서가 "자동으로 스크롤한다"까지만 정하고 수치를 주지 않아,
    /// 유일한 유사 수치인 3-E-3-1의 날짜 하이라이트 이동 시간을 기본값으로 씁니다.
    /// </summary>
    [Foldout("Settings")]
    [SerializeField]
    [Range(0.1f, 2f)]
    private float _scrollDuration = 0.5f;

    /// <summary>
    /// 목록을 맨 위로 올립니다. 홈에서 진입한 경우입니다(기획서 3-F-3-1).
    /// </summary>
    public void SetTop()
    {
        _scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 대상 카드가 뷰포트 가운데에 오도록 스크롤합니다.
    ///
    /// 목록을 채운 직후에는 아직 크기를 쓸 수 없습니다. VerticalLayoutGroup과 ContentSizeFitter가
    /// 같은 프레임에 높이를 확정하지 않아, 바로 계산하면 Content 높이가 0으로 잡혀 아무 데도 가지 않습니다.
    /// 그래서 한 프레임 넘긴 뒤 레이아웃을 강제로 확정시키고 계산합니다.
    /// </summary>
    public async UniTask ScrollToAsync(RectTransform target, CancellationToken cancellationToken)
    {
        if (target == null)
        {
            return;
        }

        await UniTask.NextFrame(cancellationToken);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);

        float destination = GetNormalizedPosition(target);
        await MoveAsync(destination, cancellationToken);
    }

    /// <summary>
    /// 대상이 뷰포트 가운데 오는 verticalNormalizedPosition을 구합니다. 1이 최상단, 0이 최하단입니다.
    /// </summary>
    private float GetNormalizedPosition(RectTransform target)
    {
        RectTransform content = _scrollRect.content;
        float scrollable = content.rect.height - _scrollRect.viewport.rect.height;

        // 목록이 뷰포트보다 짧으면 스크롤할 것이 없습니다.
        if (scrollable <= 0f)
        {
            return 1f;
        }

        // Content는 pivot이 위쪽이라 로컬 y가 아래로 갈수록 음수입니다.
        float distanceFromTop = -content.InverseTransformPoint(target.position).y;
        float offset = distanceFromTop - _scrollRect.viewport.rect.height * 0.5f;

        return 1f - Mathf.Clamp01(offset / scrollable);
    }

    private async UniTask MoveAsync(float destination, CancellationToken cancellationToken)
    {
        float start = _scrollRect.verticalNormalizedPosition;
        float elapsed = 0f;

        while (elapsed < _scrollDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _scrollRect.verticalNormalizedPosition = Mathf.Lerp(start, destination, elapsed / _scrollDuration);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        _scrollRect.verticalNormalizedPosition = destination;
    }
}
