using System;
using UnityEngine;
using VInspector;

/// <summary>
/// 채보 에디터의 테스트 플레이 진입과 종료를 담당합니다(SCREEN-008).
///
/// 곡을 처음부터 새로 트는 별도 모드가 아니라, 편집 상태에서 노트 편집만 막고 입력과 판정을 켠 상태입니다.
/// 멈춘 자리에서 곧바로 채보를 고칠 수 있어야 배치를 반복 확인할 수 있으므로,
/// 진입·종료 어느 쪽에서도 재생 위치를 옮기지 않습니다.
///
/// LiveConductor가 dspTime으로 재생 시각을 보간하므로 배속 1.0을 전제로 합니다.
/// 배속이 걸린 채로는 판정 시각과 실제 소리가 어긋나므로 테스트 중에는 배속을 잠급니다.
/// </summary>
public class LiveEditorTestPlayController : MonoBehaviour
{
    /// <summary>
    /// 테스트 플레이의 시작과 종료를 알립니다. 버튼 문구를 바꾸는 쪽이 구독합니다.
    /// </summary>
    public Action OnTestPlayStateChanged;

    // 진행바를 끌고 있는 동안은 이동이 매 프레임 들어오므로, 이 프레임 수만큼 이동이 없을 때 멎은 것으로 봅니다.
    private const int SEEK_SETTLE_FRAMES = 1;

    [Foldout("Hierarchy")]
    [SerializeField]
    private LiveEditorController _controller;

    [SerializeField]
    private LiveEditorTimeline _timeline;

    [SerializeField]
    private LiveAudioPlayer _audioPlayer;

    [SerializeField]
    private LiveConductor _conductor;

    [SerializeField]
    private LivePlayInput _playInput;

    [SerializeField]
    private LiveJudgementProcessor _judgementProcessor;

    [SerializeField]
    private UI_LiveEditorPlaybackSpeedControl _speedControl;

    [Tooltip("테스트 플레이 중에만 켜지는 점수·콤보·판정 표시입니다. 리듬게임 씬과 같은 UI를 그대로 씁니다.")]
    [SerializeField]
    private UI_Live _liveUI;

    private bool _hasPendingSeek;
    private int _lastSeekFrame;

    public bool IsTestPlaying => _controller.State == LiveEditorController.EEditorState.TestPlay;

    private void Awake()
    {
        _playInput.SetAcceptingInput(false);
        _playInput.OnLanePressed += PressLane;
        _playInput.OnLaneReleased += ReleaseLane;

        // 귀신 노트가 실패하면 이후 입력이 더 이상 집계되지 않아 테스트를 이어갈 의미가 없으므로 곧바로 멈춥니다.
        _judgementProcessor.OnGhostFailed += StopTestPlay;
        _judgementProcessor.OnNoteJudged += HideJudgedNote;

        if (_liveUI != null)
        {
            _liveUI.BindJudgement(_judgementProcessor);
        }

        SetHudVisible(false);
    }

    private void OnDestroy()
    {
        _playInput.OnLanePressed -= PressLane;
        _playInput.OnLaneReleased -= ReleaseLane;
        _judgementProcessor.OnGhostFailed -= StopTestPlay;
        _judgementProcessor.OnNoteJudged -= HideJudgedNote;
    }

    private void Update()
    {
        if (!IsTestPlaying)
        {
            return;
        }

        if (_hasPendingSeek)
        {
            RefreshPendingSeek();
            return;
        }

        // 테스트 플레이 중에는 LiveEditorController가 스크롤 갱신에서 손을 떼므로 여기서 대신 돌립니다.
        // 판정과 같은 시각(LiveConductor)으로 그려야 노트가 판정선에 닿는 순간과 판정 결과가 어긋나지 않습니다.
        _timeline.RefreshScroll(_conductor.SongTimeMs);
        _judgementProcessor.RefreshExpiredNotes(_conductor.SongTimeMs);

        if (_conductor.IsSongFinished)
        {
            StopTestPlay();
        }
    }

    public void ToggleTestPlay()
    {
        if (IsTestPlaying)
        {
            StopTestPlay();
            return;
        }

        StartTestPlay();
    }

