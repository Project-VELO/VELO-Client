using UnityEngine;

/// <summary>
/// LIVE 진입 경로별 복귀 위치를 결정합니다.
/// 기획서 SCREEN-007의 뒤로가기 표(홈 LIVE→홈, 스케줄 LIVE→사무실, 연습실 LIVE→스페이스)를 코드로 고정합니다.
/// </summary>
public static class LiveEntryBackTarget
{
    public static ESceneNames GetBackScene(EEntryType entryType)
    {
        switch (entryType)
        {
            case EEntryType.HOME_LIVE:
                return ESceneNames.HomeScene;

            case EEntryType.SCHEDULE_LIVE:
                return ESceneNames.OfficeScene;

            case EEntryType.PRACTICE_LIVE:
                return ESceneNames.SpaceScene;

            default:
                Debug.LogWarning($"[LiveEntryBackTarget] 복귀 위치가 정의되지 않은 진입 경로입니다: {entryType}");
                return ESceneNames.HomeScene;
        }
    }
}
