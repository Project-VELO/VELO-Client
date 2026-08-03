using System;
using UnityEngine;

/// <summary>
/// 스토리 한 편에 대한 플레이어의 진행 상태입니다.
///
/// 제목·해금 조건 같은 기획 값은 마스터 데이터(StoryData)에 있고, 여기에는 플레이어마다 달라지는 것만 둡니다.
/// </summary>
[Serializable]
public class StoryProgress
{
    [SerializeField]
    private EStoryStatus _status = EStoryStatus.LOCKED;

    [SerializeField]
    private bool _isNew = false;

    public EStoryStatus Status { get => _status; set => _status = value; }

    /// <summary>
    /// 목록에 NEW 배지를 띄울지 여부입니다. 해금될 때 켜지고 감상 화면에 진입하면 꺼집니다(기획서 3-F-3).
    /// 해제 상태가 재실행 후에도 유지되어야 하므로(16-14) 세이브에 남깁니다.
    /// </summary>
    public bool IsNew { get => _isNew; set => _isNew = value; }

    public bool IsCompleted => _status == EStoryStatus.COMPLETED;

    /// <summary>
    /// 감상할 수 있는 상태인지 여부입니다. 완료한 스토리도 다시 볼 수 있습니다(기획서 5.5).
    /// </summary>
    public bool CanEnter => _status != EStoryStatus.LOCKED;
}
