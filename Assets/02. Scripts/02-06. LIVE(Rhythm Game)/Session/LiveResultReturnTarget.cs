/// <summary>
/// 결과 화면의 확인 버튼이 향할 화면을 정합니다.
///
/// 기준은 진입 경로가 아니라 "이번 결과로 일일 스케줄이 완료되었는가"입니다(기획서 9.4).
/// 홈 일반 LIVE로도 조건이 맞으면 스케줄이 완료되므로(기획서 16-6), 진입 경로로 가르면
/// 같은 사건(스케줄 완료)이 경로에 따라 다른 화면으로 이어집니다.
/// 완료가 없으면 3-J-6대로 일반 LIVE 곡 선택 화면입니다.
///
/// 곡 선택 화면의 뒤로가기(LiveEntryBackTarget)는 진입 위치 복귀가 맞으므로 이 규칙과 무관합니다.
/// </summary>
public static class LiveResultReturnTarget
{
    public static ESceneNames GetReturnScene(bool hasCompletedSchedule)
    {
        return hasCompletedSchedule ? ESceneNames.OfficeScene : ESceneNames.MusicSelectScene;
    }
}
