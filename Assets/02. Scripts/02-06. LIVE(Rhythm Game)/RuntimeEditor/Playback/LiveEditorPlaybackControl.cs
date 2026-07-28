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
    private LiveEditorAudioPlayer _audioPlayer;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    public void SetPlaybackBar(int barIndex)
    {
        if (!_timeline.BarLayout.IsBuilt)
        {
            return;
        }

        _audioPlayer.SetPlaybackTime(_timeline.BarLayout.GetBarStartTimeMs(barIndex));
        _timeline.RefreshScroll(_audioPlayer.CurrentTimeMs);
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
