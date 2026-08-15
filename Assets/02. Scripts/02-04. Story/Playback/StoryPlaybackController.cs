using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 스토리 감상 화면의 진행을 총괄합니다(기획서 SCREEN-003).
///
/// 같은 NEXT 클릭이라도 출력 중이면 "즉시 전체 출력", 출력이 끝났으면 "다음 대사",
/// 마지막 줄이면 "완료 후 목록 복귀"로 갈라집니다(기획서 6.3). 이 분기를 상태로 고정합니다.
/// </summary>
public class StoryPlaybackController : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UI_Story _ui;

    [Foldout("Project")]
    [SerializeField]
    private StoryVisualBinder _visualBinder;

    /// <summary>
    /// 글자 하나가 출력되는 간격입니다. 기획서가 "한 글자씩"만 정하고 수치를 주지 않아
    /// 체감으로 맞춰야 하므로 인스펙터에서 재컴파일 없이 조정할 수 있게 둡니다.
    /// </summary>
    [Foldout("Settings")]
    [SerializeField]
    [Range(0.01f, 0.2f)]
    private float _secondsPerCharacter = 0.04f;

    private readonly StoryExitFlow _exitFlow = new StoryExitFlow();

    private EStoryPlaybackState _state = EStoryPlaybackState.LOADING;
    private StoryLineCursor _cursor;
    private StoryLinePlayer _linePlayer;
    private StoryData _story;

    private void Start()
    {
        InitUIBindings();
        Begin();
    }

    /// <summary>
    /// 씬이 내려가면 타이핑 루프도 함께 끊습니다. 남겨 두면 이미 파괴된 TMP를 만지게 됩니다.
    ///
    /// 통지 콜백은 UI_Story와 팝업이 소유하므로 배선도 여기서 되돌립니다. 지금은 같은 씬에서 함께
    /// 파괴되어 문제가 드러나지 않지만, 남의 오브젝트에 건 배선을 스스로 걷지 않는 상태를
    /// 남겨 두면 팝업이 화면보다 오래 사는 구조로 바뀌는 순간 조용히 누수가 됩니다.
    /// </summary>
    private void OnDestroy()
    {
        if (_linePlayer != null)
        {
            _linePlayer.Skip();
        }

        if (_ui == null)
        {
            return;
        }

        _ui.OnNextRequested = null;
        _ui.OnLogRequested = null;
        _ui.OnBackRequested = null;

        if (_ui.LogPopup != null)
        {
            _ui.LogPopup.OnClosed = null;
        }

        if (_ui.ExitConfirmPopup != null)
        {
            _ui.ExitConfirmPopup.OnConfirmed = null;
            _ui.ExitConfirmPopup.OnCancelled = null;
        }
    }

    private void InitUIBindings()
    {
        _ui.OnNextRequested = OnNextClicked;
        _ui.OnLogRequested = OpenLog;
        _ui.OnBackRequested = OpenExitConfirm;

        _ui.LogPopup.Init(_visualBinder);

        // 팝업이 닫혔다는 사실은 팝업만 알 수 있습니다. 통지를 받지 않으면 멈춘 상태에서 깨어나지 못합니다.
        _ui.LogPopup.OnClosed = ResumeFromPause;
        _ui.ExitConfirmPopup.OnConfirmed = ExitWithoutCompleting;
        _ui.ExitConfirmPopup.OnCancelled = ResumeFromPause;
    }

    private void Begin()
    {
        string storyId = StoryEntryContext.Instance.SelectedStoryId;

        // 대상이나 대본이 없으면 화면을 유지할 수 없습니다. 기획서 3-L이 오류 안내 후 목록 복귀를 명시합니다.
        if (!StoryScriptResolver.TryResolve(storyId, out _story, out StoryScriptData script))
        {
            _exitFlow.NotifyErrorAndReturn(ErrorMessages.STORY_SCRIPT_MISSING, this.GetCancellationTokenOnDestroy());
            return;
        }

        // NEW 배지는 감상 화면에 들어온 순간 내립니다(기획서 3-F-3).
        GameProgressService.Instance.ClearStoryNewFlag(storyId);

        _cursor = new StoryLineCursor(script.Lines);
        _linePlayer = new StoryLinePlayer(_ui, GetSecondsPerCharacter, this.GetCancellationTokenOnDestroy());
        _linePlayer.OnLineCompleted = OnLineCompleted;

        PlayCurrentLine();
    }

    private float GetSecondsPerCharacter()
    {
        return _secondsPerCharacter;
    }

    private void PlayCurrentLine()
    {
        _state = EStoryPlaybackState.TYPING;
        _linePlayer.Play(_cursor.Current);
    }

    /// <summary>
    /// 타이핑이 끝나기 전에 팝업이 열렸거나 화면을 벗어났다면 상태를 되돌리지 않습니다.
    /// </summary>
    private void OnLineCompleted()
    {
        if (_state == EStoryPlaybackState.TYPING)
        {
            _state = EStoryPlaybackState.WAITING_NEXT;
        }
    }

    /// <summary>
    /// 출력 중인 대사의 남은 글자를 즉시 채웁니다. NEXT의 1단계입니다(기획서 6.3).
    ///
    /// 대사 상자를 덮는 버튼 하나가 넘기기와 즉시 출력을 함께 맡으므로 바깥에서 부르지 않습니다.
    /// TYPING이 아닌 상태에서 들어오면 아무것도 하지 않고 돌아가, 다음 대사로 넘어가는 판단은 NEXT가 갖습니다.
    /// </summary>
    private void OnSkipTypeWriterClicked()
    {
        if (_state != EStoryPlaybackState.TYPING)
        {
            return;
        }

        _state = EStoryPlaybackState.WAITING_NEXT;
        _linePlayer.Skip();
    }

    /// <summary>
    /// 기획서 6.3의 NEXT 3단계입니다.
    /// 출력 중 → 즉시 전체 출력 / 출력 완료 → 다음 대사 / 마지막 대사 → 완료 후 목록 복귀.
    /// PAUSED와 FINISHING에서는 아무 반응도 하지 않습니다(기획서 6.4, 3-L).
    /// </summary>
    private void OnNextClicked()
    {
        if (_state == EStoryPlaybackState.TYPING)
        {
            OnSkipTypeWriterClicked();
            return;
        }

        if (_state != EStoryPlaybackState.WAITING_NEXT)
        {
            return;
        }

        if (_cursor.MoveNext())
        {
            PlayCurrentLine();
            return;
        }

        _state = EStoryPlaybackState.FINISHING;
        _exitFlow.CompleteAndReturn(_story, this.GetCancellationTokenOnDestroy());
    }

    private void OpenLog()
    {
        if (!TryPauseForPopup())
        {
            return;
        }

        _ui.LogPopup.SetLines(_cursor.Lines);
        UIManager.Instance.OpenPopup(_ui.LogPopup);
    }

    private void OpenExitConfirm()
    {
        if (!TryPauseForPopup())
        {
            return;
        }

        UIManager.Instance.OpenPopup(_ui.ExitConfirmPopup);
    }

    /// <summary>
    /// 팝업을 열기 전에 진행을 멈춥니다(기획서 6.4).
    ///
    /// 출력 중이었다면 남은 글자를 즉시 채우고 멈춥니다. 중간 글자에서 얼려 두면 팝업을 닫은 뒤
    /// 남은 글자를 어떤 속도로 이어 갈지가 새 규격이 되므로, 줄 단위로만 멈추게 고정합니다.
    /// 커서는 움직이지 않으므로 닫으면 같은 줄에서 이어집니다.
    /// </summary>
    private bool TryPauseForPopup()
    {
        if (_state != EStoryPlaybackState.TYPING && _state != EStoryPlaybackState.WAITING_NEXT)
        {
            return false;
        }

        _state = EStoryPlaybackState.PAUSED;
        _linePlayer.Skip();
        return true;
    }

    private void ResumeFromPause()
    {
        if (_state == EStoryPlaybackState.PAUSED)
        {
            _state = EStoryPlaybackState.WAITING_NEXT;
        }
    }

    private void ExitWithoutCompleting()
    {
        _state = EStoryPlaybackState.FINISHING;
        _exitFlow.ReturnToStoryList(this.GetCancellationTokenOnDestroy());
    }
}
