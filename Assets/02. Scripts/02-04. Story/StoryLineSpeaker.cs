/// <summary>
/// 대사 한 줄의 화자명을 정합니다.
///
/// 감상 화면과 스크립트 확인 팝업 두 곳이 같은 규칙을 써야 해서 한곳에 모았습니다.
/// 갈라 두면 대사 상자에는 이름이 나오는데 로그에는 빈칸이 되는 식으로 어긋납니다.
/// </summary>
public static class StoryLineSpeaker
{
    /// <summary>
    /// 화면에 낼 화자명을 돌려줍니다. 화자가 없으면 빈 문자열입니다.
    ///
    /// characters.json에 등록된 인물이면 그 표시명을 씁니다. 이름이 바뀌면 한 곳만 고치면 되기 때문입니다.
    /// 단역은 등록돼 있지 않으므로 원고에 적힌 표기를 그대로 씁니다.
    /// </summary>
    public static string GetDisplayName(StoryLineData line)
    {
        if (line.LineType == ELineType.NARRATION)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(line.SpeakerId))
        {
            return MasterDataProvider.Instance.GetCharacterDisplayName(line.SpeakerId);
        }

        // 공백만 든 칸은 이름이 없는 것으로 봅니다. 그대로 내보내면 빈 이름표만 뜹니다.
        return string.IsNullOrWhiteSpace(line.SpeakerName) ? string.Empty : line.SpeakerName;
    }
}
