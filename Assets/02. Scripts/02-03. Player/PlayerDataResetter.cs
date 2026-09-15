/// <summary>
/// 플레이 기록을 모두 지우고 처음 실행한 상태로 되돌립니다.
///
/// 전시장처럼 한 컴퓨터를 여러 사람이 이어서 쓸 때, 다음 사람이 앞사람의 진행을 물려받지 않게 하는 용도입니다.
///
/// 세이브만 지우면 부족합니다. 화면을 오가며 넘겨주는 값(고른 곡, 편성, 결과, 스토리 강조)이 실행 동안
/// 싱글톤에 남아 있어 새 세이브와 어긋납니다. 그래서 함께 비웁니다.
///
/// 언어는 건드리지 않습니다. 플레이 기록이 아니라 기기 설정이고, 같은 설정 팝업에서 바로 다시 고를 수 있습니다.
/// </summary>
public static class PlayerDataResetter
{
    public static void ResetAll()
    {
        PlayerDataProvider.Instance.ResetToNewGame();
        ClearSessionContexts();
    }

    private static void ClearSessionContexts()
    {
        StoryEntryContext.Instance.Clear();
        LiveEntryContext.Instance.Clear();
        LiveLoadoutContext.Instance.Clear();
        LiveResultContext.Instance.Clear();
    }
}
