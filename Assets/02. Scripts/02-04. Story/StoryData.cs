using System;
using UnityEngine;

/// <summary>
/// 스토리 챕터 및 회차에 대한 기획 메타데이터를 저장하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class StoryData
{
    [SerializeField]
    private string _storyId;

    [SerializeField]
    private string _chapterId;

    [SerializeField]
    private int _episodeNumber;

    [SerializeField]
    private string _title;

    [SerializeField]
    private EUnlockType _unlockType = EUnlockType.NONE;

    [SerializeField]
    private string _unlockTargetId;

    [SerializeField]
    private string _firstBackgroundId;

    [SerializeField]
    private string _firstLineId;

    [SerializeField]
    private string _rewardId;

    public string StoryId { get => _storyId; set => _storyId = value; }
    public string ChapterId { get => _chapterId; set => _chapterId = value; }
    public int EpisodeNumber { get => _episodeNumber; set => _episodeNumber = value; }
    public string Title { get => _title; set => _title = value; }

    public EUnlockType UnlockType { get => _unlockType; set => _unlockType = value; }
    public string UnlockTargetId { get => _unlockTargetId; set => _unlockTargetId = value; }

    public string FirstBackgroundId { get => _firstBackgroundId; set => _firstBackgroundId = value; }
    public string FirstLineId { get => _firstLineId; set => _firstLineId = value; }

    public string RewardId { get => _rewardId; set => _rewardId = value; }
}
