using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// 대사 한 줄을 한 글자씩 출력합니다(기획서 6.7 "한 글자씩 한 문장만 출력").
///
/// 문자열은 줄이 시작될 때 한 번만 대입하고, 그 뒤로는 TMP의 maxVisibleCharacters만 바꿉니다.
/// 매 프레임 Substring으로 부분 문자열을 만들면 타이핑이 도는 내내 GC가 쌓입니다. 아래 루프의 할당은 0입니다.
///
/// 글자마다 UniTask.Delay를 걸지 않는 것도 같은 이유입니다. UniTask.Yield는 캐시된 awaitable을
/// 돌려주어 할당이 없고, 경과 시간을 누적하는 방식이라 프레임률이 낮아도 속도가 밀리지 않습니다.
/// </summary>
public class StoryTypewriter
{
    private readonly TMP_Text _target;
    private readonly Func<float> _getSecondsPerCharacter;

    private int _visibleCount;

    public StoryTypewriter(TMP_Text target, Func<float> getSecondsPerCharacter)
    {
        _target = target;
        _getSecondsPerCharacter = getSecondsPerCharacter;
    }

    /// <summary>
    /// 한 줄을 끝까지 출력합니다. 취소되면 남은 글자를 즉시 채우고 빠져나옵니다.
    ///
    /// 취소와 정상 완료가 같은 최종 상태(전체 출력)에 도달하도록 finally에서 마무리합니다.
    /// 호출부에서 "취소됐으면 마저 채워라"를 따로 하면 경로가 둘로 갈라져 언젠가 어긋납니다.
    /// </summary>
    public async UniTask TypeAsync(string text, ETextSpeed speed, CancellationToken cancellationToken)
    {
        int totalCharacters = BeginLine(text);

        // 즉시 출력은 루프를 한 바퀴도 돌 필요가 없습니다. 한 프레임이라도 돌면 첫 글자만 보였다가
        // 채워지는 것이 눈에 띄어 "순간적으로 튀어나온다"는 연출 의도가 깨집니다.
        if (StoryTypingSpeed.IsInstant(speed))
        {
            Complete(totalCharacters);
            return;
        }

        try
        {
            float elapsed = 0f;
            float secondsPerCharacter = Mathf.Max(0.001f, StoryTypingSpeed.GetSecondsPerCharacter(speed, _getSecondsPerCharacter()));
            bool isWordByWord = StoryTypingSpeed.IsWordByWord(speed);

            while (_visibleCount < totalCharacters)
            {
                elapsed += Time.unscaledDeltaTime;

                int target = Mathf.Min(totalCharacters, (int)(elapsed / secondsPerCharacter));

                if (isWordByWord)
                {
                    target = ExtendToWordEnd(target, totalCharacters);
                }

                // 실제로 늘어난 프레임에만 대입합니다. 매 프레임 넣으면 TMP가 메시를 다시 만듭니다.
                // 단어 단위에서는 목표가 앞질러 가 있는 동안 글자 수 계산이 뒤처지므로, 되돌아가지 않도록 부등호로 막습니다.
                if (_visibleCount < target)
                {
                    _visibleCount = target;
                    _target.maxVisibleCharacters = target;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // NEXT로 건너뛰었거나 씬이 내려간 경우입니다. finally에서 상태를 맞춥니다.
        }
        finally
        {
            Complete(totalCharacters);
        }
    }

    /// <summary>
    /// 이번 줄에서 유일하게 문자열을 대입하는 지점입니다.
    /// ForceMeshUpdate는 characterCount를 확정시키기 위해 필요하며 줄마다 한 번만 부릅니다.
    /// </summary>
    private int BeginLine(string text)
    {
        _target.text = text;
        _target.maxVisibleCharacters = 0;
        _target.ForceMeshUpdate();

        _visibleCount = 0;
        return _target.textInfo.characterCount;
    }

    /// <summary>
    /// 도달한 지점이 단어 중간이면 그 단어의 끝까지 밀어 냅니다.
    ///
    /// 문자열을 자르지 않고 TMP가 이미 만들어 둔 characterInfo를 그대로 읽습니다.
    /// Split이나 Substring으로 단어를 나누면 줄마다 배열이 새로 생겨 타이핑 중 GC가 쌓입니다.
    /// </summary>
    private int ExtendToWordEnd(int target, int totalCharacters)
    {
        if (target <= 0)
        {
            return 0;
        }

        TMP_CharacterInfo[] characters = _target.textInfo.characterInfo;
        int index = target;

        while (index < totalCharacters && !char.IsWhiteSpace(characters[index].character))
        {
            index++;
        }

        return index;
    }

    /// <summary>
    /// 씬 언로드가 취소원이었다면 TMP가 이미 파괴돼 있을 수 있습니다.
    /// Unity 객체이므로 ReferenceEquals가 아니라 == 로 확인해야 파괴된 객체를 걸러 냅니다.
    /// </summary>
    private void Complete(int totalCharacters)
    {
        if (_target == null)
        {
            return;
        }

        _visibleCount = totalCharacters;
        _target.maxVisibleCharacters = totalCharacters;
    }
}
