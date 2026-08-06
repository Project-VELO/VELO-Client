using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 리듬게임 한 판의 진행 상태를 관리합니다(SCREEN-009).
/// 카운트다운 → 플레이 → 일시정지 → 종료로 이어지는 전환과 입력 전달만 맡습니다.
///
/// 일시정지에 Time.timeScale을 쓰지 않습니다. 음악은 어차피 따로 멈춰야 하고,
/// SceneTransitionManager가 화면을 옮길 때마다 timeScale을 1로 되돌리기 때문에 상태로 다루는 편이 안전합니다.
/// </summary>
public class LiveGameController : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private LivePlaySession _session;

    [SerializeField]
    private LiveConductor _conductor;

    [SerializeField]
    private LiveJudgementProcessor _judgementProcessor;

    [SerializeField]
    private LivePlayInput _playInput;

    [SerializeField]
    private LiveCountdown _countdown;

    [SerializeField]
    private LiveResultDispatcher _resultDispatcher;

    [SerializeField]
    private UI_Live _liveUI;

    private ELiveGameState _state = ELiveGameState.Loading;

    public ELiveGameState State => _state;

    private void Awake()
    {
        _liveUI.BindJudgement(_judgementProcessor);

        _playInput.OnLanePressed += PressLane;
        _playInput.OnLaneReleased += ReleaseLane;
        _judgementProcessor.OnGhostFailed += FinishPlay;
        _conductor.AudioPlayer.OnClipLoaded += OnAudioClipLoaded;
    }

    private void Start()
    {
        _playInput.SetAcceptingInput(false);

        if (!_session.TryInitSession())
        {
            LoadBackScene();
        }
    }

    private void OnDestroy()
    {
        _playInput.OnLanePressed -= PressLane;
        _playInput.OnLaneReleased -= ReleaseLane;
        _judgementProcessor.OnGhostFailed -= FinishPlay;
        _conductor.AudioPlayer.OnClipLoaded -= OnAudioClipLoaded;
    }

    private void Update()
    {
        if (_state == ELiveGameState.Loading)
        {
            return;
        }

        // 카운트다운과 일시정지 중에도 노트를 그려 두어야 하므로 스크롤 갱신은 상태와 무관하게 돌립니다.
        _session.RefreshTrack(_conductor.SongTimeMs);

        if (_state != ELiveGameState.Playing)
        {
            return;
        }

        _judgementProcessor.RefreshExpiredNotes(_conductor.SongTimeMs);

        // 귀신 실패로 상태가 이미 Finishing이면 FinishPlay가 스스로 걸러 내므로 여기서 다시 확인하지 않습니다.
        if (_conductor.IsSongFinished)
        {
            FinishPlay();
        }
    }

    /// <summary>
    /// 플레이 중일 때만 멈추고, 실제로 멈췄는지를 돌려줍니다. 팝업을 띄우는 쪽이 이 결과를 보고 진행합니다.
    /// </summary>
    public bool TryPause()
    {
        if (_state != ELiveGameState.Playing)
        {
            return false;
        }

        _state = ELiveGameState.Paused;
        _conductor.Pause();
        _playInput.SetAcceptingInput(false);

        return true;
    }

    public void ResumePlay()
    {
        if (_state != ELiveGameState.Paused)
        {
            return;
        }

        StartCountdownAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void RestartPlay()
    {
        _session.ResetSession();
        StartCountdownAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    /// <summary>
    /// 중도 종료는 보상도 기록도 남기지 않으므로(3-J-3) 결과를 만들지 않고 진입 경로로 되돌아갑니다.
    /// </summary>
    public void QuitPlay()
    {
        _conductor.Stop();
        _playInput.SetAcceptingInput(false);
        LiveResultContext.Instance.Clear();

        LoadBackScene();
    }

    private void OnAudioClipLoaded()
    {
        _session.InitBarLayout();
        StartCountdownAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    /// <summary>
    /// 곡 시작과 일시정지 재개가 모두 "3초 센 뒤 현재 위치에서 재생"이므로 한 경로로 묶었습니다.
    /// </summary>
    private async UniTaskVoid StartCountdownAsync(CancellationToken cancellationToken)
    {
        _state = ELiveGameState.Countdown;
        _playInput.SetAcceptingInput(false);

        await _countdown.PlayAsync(cancellationToken);

        _conductor.Play();
        _playInput.SetAcceptingInput(true);
        _state = ELiveGameState.Playing;
    }

    private void PressLane(int lane)
    {
        if (_state != ELiveGameState.Playing)
        {
            return;
        }

        if (_liveUI.LaneFeedback != null)
        {
            _liveUI.LaneFeedback.RefreshLanePress(lane);
        }

        _judgementProcessor.PressLane(lane, _conductor.SongTimeMs);
    }

    private void ReleaseLane(int lane)
    {
        if (_state != ELiveGameState.Playing)
        {
            return;
        }

        _judgementProcessor.ReleaseLane(lane, _conductor.SongTimeMs);
    }

    /// <summary>
    /// 완주와 귀신 노트 실패가 모두 여기로 모입니다. 귀신 실패는 판정 도중에 통지되므로 상태를 먼저 확인합니다.
    /// </summary>
    private void FinishPlay()
    {
        if (_state != ELiveGameState.Playing)
        {
            return;
        }

        _state = ELiveGameState.Finishing;
        _conductor.Stop();
        _playInput.SetAcceptingInput(false);

        // 귀신 실패가 아니라면 곡이 끝난 시점에 남아 있던 노트까지 마저 BAD로 확정하고 결과를 냅니다.
        _judgementProcessor.FlushRemainingNotes();

        _resultDispatcher.Dispatch();
    }

    private void LoadBackScene()
    {
        ESceneNames backScene = LiveEntryBackTarget.GetBackScene(LiveEntryContext.Instance.EntryType);
        LiveSceneNavigator.LoadScene(backScene, this.GetCancellationTokenOnDestroy(), nameof(LiveGameController));
    }
}
