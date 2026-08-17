using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 대사 한 줄에 딸린 BGM과 효과음을 재생합니다(연출표의 [사운드 제어] 열).
///
/// 두 소리의 규칙이 다릅니다. 효과음은 값이 있는 줄에서 한 번 울리고 끝납니다.
/// BGM은 빈 칸이 "직전 곡 유지"라서, 곡을 바꾸라는 줄에서만 바꾸고 BGM_NONE에서 멈춥니다.
/// 매 줄 같은 값을 채우면 같은 곡을 계속 다시 트는 지시가 되므로, 지금 무슨 곡인지 기억해 둡니다.
///
/// 아직 음원이 없어 대부분의 조회가 빈손으로 돌아옵니다. ID마다 한 번만 알리는 것은
/// 128줄을 넘기며 같은 경고가 반복되면 정작 봐야 할 로그가 밀려나기 때문입니다.
/// </summary>
public class StoryAudioPlayer : IDisposable
{
    private readonly StoryAudioBinder _binder;
    private readonly CancellationToken _sceneToken;

    /// <summary>
    /// 이미 "음원이 없다"고 알린 ID입니다. 경고를 한 번으로 줄이려고 들고 있습니다.
    /// </summary>
    private readonly HashSet<string> _reportedMissingIds = new HashSet<string>();

    /// <summary>
    /// 지금 흐르고 있는 BGM입니다. 같은 곡을 다시 트는 것을 막는 기준입니다.
    /// </summary>
    private string _currentBgmId;

    private CancellationTokenSource _fadeCts;

    public StoryAudioPlayer(StoryAudioBinder binder, CancellationToken sceneToken)
    {
        _binder = binder;
        _sceneToken = sceneToken;
    }

    /// <summary>
    /// 이 줄의 소리를 겁니다. 효과음을 먼저 울리는 것은 BGM 전환이 페이드로 시간을 쓰기 때문입니다.
    /// </summary>
    public void Play(string bgmId, string sfxId)
    {
        PlaySfx(sfxId);
        ApplyBgm(bgmId);
    }

    /// <summary>
    /// 화면을 떠날 때 소리를 모두 걷습니다.
    /// </summary>
    public void Dispose()
    {
        StopFade();

        if (_binder != null && _binder.BgmSource != null)
        {
            _binder.BgmSource.Stop();
            _binder.BgmSource.volume = 0f;
        }

        _currentBgmId = null;
    }

    private void PlaySfx(string sfxId)
    {
        if (string.IsNullOrEmpty(sfxId))
        {
            return;
        }

        AudioClip clip = _binder.GetSfx(sfxId);

        if (clip == null)
        {
            ReportMissing(sfxId, "효과음");
            return;
        }

        _binder.PlaySfx(clip);
    }

    private void ApplyBgm(string bgmId)
    {
        // 빈 칸은 "직전 곡 유지"입니다. 곡을 다시 트지 않습니다.
        if (string.IsNullOrEmpty(bgmId))
        {
            return;
        }

        if (bgmId == StoryScriptTokens.BGM_NONE)
        {
            if (_currentBgmId == null)
            {
                return;
            }

            _currentBgmId = null;
            StopFade();
            _fadeCts = CancellationTokenSource.CreateLinkedTokenSource(_sceneToken);
            FadeOutAsync(_fadeCts.Token).Forget();
            return;
        }

        // 같은 곡을 가리키는 줄이 이어질 수 있습니다. 다시 틀면 흐르던 연주가 처음으로 튑니다.
        if (bgmId == _currentBgmId)
        {
            return;
        }

        AudioClip clip = _binder.GetBgm(bgmId);

        if (clip == null)
        {
            ReportMissing(bgmId, "BGM");
            return;
        }

        _currentBgmId = bgmId;
        StopFade();
        _fadeCts = CancellationTokenSource.CreateLinkedTokenSource(_sceneToken);
        SwitchAsync(clip, _fadeCts.Token).Forget();
    }

    /// <summary>
    /// 흐르던 곡을 줄여 끄고 새 곡을 올립니다. 겹쳐 트는 대신 이어 붙이는 것은
    /// 소스가 하나라서이고, 연출표도 BGM을 겹치라고 요구하지 않습니다.
    /// </summary>
    private async UniTaskVoid SwitchAsync(AudioClip clip, CancellationToken cancellationToken)
    {
        try
        {
            AudioSource source = _binder.BgmSource;

            if (source.isPlaying)
            {
                await FadeAsync(source.volume, 0f, cancellationToken);
            }

            source.clip = clip;
            source.volume = 0f;
            source.Play();

            await FadeAsync(0f, _binder.BgmVolume, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // 다음 지시가 들어왔거나 화면이 내려갔습니다. 볼륨은 그쪽이 다시 정합니다.
        }
    }

    private async UniTaskVoid FadeOutAsync(CancellationToken cancellationToken)
    {
        try
        {
            AudioSource source = _binder.BgmSource;

            await FadeAsync(source.volume, 0f, cancellationToken);

            source.Stop();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private UniTask FadeAsync(float from, float to, CancellationToken cancellationToken)
    {
        AudioSource source = _binder.BgmSource;

        return StoryEffectTween.LerpAsync(_binder.BgmFadeSeconds,
            progress => source.volume = Mathf.Lerp(from, to, progress), cancellationToken);
    }

    private void StopFade()
    {
        if (_fadeCts == null)
        {
            return;
        }

        _fadeCts.Cancel();
        _fadeCts.Dispose();
        _fadeCts = null;
    }

    private void ReportMissing(string id, string kind)
    {
        if (!_reportedMissingIds.Add(id))
        {
            return;
        }

        Debug.LogWarning($"[StoryAudioPlayer] {kind} '{id}'의 음원이 아직 등록되지 않았습니다.");
    }
}
