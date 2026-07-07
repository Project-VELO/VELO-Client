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

    private void Start()
    {
        bool hasSubScene = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != "PersistentScene")
            {
                _currentLoadedSubScene = scene.name;
                hasSubScene = true;
                break;
            }
        }

        if (!hasSubScene)
        {
            LoadSceneAsync(ESceneNames.HomeScene).Forget();
        }
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
            string actualSceneName = GetActualSceneName(eSceneName);
            string oldSceneName = _currentLoadedSubScene;

            // 1. 신규 씬을 먼저 Additive로 로드
            await LoadAndActivateSceneAsync(actualSceneName, cancellationToken);

            // 2. 신규 씬 로드가 완전히 끝난 뒤에 이전 서브 씬을 언로드 (단일 씬 언로드 에러 방지)
            if (!string.IsNullOrEmpty(oldSceneName) && oldSceneName != actualSceneName)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(oldSceneName);
                if (unloadOp != null)
                {
                    await unloadOp.WithCancellation(cancellationToken);
                }
            }
        }
        finally
        {
            CleanupTransition();
        }
    }

    private string GetActualSceneName(ESceneNames eSceneName)
    {
        string enumName = eSceneName.ToString();
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            string cleanedName = System.Text.RegularExpressions.Regex.Replace(sceneName, @"^\d+_", "");
            if (cleanedName == enumName)
            {
                return sceneName;
            }
        }
        return enumName;
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