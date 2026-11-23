/// <summary>
/// 홈 프로필 세 값을 읽고 저장하는 창구입니다.
///
/// 읽을 때 빈 값을 기본값으로 해석하고, 저장은 프로필 팝업의 확인(P08)에서만 합니다(개발 기획서 3장, 15장).
/// GameProgressService에 넣지 않은 것은 그쪽이 본편 진행을 맡고 있고 이미 200줄 가까이 되기 때문입니다.
/// </summary>
public class HomeProfileService : POCOSingleton<HomeProfileService>
{
    /// <summary>
    /// 저장된 홈 캐릭터가 없을 때의 인물입니다(개발 기획서 3장 "저장된 홈 캐릭터가 없으면 리아").
    /// </summary>
    public const string DEFAULT_HOME_CHARACTER_ID = "CHAR_RIA";

    /// <summary>
    /// 기본 초상은 원문에 없어(기획자 입력) 지금 홈에 그려진 리아 초상을 그대로 씁니다.
    /// </summary>
    public const string DEFAULT_PORTRAIT_ID = "CHAR_RIA";

    /// <summary>
    /// 기본 이름도 원문에 없어 지금 홈 화면 글을 그대로 씁니다. 번역표의 한국어 원문 키이기도 합니다.
    /// </summary>
    public const string DEFAULT_PRODUCER_NAME = "프로듀서";

    private HomeProfileData Profile => PlayerDataProvider.Instance.Data.HomeProfile;

    public string PortraitId => ResolveOrDefault(Profile.PortraitId, DEFAULT_PORTRAIT_ID);
    public string HomeCharacterId => ResolveOrDefault(Profile.HomeCharacterId, DEFAULT_HOME_CHARACTER_ID);

    /// <summary>
    /// 사용자가 정한 이름은 번역하지 않고, 정하지 않았을 때만 기본 문구를 지금 언어로 보여 줍니다.
    /// </summary>
    public string ProducerName => string.IsNullOrEmpty(Profile.ProducerName)
        ? UiText.Localize(DEFAULT_PRODUCER_NAME)
        : Profile.ProducerName;

    /// <summary>
    /// 사용자가 정한 이름 그대로입니다(정하지 않았으면 빈 값). 이름을 바꾸지 않고 확인했을 때
    /// 지금 언어의 기본 문구가 이름으로 굳어 저장되지 않도록, 팝업은 이 값을 들고 있다가 그대로 넘깁니다.
    /// </summary>
    public string SavedProducerName => Profile.ProducerName;

    /// <summary>
    /// 고른 적 없는 초상, 홈 캐릭터는 빈 값 그대로입니다. 바꾸지 않고 확인했을 때 지금 기본값이 고른 값으로 굳어
    /// 저장되면, 뒤에 기본값이 정해져도(기획자 입력) 그 사용자에게는 반영되지 않습니다.
    /// </summary>
    public string SavedPortraitId => Profile.PortraitId;
    public string SavedHomeCharacterId => Profile.HomeCharacterId;

    /// <summary>
    /// 세 값을 함께 저장합니다. 저장에 실패하면 이전 값으로 되돌려 표시값과 저장값이 어긋나지 않게 합니다
    /// (개발 기획서 3장 "저장 실패: 실패 안내, 이전 저장값 보존"). 실패 안내는 SaveService의 실패 이벤트를
    /// 듣는 SaveFailureNotifier가 띄웁니다.
    /// </summary>
    public bool Apply(string portraitId, string homeCharacterId, string producerName)
    {
        HomeProfileData profile = Profile;

        string previousPortraitId = profile.PortraitId;
        string previousHomeCharacterId = profile.HomeCharacterId;
        string previousProducerName = profile.ProducerName;

        profile.PortraitId = portraitId;
        profile.HomeCharacterId = homeCharacterId;
        profile.ProducerName = producerName;

        if (PlayerDataProvider.Instance.Save())
        {
            return true;
        }

        profile.PortraitId = previousPortraitId;
        profile.HomeCharacterId = previousHomeCharacterId;
        profile.ProducerName = previousProducerName;

        return false;
    }

    private static string ResolveOrDefault(string value, string defaultValue)
    {
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }
}
