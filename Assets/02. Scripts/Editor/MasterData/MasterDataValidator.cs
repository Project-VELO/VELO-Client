/// <summary>
/// 마스터 데이터 검사를 순서대로 실행하고 결과를 하나로 모읍니다.
///
/// ID를 손으로 적는 이상 오타는 반드시 납니다. 잘못된 ID는 게임을 죽이지 않고 조용히 "없는 것처럼" 동작하므로,
/// 화면을 만들기 전에 여기서 걸러내지 않으면 원인을 찾는 데 훨씬 오래 걸립니다.
///
/// 검사 대상별 판정은 각 Validator가 맡고, 이 클래스는 실행 순서와 데이터 갱신만 책임집니다.
/// </summary>
public class MasterDataValidator
{
    private readonly CharacterCardValidator _characterCardValidator = new CharacterCardValidator();
    private readonly StoryValidator _storyValidator = new StoryValidator();
    private readonly ScheduleValidator _scheduleValidator = new ScheduleValidator();
    private readonly NewGameConfigValidator _newGameConfigValidator = new NewGameConfigValidator();
    private readonly StoryScriptValidator _scriptValidator = new StoryScriptValidator();

    public MasterDataValidationReport Validate()
    {
        var report = new MasterDataValidationReport();

        // 검사 대상은 항상 디스크의 최신 내용이어야 하므로 캐시를 쓰지 않고 다시 읽습니다.
        MasterDataProvider provider = MasterDataProvider.Instance;
        provider.Rebuild();
        LiveSongCatalog.Instance.Rebuild();

        AddCounts(provider, report);

        _characterCardValidator.Validate(provider, report);
        _storyValidator.Validate(provider, report);
        _scheduleValidator.Validate(provider, report);
        _newGameConfigValidator.Validate(provider, report);
        _scriptValidator.Validate(report);

        return report;
    }

    private void AddCounts(MasterDataProvider provider, MasterDataValidationReport report)
    {
        report.AddSummary($"캐릭터 {provider.Characters.Count} / 카드 {provider.Cards.Count} / 아이템 {provider.Items.Count}");
        report.AddSummary($"보상 {provider.Rewards.Count} / 스토리 {provider.Stories.Count} / 스케줄 {provider.Schedules.Count}");
    }
}
