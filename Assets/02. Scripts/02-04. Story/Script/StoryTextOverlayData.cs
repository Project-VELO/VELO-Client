using System;
using System.Collections.Generic;

/// <summary>
/// 한 회차의 번역문입니다. StoryScripts/{언어}/{storyId}.json의 최상위 객체입니다.
///
/// 원본 대본에 언어별 칸을 늘리지 않고 파일을 따로 둔 이유는 두 가지입니다.
/// 대본은 기획 시트에서 다시 뽑아 덮어쓰는 파일이라 여기에 손으로 넣은 번역문은 그때 사라집니다.
/// 그리고 회차 하나를 읽을 때 쓰지 않는 언어의 문장까지 함께 읽게 됩니다.
///
/// 연출 지시(배경·표정·효과)는 담지 않습니다. 번역으로 바뀌는 것은 글자뿐입니다.
/// </summary>
[Serializable]
public class StoryTextOverlayData
{
    public int SchemaVersion = MasterDataSchema.CURRENT_VERSION;

    /// <summary>
    /// 이 번역문이 붙을 대본입니다. 파일명과 일치해야 합니다.
    /// </summary>
    public string StoryId;

    public List<StoryTextOverlayLine> Lines = new List<StoryTextOverlayLine>();
}
