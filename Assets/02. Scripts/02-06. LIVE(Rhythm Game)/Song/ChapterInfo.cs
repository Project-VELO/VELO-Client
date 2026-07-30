using System;
using UnityEngine;

/// <summary>
/// 수록 공간의 챕터 폴더에 함께 두는 chapter_info.json의 DTO 클래스입니다.
/// 챕터 소속 자체는 폴더가 결정하고, 이 파일은 화면 표시명과 해금 조건만 보충합니다.
/// 파일이 없어도 챕터는 폴더명으로 동작하므로 작성은 선택입니다.
/// </summary>
[Serializable]
public class ChapterInfo
{
    [SerializeField]
    private string _displayName;

    [SerializeField]
    private string _unlockStoryId;

    public string DisplayName { get => _displayName; set => _displayName = value; }
    public string UnlockStoryId { get => _unlockStoryId; set => _unlockStoryId = value; }
}