    /// <summary>
    /// 지금 보고 있는 재생 지점에서 그대로 판정을 켭니다. 위치를 옮기지 않으므로 편집하던 구간을 곧바로 쳐 볼 수 있습니다.
    /// </summary>
    public void StartTestPlay()
    {
        if (IsTestPlaying)
        {
            return;
        }

        if (ReferenceEquals(_controller.CurrentChart, null) || !_timeline.BarLayout.IsBuilt)
        {
            Debug.LogWarning("[LiveEditorTestPlayController] 편집 중인 채보가 없거나 음원이 아직 로드되지 않아 테스트 플레이를 시작할 수 없습니다.");
            return;
        }

        _speedControl.ResetSpeed();
        _speedControl.SetInteractable(false);

        _controller.SetState(LiveEditorController.EEditorState.TestPlay);
        SetHudVisible(true);

        InitJudgementFromCurrentTime();

        _conductor.Play();
        _playInput.SetAcceptingInput(true);

        OnTestPlayStateChanged?.Invoke();
    }

    /// <summary>
    /// 멈춘 그 자리에서 곧바로 채보를 이어 고칠 수 있도록, 재생 위치는 그대로 두고 판정으로 가려 둔 노트만 되살립니다.
    /// </summary>
    public void StopTestPlay()
    {
        if (!IsTestPlaying)
        {
            return;
        }

        _conductor.Stop();
        _playInput.SetAcceptingInput(false);
        _hasPendingSeek = false;

        SetHudVisible(false);
        _speedControl.SetInteractable(true);

        _controller.SetState(LiveEditorController.EEditorState.Paused);

        RestoreNoteVisuals();
        _timeline.RefreshScroll(_audioPlayer.CurrentTimeMs);

        OnTestPlayStateChanged?.Invoke();
    }

    /// <summary>
    /// 재생 위치가 밖에서 옮겨졌음을 알립니다(진행바, 마디·분박 탐색).
    /// 판정 기준을 새 위치로 다시 세워야 하는데, 진행바를 끄는 동안에는 매 프레임 들어오므로
    /// 여기서는 음악과 입력만 멈춰 두고 실제 재구성은 이동이 멎은 뒤 한 번만 수행합니다.
    /// </summary>
    public void NotifySeeked()
    {
        if (!IsTestPlaying)
        {
            return;
        }

        _conductor.Pause();
        _playInput.SetAcceptingInput(false);

        _hasPendingSeek = true;
        _lastSeekFrame = Time.frameCount;
    }

    private void RefreshPendingSeek()
    {
        _timeline.RefreshScroll(_audioPlayer.CurrentTimeMs);

        if (Time.frameCount <= _lastSeekFrame + SEEK_SETTLE_FRAMES)
        {
            return;
        }

        _hasPendingSeek = false;

        RestoreNoteVisuals();
        InitJudgementFromCurrentTime();

        _conductor.Play();
        _playInput.SetAcceptingInput(true);
    }

    /// <summary>
    /// 현재 재생 위치를 시작점으로 삼아 집계를 다시 세웁니다.
    /// 앞선 노트까지 실으면 첫 프레임에 한꺼번에 BAD로 쏟아지므로 시작 시각을 넘겨 걸러 냅니다.
    /// </summary>
    private void InitJudgementFromCurrentTime()
    {
        _judgementProcessor.InitSession(_controller.CurrentChart, _audioPlayer.CurrentTimeMs);
    }

    private void RestoreNoteVisuals()
    {
        _timeline.NoteRenderer.ClearHiddenNotes();
        _timeline.RefreshNoteVisuals();
    }

    /// <summary>
    /// 판정이 끝난 노트를 화면에서 지웁니다. 판정기는 화면을 모르므로, 타임라인을 쥔 이 클래스가 통지를 받아 처리합니다.
    /// note가 null이면 노트 없이 귀신 레인을 누른 오입력이라 지울 노트가 없습니다.
    /// </summary>
    private void HideJudgedNote(NoteData note, EJudgement judgement)
    {
        if (note == null)
        {
            return;
        }

        _timeline.NoteRenderer.HideNote(note.NoteId);
    }

    private void PressLane(int lane)
    {
        if (!IsTestPlaying)
        {
            return;
        }

        _judgementProcessor.PressLane(lane, _conductor.SongTimeMs);
    }

    private void ReleaseLane(int lane)
    {
        if (!IsTestPlaying)
        {
            return;
        }

        _judgementProcessor.ReleaseLane(lane, _conductor.SongTimeMs);
    }

    private void SetHudVisible(bool isVisible)
    {
        if (_liveUI == null)
        {
            return;
        }

        _liveUI.SetPlayHudVisible(isVisible);
    }
}
