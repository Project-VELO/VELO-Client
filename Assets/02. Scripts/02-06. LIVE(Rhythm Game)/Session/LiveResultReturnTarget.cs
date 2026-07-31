using UnityEngine;

/// <summary>
/// 결과 화면의 확인 버튼이 향할 화면을 진입 경로별로 정합니다(기획서 3-J-6).
/// 곡 선택 화면의 뒤로가기(LiveEntryBackTarget)와는 목적지가 다르므로 표를 따로 둡니다.
/// </summary>
public static class LiveResultReturnTarget
{
    public static ESceneNames GetReturnScene(EEntryType entryType)
    {
        switch (entryType)
        {
            // 홈 LIVE와 연습실 LIVE는 각자의 곡 선택 화면으로 돌아갑니다. 어느 쪽 목록을 열지는 진입 컨텍스트가 정합니다.
            case EEntryType.HOME_LIVE:
            case EEntryType.PRACTICE_LIVE:
                return ESceneNames.MusicSelectScene;

            case EEntryType.SCHEDULE_LIVE:
                return ESceneNames.OfficeScene;

            default:
                Debug.LogWarning($"[LiveResultReturnTarget] 복귀 위치가 정의되지 않은 진입 경로입니다: {entryType}");
                return ESceneNames.HomeScene;
        }
    }
}
