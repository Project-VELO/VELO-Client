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
    public void Enter(string characterId, float seconds, CancellationToken cancellationToken)
    {
        StopFade();

        _characterId = characterId;
        _image.gameObject.SetActive(true);

        SetAlpha(0f);
        FadeAsync(0f, 1f, seconds, false, cancellationToken).Forget();
    }

    /// <summary>
    /// 서 있던 인물을 내립니다. 다 사라진 뒤에 오브젝트를 끕니다.
    /// 먼저 끄면 페이드가 보이지 않고 그냥 사라집니다.
    /// </summary>
    public void Exit(float seconds, CancellationToken cancellationToken)
    {
        StopFade();

        _characterId = null;

        if (!_image.gameObject.activeSelf)
        {
            return;
        }

        FadeAsync(_image.color.a, 0f, seconds, true, cancellationToken).Forget();
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
    }

    /// <summary>
    /// 화면을 떠날 때 진행 중인 페이드를 끊습니다.
    /// </summary>
    public void Dispose()
    {
        StopFade();
    }

    private async UniTaskVoid FadeAsync(float from, float to, float seconds, bool hideWhenDone,
        CancellationToken cancellationToken)
    {
        _fade = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            await StoryEffectTween.LerpAsync(seconds,
                progress => SetAlpha(Mathf.Lerp(from, to, progress)), _fade.Token);

            if (hideWhenDone)
            {
                _image.gameObject.SetActive(false);
            }
        }
        catch (System.OperationCanceledException)
        {
            // 다음 줄이 이 자리를 다시 지정했거나 화면을 떠난 것뿐입니다. 새 지시가 곧 자기 값을 씁니다.
        }
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
