using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class SceneTransitionManager : MonoBehaviourSingleton<SceneTransitionManager>
{
    private string _currentLoadedSubScene;
    private bool _isTransitioning;

    public string CurrentLoadedSubScene => _currentLoadedSubScene;
    public bool IsTransitioning => _isTransitioning;

    protected override void Awake()
    {
        base.Awake();
    }

    public async UniTask LoadSceneAsync(ESceneNames eSceneName, CancellationToken cancellationToken = default)
    {
        if (_isTransitioning)
        {
            return;
        }

        PrepareTransition();

        try
        {
            await UnloadCurrentSubSceneAsync(cancellationToken);
            await LoadAndActivateSceneAsync(eSceneName.ToString(), cancellationToken);
        }
        finally
        {
            CleanupTransition();
        }
    }

    private void PrepareTransition()
    {
        _isTransitioning = true;
        InputHandler.BlockInput();
        Time.timeScale = 1f;

        if (UIManager.Instance != null && UIManager.Instance.PopupHandler != null)
        {
            UIManager.Instance.PopupHandler.ClearAllPopups();
        }
    }

    private async UniTask UnloadCurrentSubSceneAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentLoadedSubScene))
        {
            return;
        }

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(_currentLoadedSubScene);
        if (unloadOp != null)
        {
            await unloadOp.WithCancellation(cancellationToken);
        }
    }

    private async UniTask LoadAndActivateSceneAsync(string sceneName, CancellationToken cancellationToken)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null)
        {
            Debug.LogError($"'{sceneName}' 로드 실패. 빌드 프로필 확인해보세요");
            return;
        }

        await loadOp.WithCancellation(cancellationToken);

        Scene newlyLoadedScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newlyLoadedScene);
        _currentLoadedSubScene = sceneName;
        await UniTask.Yield(cancellationToken);
    }

    private void CleanupTransition()
    {
        _isTransitioning = false;
        InputHandler.UnblockInput();
    }
}