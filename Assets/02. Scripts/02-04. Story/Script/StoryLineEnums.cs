/// <summary>
/// 대사 한 줄의 이름 문자열을 열거값으로 바꿉니다.
///
/// StoryLineData에서 떼어낸 이유는 바뀌는 이유가 다르기 때문입니다. 그쪽은 대본에 어떤 열이
/// 있는지를 담고, 이쪽은 그 이름을 무엇으로 읽을지를 정합니다. 열이 늘 때마다 변환 규칙이
/// 자료 정의 사이에 끼어들면 어떤 열이 있는지 한눈에 보이지 않습니다.
/// </summary>
public static class StoryLineEnums
{
    /// <summary>
    /// 이름 열을 읽어 열거값 자리를 채웁니다. 역직렬화 직후 한 번 부릅니다.
    /// 없는 이름은 경고와 함께 기본값으로 떨어집니다(MasterDataEnum).
    /// </summary>
    public static void Apply(StoryLineData line)
    {
        line.LineType = Parse(line.LineTypeName, ELineType.NARRATION, line.LineId, nameof(line.LineTypeName));
        line.TextSpeed = Parse(line.TextSpeedName, ETextSpeed.NORMAL, line.LineId, nameof(line.TextSpeedName));
        line.TextPlacement = Parse(line.TextPlacementName, EStoryTextPlacement.DIALOG_BOX, line.LineId,
            nameof(line.TextPlacementName));

        line.Transition = ParseTransition(line.TransitionName, line.LineId, nameof(line.TransitionName));
        line.CenterTransition = ParseTransition(line.CenterTransitionName, line.LineId, nameof(line.CenterTransitionName));
        line.RightTransition = ParseTransition(line.RightTransitionName, line.LineId, nameof(line.RightTransitionName));
        line.UpperTransition = ParseTransition(line.UpperTransitionName, line.LineId, nameof(line.UpperTransitionName));
    }

    /// <summary>
    /// 열거값 자리를 이름 열로 되돌립니다. 직렬화 직전에 한 번 부릅니다.
    /// 등장 방식은 사람이 적는 열이라, 비어 있던 칸을 FADE로 채워 돌려주지 않습니다.
    /// </summary>
    public static void ToNames(StoryLineData line)
    {
        line.LineTypeName = MasterDataEnum.ToName(line.LineType);
        line.TextSpeedName = MasterDataEnum.ToName(line.TextSpeed);
        line.TextPlacementName = MasterDataEnum.ToName(line.TextPlacement);
    }

    private static EStoryCharacterTransition ParseTransition(string name, int lineId, string fieldName)
    {
        return Parse(name, EStoryCharacterTransition.FADE, lineId, fieldName);
    }

    private static T Parse<T>(string name, T fallback, int lineId, string fieldName)
        where T : struct, System.Enum
    {
        return MasterDataEnum.Parse(name, fallback, $"{nameof(StoryLineData)}(line {lineId}).{fieldName}");
    }
}
