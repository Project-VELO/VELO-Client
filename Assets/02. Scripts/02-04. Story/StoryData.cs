using System;
using UnityEngine;

/// <summary>
/// 스토리 한 회차의 기획 메타데이터입니다(stories.json).
///
/// 진행 상태(LOCKED/UNLOCKED/COMPLETED)와 NEW 표시 여부는 플레이어마다 다르므로 여기가 아니라 세이브 데이터에 있습니다.
/// 이 클래스는 어느 플레이어에게나 같은 값만 담습니다.
///
/// 대사는 StoryScripts/{StoryId}.json에 따로 있으며, 경로는 MasterDataPaths가 StoryId에서 유도합니다.
/// 첫 배경과 첫 대사 역시 대본 파일의 첫 줄이 이미 갖고 있어 여기에 중복해 두지 않습니다.
///
/// 필드가 public인 것은 의도적입니다. 사람이 직접 편집하는 JSON이라
/// [SerializeField] private 방식의 밑줄 접두사가 키에 노출되면 곤란합니다(Convention 1-b-(2)).
/// </summary>
[Serializable]
public class StoryData : ISerializationCallbackReceiver
{
    public string StoryId;
    public string ChapterId;

    /// <summary>
    /// 스토리 목록에서 챕터를 위에서 아래로 배치할 때 쓰는 순서입니다(기획서 3-F-3-1).
    /// 챕터 ID 문자열만으로는 PROLOGUE가 CHAPTER01보다 앞이라는 것을 알 수 없습니다.
    /// </summary>
    public int ChapterOrder;

    /// <summary>
    /// 챕터 안에서의 회차 번호입니다. 1부터 시작합니다.
    /// </summary>
    public int EpisodeNumber;

    public string Title;

    /// <summary>
    /// JSON에 적는 해금 조건 이름입니다("NONE" / "WEEK_CLEAR" / "PREVIOUS_STORY_CLEAR").
    /// </summary>
    public string UnlockTypeName;

    /// <summary>
    /// 해금에 필요한 대상 ID입니다. UnlockType에 따라 주차 ID 또는 선행 스토리 ID가 들어갑니다.
    /// UnlockType이 NONE이면 비어 있습니다.
    /// </summary>
    public string UnlockTargetId;

    /// <summary>
    /// 최초 완료 시 지급할 보상입니다(기획서 3-J-4). 재감상 시에는 지급하지 않습니다.
    /// </summary>
    public string RewardId;

    /// <summary>
    /// UnlockTypeName을 변환한 값입니다. 역직렬화 직후 채워집니다.
    /// </summary>
    [NonSerialized]
    public EUnlockType UnlockType;

    public void OnBeforeSerialize()
    {
        UnlockTypeName = MasterDataEnum.ToName(UnlockType);
    }

    public void OnAfterDeserialize()
    {
        UnlockType = MasterDataEnum.Parse(UnlockTypeName, EUnlockType.NONE, $"{nameof(StoryData)}({StoryId}).{nameof(UnlockTypeName)}");
    }
}
