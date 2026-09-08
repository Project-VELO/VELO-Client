using System;

/// <summary>
/// 번역된 대사 한 줄입니다. 원본 대본의 같은 LineId 줄을 덮어씁니다.
///
/// 순서가 아니라 LineId로 짚는 것은 대본이 다시 생성되면서 줄이 늘거나 줄어들 수 있기 때문입니다.
/// 순서로 짚으면 한 줄만 어긋나도 그 뒤가 전부 다른 줄에 붙습니다.
/// </summary>
[Serializable]
public class StoryTextOverlayLine
{
    public int LineId;

    /// <summary>
    /// 번역된 화자 표기입니다. 비어 있으면 원본의 화자명을 그대로 씁니다.
    /// </summary>
    public string SpeakerName;

    /// <summary>
    /// 번역된 문장입니다. 비어 있으면 원본 문장이 그대로 나갑니다.
    /// 아직 번역되지 않은 줄을 빈칸으로 두면 화면이 비지 않고 원문으로 떨어집니다.
    /// </summary>
    public string Text;
}
