using System;
using UnityEngine;

/// <summary>
/// 스토리 한 편에 대한 플레이어의 진행 상태입니다.
///
/// 제목·해금 조건 같은 기획 값은 마스터 데이터(StoryData)에 있고, 여기에는 플레이어마다 달라지는 것만 둡니다.
///
/// 값을 바꾸는 것은 진행 서비스뿐입니다. 화면에는 IStoryProgress로 넘겨 읽기만 하도록 합니다.
/// </summary>
[Serializable]
public class StoryProgress : IStoryProgress
{
    [SerializeField]
    private EStoryStatus _status = EStoryStatus.LOCKED;

    [SerializeField]
    private bool _isNew = false;

    [SerializeField]
    private bool _isStarted = false;

    public EStoryStatus Status { get => _status; set => _status = value; }

    /// <summary>
    /// 목록에 NEW 배지를 띄울지 여부입니다. 해금될 때 켜지고 감상 화면에 진입하면 꺼집니다(기획서 3-F-3).
    /// 해제 상태가 재실행 후에도 유지되어야 하므로(16-14) 세이브에 남깁니다.
    /// </summary>
    public bool IsNew { get => _isNew; set => _isNew = value; }

    /// <summary>
    /// 감상 화면에 한 번이라도 들어갔는지 여부입니다. 대사 위치는 저장하지 않으므로 들어갔다는 사실만 남깁니다.
    ///
    /// NEW로는 이것을 알 수 없습니다. 해금 조건이 없는 스토리는 처음부터 NEW가 꺼진 채 만들어져,
    /// NEW가 꺼졌다고 들어가 본 것은 아니기 때문입니다. 이 필드가 생기기 전의 세이브는 꺼진 채 읽힙니다.
    /// </summary>
    public bool IsStarted { get => _isStarted; set => _isStarted = value; }

    public bool IsCompleted => _status == EStoryStatus.COMPLETED;

    /// <summary>
    /// 들어갔다가 끝내지 않은 스토리입니다(개발 기획서 5장 '진행 중'). 홈 카드 배지와 이어하기 안내가 같은 판정을 씁니다.
    /// </summary>
    public bool IsInProgress => _isStarted && CanEnter && !IsCompleted;

    /// <summary>
    /// 감상할 수 있는 상태인지 여부입니다. 완료한 스토리도 다시 볼 수 있습니다(기획서 5.5).
    /// </summary>
    public bool CanEnter => _status != EStoryStatus.LOCKED;
}
