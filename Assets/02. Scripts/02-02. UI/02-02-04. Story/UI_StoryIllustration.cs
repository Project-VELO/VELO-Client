using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 한 장면을 통째로 덮는 삽화(이벤트 CG)를 갈아 끼웁니다(연출표의 [일러스트] 열).
///
/// 배경·인물과 나눈 이유는 그리는 층이 다르기 때문입니다. 삽화는 무대 위에 얹혀 배경과 인물을
/// 함께 가리고, 대사창과 화면 이펙트보다는 아래에 놓입니다. 무대(UI_StoryStage)에 합치면
/// 배경 교체와 인물 배치까지 한 클래스가 떠안게 됩니다.
///
/// 대본에서 삽화와 인물이 같은 줄에 함께 오는 경우는 없습니다. 그래서 삽화가 떠 있는 동안
/// 인물을 따로 내리지 않습니다. 둘이 겹치는 연출이 생기면 그때 규칙을 정하면 됩니다.
/// </summary>
public class UI_StoryIllustration : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Image _image;

    [Foldout("Project")]
    [SerializeField]
    private StoryVisualBinder _visualBinder;

    [Foldout("Settings")]
    /// <summary>
    /// 삽화가 드러나고 걷히는 데 걸리는 시간입니다.
    /// 연출표가 삽화 등장을 대개 "디졸브"·"컷 인"으로 적어 두므로 짧게 둡니다.
    /// 0으로 두면 즉시 나타나고 사라집니다.
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float _fadeSeconds = 0.35f;

    /// <summary>
    /// 지금 떠 있는 삽화입니다. 같은 삽화를 여러 줄이 이어 쓰는 것이 기본이라,
    /// 줄마다 다시 띄우면 글을 읽는 동안 삽화가 계속 깜빡입니다.
    /// </summary>
    private string _currentId;

    private CancellationTokenSource _fade;

    private void OnDestroy()
    {
        StopFade();
    }

    /// <summary>
    /// 이 줄의 삽화를 세웁니다. 빈 칸은 "삽화 없음"이며 떠 있던 것을 걷습니다.
    ///
    /// 배경과 달리 빈 칸을 "직전 유지"로 보지 않습니다. 연출표가 삽화를 컷 단위로 넣고 빼며,
    /// 유지할 구간은 같은 ID를 이어 적어 두기 때문입니다.
    /// </summary>
    public void Set(string illustrationId)
    {
        if (_currentId == illustrationId)
        {
            return;
        }

        _currentId = illustrationId;

        if (string.IsNullOrEmpty(illustrationId))
        {
            Fade(null);
            return;
        }

        Sprite sprite = _visualBinder.GetIllustration(illustrationId);

        if (sprite == null)
        {
            // 그림이 아직 없는 삽화입니다. 빈 사각형을 띄우는 대신 배경을 그대로 두고 알립니다.
            Debug.LogWarning($"[UI_StoryIllustration] 삽화 그림이 없습니다: {illustrationId}");
            Fade(null);
            return;
        }

        Fade(sprite);
    }

    /// <summary>
    /// 그림을 바꾸며 서서히 드러내거나 걷습니다.
    /// 걷을 때는 다 사라진 뒤에 오브젝트를 끕니다. 먼저 끄면 페이드가 보이지 않습니다.
    /// </summary>
    private void Fade(Sprite sprite)
    {
        StopFade();

        if (sprite != null)
        {
            _image.sprite = sprite;
            _image.gameObject.SetActive(true);
            StoryBackgroundFit.Cover(_image, sprite);
        }

        if (_fadeSeconds <= 0f)
        {
            SetAlpha(sprite == null ? 0f : 1f);
            _image.gameObject.SetActive(sprite != null);
            return;
        }

        _fade = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        FadeAsync(sprite != null, _fade.Token).Forget();
    }

    private async UniTaskVoid FadeAsync(bool isAppearing, CancellationToken cancellationToken)
    {
        try
        {
            float from = _image.color.a;
            float to = isAppearing ? 1f : 0f;

            await StoryEffectTween.LerpAsync(_fadeSeconds,
                progress => SetAlpha(Mathf.Lerp(from, to, progress)), cancellationToken);

            if (!isAppearing)
            {
                _image.gameObject.SetActive(false);
            }
        }
        catch (OperationCanceledException)
        {
            // 다음 줄이 다른 삽화를 지시했거나 화면을 떠난 것뿐입니다. 새 지시가 곧 자기 값을 씁니다.
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = _image.color;
        color.a = alpha;
        _image.color = color;
    }

    private void StopFade()
    {
        if (_fade == null)
        {
            return;
        }

        _fade.Cancel();
        _fade.Dispose();
        _fade = null;
    }
}
