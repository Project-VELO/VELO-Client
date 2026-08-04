using DG.Tweening;
using UnityEngine;
using VInspector;

/// <summary>
/// 씬 전환 중에만 화면을 덮는 로딩 패널입니다(기획서 3-L "화면 로딩 중 입력 | 입력 비활성화").
///
/// 켜고 끄는 순간 화면이 툭 바뀌지 않도록 페이드로 드나듭니다. 다만 <b>입력 차단은 페이드와 무관하게 즉시</b>
/// 시작합니다 — 알파가 0이어도 CanvasGroup의 blocksRaycasts가 켜져 있으면 클릭이 막히므로,
/// 페이드 인이 도는 동안에도 떠나는 화면의 버튼은 눌리지 않습니다.
///
/// 반대로 페이드 아웃 중에는 차단을 유지합니다. 아직 패널에 가려 보이지 않는 버튼이
/// 눌리는 편이 0.2초 더 막히는 것보다 나쁘기 때문입니다.
/// </summary>
public class UI_LoadingPanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private CanvasGroup _canvasGroup;

    [Foldout("Settings")]
    [SerializeField]
    [Range(0.05f, 1f)]
    private float _fadeInDuration = 0.12f;

    [SerializeField]
    [Range(0.05f, 1f)]
    private float _fadeOutDuration = 0.2f;

    private Tween _fadeTween;

    /// <summary>
    /// 페이드 아웃이 끝나 스스로 꺼질 차례인지 가리는 표식입니다.
    /// 아웃이 끝나기 전에 다음 전환이 시작되면 값이 달라져, 이미 다시 켜진 패널을 뒤늦게 끄지 않습니다.
    /// </summary>
    private int _showGeneration;

    public void Show()
    {
        _showGeneration++;
        StopFade();

        gameObject.SetActive(true);

        // 차단이 먼저입니다. 페이드가 끝나기를 기다리면 그 사이의 클릭이 떠나는 화면으로 들어갑니다.
        _canvasGroup.blocksRaycasts = true;

        // 전환 도중 다시 호출되면(연속 전환) 이미 올라간 알파를 0으로 되돌리지 않습니다.
        _fadeTween = _canvasGroup.DOFade(1f, _fadeInDuration).SetUpdate(true);
    }

    public void Hide()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        StopFade();

        int generation = _showGeneration;
        _fadeTween = _canvasGroup.DOFade(0f, _fadeOutDuration)
            .SetUpdate(true)
            .OnComplete(() => FinishHide(generation));
    }

    private void FinishHide(int generation)
    {
        // Kill(complete: false)은 OnComplete를 부르지 않으므로 보통은 여기 오지 않지만,
        // 트윈이 어떤 이유로 끝까지 돌아버린 경우에도 지난 세대가 현재 패널을 끄지 못하게 막습니다.
        if (generation != _showGeneration)
        {
            return;
        }

        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void StopFade()
    {
        if (_fadeTween != null && _fadeTween.IsActive())
        {
            _fadeTween.Kill();
        }

        _fadeTween = null;
    }
}
