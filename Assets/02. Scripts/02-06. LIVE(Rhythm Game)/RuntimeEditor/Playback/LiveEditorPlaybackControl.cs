using UnityEngine;
using VInspector;

/// <summary>
/// 재생/일시정지와 마디·분박 단위 위치 이동을 전담합니다.
/// 정지 상태에서는 Update의 스크롤 갱신이 재생 시각을 따라오지 않으므로, 위치를 옮긴 직후 직접 갱신해 줍니다.
///
/// 위치 이동은 테스트 플레이 중에도 씁니다. 다만 판정 기준 시각은 LiveEditorTestPlayController가
/// 쥐고 있으므로 옮긴 사실을 알려 다시 맞추게 합니다.
///
/// 재생/일시정지만은 테스트 플레이 중에 막습니다. 여기서 상태를 바꾸면 종료 절차를 거치지 않고
/// 풀려 버려 판정 UI와 입력이 켜진 채로 남기 때문입니다. 중단은 테스트 플레이 버튼으로 합니다.
/// </summary>
public class LiveEditorPlaybackControl : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private LiveAudioPlayer _audioPlayer;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    [SerializeField]
    private LiveEditorTestPlayController _testPlayController;

    /// <summary>
    /// 재생 위치를 절대 시각으로 옮깁니다. 진행바로 임의 지점을 찍어 이동할 때 사용합니다.
    /// </summary>
    public void SetPlaybackTime(int timeMs)
    {
        _audioPlayer.SetPlaybackTime(timeMs);
        RefreshAfterSeek();
    }

    public void SetPlaybackBar(int barIndex)
    {
        if (!_timeline.BarLayout.IsBuilt)
        {
            return;
        }

        SetPlaybackTime(_timeline.BarLayout.GetBarStartTimeMs(barIndex));
    }

    public void SeekByBar(int direction)
    {
        SetPlaybackBar(_timeline.GetCurrentBarIndex() + direction);
    }

    public void SeekByGridStep(int direction)
    {
        _audioPlayer.SeekByGridStep(direction, _timeline.SnapDivision);
        RefreshAfterSeek();
    }

    public void TogglePlayPause()
    {
        if (_testPlayController.IsTestPlaying || ReferenceEquals(_controller.CurrentChart, null))
        {
            return;
        }

        if (_audioPlayer.IsPlaying)
        {
            _controller.SetState(LiveEditorController.EEditorState.Paused);
            return;
        }

        _audioPlayer.Play();
        _controller.SetState(LiveEditorController.EEditorState.Editing);
    }

    private void RefreshAfterSeek()
    {
        _timeline.RefreshScroll(_audioPlayer.CurrentTimeMs);
        _testPlayController.NotifySeeked();
    }
}
