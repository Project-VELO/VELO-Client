using UnityEngine;
using VInspector;

/// <summary>
/// 재생/일시정지와 마디·분박 단위 위치 이동을 전담합니다.
/// 정지 상태에서는 Update의 스크롤 갱신이 재생 시각을 따라오지 않으므로, 위치를 옮긴 직후 직접 갱신해 줍니다.
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

    /// <summary>
    /// 재생 위치를 절대 시각으로 옮깁니다. 진행바로 임의 지점을 찍어 이동할 때 사용합니다.
    /// </summary>
    public void SetPlaybackTime(int timeMs)
    {
        _audioPlayer.SetPlaybackTime(timeMs);
        _timeline.RefreshScroll(_audioPlayer.CurrentTimeMs);
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
        _timeline.RefreshScroll(_audioPlayer.CurrentTimeMs);
    }

    public void TogglePlayPause()
    {
        if (ReferenceEquals(_controller.CurrentChart, null))
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
}
