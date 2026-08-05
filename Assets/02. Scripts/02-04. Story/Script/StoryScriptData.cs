using System;
using System.Collections.Generic;

/// <summary>
/// 스토리 한 회차의 대사 전체입니다. StoryScripts/{storyId}.json의 최상위 객체입니다.
///
/// 회차별로 파일을 나누는 이유는 채보를 곡별 파일로 두는 것과 같습니다.
/// 31화를 한 파일에 담으면 감상 화면에 들어갈 때마다 전체를 읽어야 합니다.
/// </summary>
[Serializable]
public class StoryScriptData
{
    public int SchemaVersion = MasterDataSchema.CURRENT_VERSION;

    /// <summary>
    /// 이 대본이 속한 스토리입니다. 파일명과 일치해야 하며, 검증 도구가 대조합니다.
    /// </summary>
    public string StoryId;

    public List<StoryLineData> Lines = new List<StoryLineData>();
}
