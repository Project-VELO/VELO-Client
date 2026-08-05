using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// LIVE 진입 타입을 확정하고 곡 선택 화면으로 이동하는 것을 한 호출로 묶습니다.
///
/// 화면마다 SetEntry와 씬 이동을 따로 쓰면 한 곳만 빠뜨려도 이전 진입 타입이 그대로 남습니다.
/// 예를 들어 PRACTICE_LIVE가 잔류하면 스케줄이 완료되지 않고 보상도 5%만 지급됩니다.
/// 두 동작을 붙여 두어 누락이 구조적으로 불가능하게 만듭니다.
/// </summary>
public static class LiveEntryStarter
{
    /// <summary>
    /// 진입 타입을 기록하고 곡 선택 화면으로 이동합니다.
    ///
    /// designatedSongId를 넘기면 그 곡으로 고정되고 목록에서 다른 곡을 고를 수 없습니다.
    /// 자유 선택으로 진입할 때는 두 인자를 비워 둡니다.
    ///
    /// 홈의 일일 스케줄 바로가기도 HOME_LIVE입니다. 이름만 보고 SCHEDULE_LIVE를 넣으면
    /// 뒤로가기와 결과 확인 후 복귀가 사무실로 빠집니다. SCHEDULE_LIVE는 사무실에서 진입한 경우에만 씁니다.
    /// </summary>
    public static void StartLive(
        EEntryType entryType,
        string designatedSongId = null,
        string scheduleId = null,
        CancellationToken cancellationToken = default)
    {
        LiveEntryContext.Instance.SetEntry(entryType, designatedSongId, scheduleId);

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning($"[LiveEntryStarter] SceneTransitionManager가 없어 곡 선택 화면으로 이동하지 못했습니다: {entryType}");
            return;
        }

        SceneTransitionManager.Instance.LoadSceneAsync(ESceneNames.MusicSelectScene, cancellationToken).Forget();
    }
}
