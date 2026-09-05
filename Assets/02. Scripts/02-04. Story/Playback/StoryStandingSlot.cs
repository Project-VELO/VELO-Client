using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인물이 서는 자리 하나의 등장·퇴장을 맡습니다.
///
/// 자리마다 상태를 따로 들고 있어야 하는 이유는, 빈 칸이 "직전 유지"라서 같은 인물이 여러 줄에
/// 걸쳐 서 있기 때문입니다(StoryScriptLoader의 캐리오버). 줄마다 무조건 페이드하면 서 있는 인물이
/// 매 줄 깜빡입니다. 그래서 바뀌는 순간에만 연출합니다.
/// </summary>
public class StoryStandingSlot
{
    private readonly Image _image;

    /// <summary>
    /// 지금 이 자리에 서 있는 인물입니다. 비어 있으면 아무도 없다는 뜻입니다.
    /// </summary>
    private string _characterId;

    private CancellationTokenSource _fade;

    /// <summary>
    /// 이 자리의 제자리입니다. 움직여 들어오고 나가는 방식이 여기를 기준으로 오갑니다.
    /// 자리는 인물마다 다르므로(StoryCharacterLayoutTable) 등장할 때마다 다시 잽니다.
    /// </summary>
    private Vector2 _home;

    public StoryStandingSlot(Image image)
    {
        _image = image;
    }

    /// <summary>
    /// 이번 줄의 인물이 지금 서 있는 인물과 같은지 봅니다. 같으면 표정만 갈아 끼우면 됩니다.
    /// </summary>
    public bool Holds(string characterId)
    {
        return _characterId == characterId;
    }

    /// <summary>
    /// 아무도 없던 자리에 인물을 세웁니다. 투명한 상태에서 시작해 서서히 드러납니다.
    /// </summary>
    public void Enter(string characterId, EStoryCharacterTransition transition, float seconds,
        CancellationToken cancellationToken)
    {
        StopFade();

        _characterId = characterId;
        _image.gameObject.SetActive(true);

        RectTransform rect = _image.rectTransform;
        _home = rect.anchoredPosition;

        if (StoryStandingMotion.IsInstant(transition))
        {
            SetAlpha(1f);
            return;
        }

        // 움직여 들어오는 방식은 처음부터 다 보입니다. 자리까지 흐릿하면 무엇이 움직이는지 읽히지 않습니다.
        SetAlpha(transition == EStoryCharacterTransition.FADE ? 0f : 1f);

        _fade = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        EnterAsync(transition, seconds, _fade.Token).Forget();
    }

    /// <summary>
    /// 서 있던 인물을 내립니다. 다 사라진 뒤에 오브젝트를 끕니다.
    /// 먼저 끄면 페이드가 보이지 않고 그냥 사라집니다.
    /// </summary>
    public void Exit(EStoryCharacterTransition transition, float seconds, CancellationToken cancellationToken)
    {
        StopFade();

        _characterId = null;

        if (!_image.gameObject.activeSelf)
        {
            return;
        }

        if (StoryStandingMotion.IsInstant(transition))
        {
            Hide();
            return;
        }

        _fade = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        ExitAsync(transition, seconds, _fade.Token).Forget();
    }

    /// <summary>
    /// 서 있던 인물이 다른 인물로 바뀌는 경우입니다. 사이에 빈 화면을 두지 않도록 그대로 갈아 끼웁니다.
    /// 나갔다 들어오는 연출은 대본이 NONE으로 한 줄 비워 지시합니다.
    /// </summary>
    public void Replace(string characterId)
    {
        StopFade();

        _characterId = characterId;
        _image.gameObject.SetActive(true);
        SetAlpha(1f);

        _image.rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// 화면을 떠날 때 진행 중인 페이드를 끊습니다.
    /// </summary>
    public void Dispose()
    {
        StopFade();
    }

    private async UniTaskVoid EnterAsync(EStoryCharacterTransition transition, float seconds,
        CancellationToken cancellationToken)
    {
        try
        {
            RectTransform rect = _image.rectTransform;

            if (transition == EStoryCharacterTransition.FADE)
            {
                await StoryEffectTween.LerpAsync(seconds, progress => SetAlpha(progress), cancellationToken);
                return;
            }

            await StoryStandingMotion.EnterAsync(rect, transition, _home, seconds, cancellationToken);
        }
        catch (System.OperationCanceledException)
        {
            // 다음 줄이 이 자리를 다시 지정했거나 화면을 떠난 것뿐입니다. 새 지시가 곧 자기 값을 씁니다.
        }
    }

    private async UniTaskVoid ExitAsync(EStoryCharacterTransition transition, float seconds,
        CancellationToken cancellationToken)
    {
        try
        {
            if (transition == EStoryCharacterTransition.FADE)
            {
                float from = _image.color.a;
                await StoryEffectTween.LerpAsync(seconds,
                    progress => SetAlpha(Mathf.Lerp(from, 0f, progress)), cancellationToken);
            }
            else
            {
                await StoryStandingMotion.ExitAsync(_image.rectTransform, transition, _home, seconds, cancellationToken);
            }

            Hide();
        }
        catch (System.OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// 자리를 비웁니다. 다음에 들어올 인물이 앞 인물의 배율과 자리를 물려받지 않도록 함께 되돌립니다.
    /// </summary>
    private void Hide()
    {
        _image.gameObject.SetActive(false);
        _image.rectTransform.localScale = Vector3.one;
        _image.rectTransform.anchoredPosition = _home;
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

    private void SetAlpha(float alpha)
    {
        Color color = _image.color;
        color.a = alpha;
        _image.color = color;
    }
}
