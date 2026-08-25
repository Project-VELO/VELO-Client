using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

/// <summary>
/// 스토리 감상 화면의 총괄입니다(기획서 SCREEN-003).
///
/// 대본을 밀고 나가는 일은 StoryProgressFlow가 맡고, 여기서는 화면을 드나드는 일만 봅니다.
/// 대본을 여는 것, 팝업을 띄우기 전에 진행을 멈추는 것, 완료하거나 중도 이탈하는 것입니다.
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

    private StoryProgressFlow _progress;
    private StoryUiBindings _bindings;
    private StoryData _story;

    private void Start()
    {
        _bindings = new StoryUiBindings(_ui);
        _bindings.Bind(_visualBinder, OnNextClicked, OpenLog, OpenExitConfirm, ResumeFromPause, ExitWithoutCompleting);

        Begin();
    }

    /// <summary>
    /// 씬이 내려가면 타이핑 루프도 함께 끊습니다. 남겨 두면 이미 파괴된 TMP를 만지게 됩니다.
    /// </summary>
    private void OnDestroy()
    {
        if (_progress != null)
        {
            _progress.Dispose();
        }

        if (_bindings != null)
        {
            _bindings.Dispose();
        }
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

        _progress = new StoryProgressFlow(_ui, script.Lines, GetSecondsPerCharacter,
            this.GetCancellationTokenOnDestroy());
        _progress.OnFinished = CompleteAndReturn;

        _progress.Begin();
    }

    private float GetSecondsPerCharacter()
    {
        return _secondsPerCharacter;
    }

    private void OnNextClicked()
    {
        _progress.Next();
    }

    private void OpenLog()
    {
        if (!_progress.TryPause())
        {
            return;
        }

        _ui.LogPopup.SetLines(_progress.Cursor.Lines, _progress.Cursor.ReadCount);
        UIManager.Instance.OpenPopup(_ui.LogPopup);
    }

    private void OpenExitConfirm()
    {
        if (!_progress.TryPause())
        {
            return;
        }

        UIManager.Instance.OpenPopup(_ui.ExitConfirmPopup);
    }

    private void ResumeFromPause()
    {
        _progress.Resume();
    }

    private void CompleteAndReturn()
    {
        _exitFlow.CompleteAndReturn(_story, this.GetCancellationTokenOnDestroy());
    }

    private void ExitWithoutCompleting()
    {
        _progress.Finish();
        _exitFlow.ReturnToStoryList(this.GetCancellationTokenOnDestroy());
    }
}
