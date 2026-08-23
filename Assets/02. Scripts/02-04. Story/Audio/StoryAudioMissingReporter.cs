using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 음원이 등록되지 않은 ID를 한 번씩만 알립니다.
///
/// 대본이 쓰는 소리 ID는 많은데 실제 음원은 아직 일부뿐이라, 알릴 때마다 찍으면 한 회차에만
/// 같은 경고가 수십 번 쌓여 정작 봐야 할 로그가 밀려납니다.
///
/// 재생 규칙과 성격이 달라 StoryAudioPlayer에서 떼어 냈습니다. 화면 수명 동안 무엇을 이미
/// 알렸는지 기억하는 것이 이 클래스가 하는 일의 전부입니다.
/// </summary>
public class StoryAudioMissingReporter
{
    private readonly HashSet<string> _reportedIds = new HashSet<string>();

    public void Report(string id, string kind)
    {
        if (!_reportedIds.Add(id))
        {
            return;
        }

        Debug.LogWarning($"[{nameof(StoryAudioPlayer)}] {kind} '{id}'의 음원이 아직 등록되지 않았습니다.");
    }
}
