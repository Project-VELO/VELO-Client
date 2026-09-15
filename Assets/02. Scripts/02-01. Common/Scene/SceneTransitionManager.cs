using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class SceneTransitionManager : MonoBehaviourSingleton<SceneTransitionManager>
{
    /// <summary>
    /// 전환의 시작과 끝을 알리기만 합니다. 팝업 정리·로딩 표시 같은 UI 뒤처리는 UIManager가 구독해서 수행합니다.
    /// 씬 전환 인프라가 UI 계층을 직접 호출하면 하위 계층이 상위 구현에 매이므로 통지로 끊습니다.
    /// 외부에서 대입으로 기존 구독자를 지우지 못하게 event로 선언합니다(선례: SaveService.OnSaveFailed).
    /// </summary>
    public event Action OnTransitionStarted;
    public event Action OnTransitionFinished;

    private const string PersistentSceneName = "PersistentScene";

    private string _currentLoadedSubScene;
    private bool _isTransitioning;

    public string CurrentLoadedSubScene => _currentLoadedSubScene;
    public bool IsTransitioning => _isTransitioning;

    public static string CleanSceneName(string sceneName)
    {
        string cleaned = System.Text.RegularExpressions.Regex.Replace(sceneName, @"^\d+_", "");
        return IdentifierUtils.SanitizeIdentifier(cleaned);
    }

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
            if (scene.name != PersistentSceneName)
            {
                _currentLoadedSubScene = scene.name;
                hasSubScene = true;
                break;
            }
        }

        if (!hasSubScene)
        {
            LoadSceneAsync(ESceneNames.HomeScene, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    public async UniTask LoadSceneAsync(ESceneNames eSceneName, CancellationToken cancellationToken = default)
    {
        if (_isTransitioning)
        {
            return;
        }

        string actualSceneName = GetActualSceneName(eSceneName);
        if (_currentLoadedSubScene == actualSceneName)
        {
            return;
        }

        try
        {
            // 전환 시작 통지(OnTransitionStarted)의 구독자가 예외를 던져도 finally의 CleanupTransition이
            // 실행되어야 하므로 try 안에서 시작합니다. 밖에 두면 입력 차단과 전환 중 표시가 풀리지 않은 채 남습니다.
            PrepareTransition();

            string oldSceneName = _currentLoadedSubScene;

            bool isLoaded = await LoadAndActivateSceneAsync(actualSceneName, cancellationToken);
            if (!isLoaded)
            {
                return;
            }

            // 언로드가 먼저 끝나면 씬이 하나도 남지 않는 순간이 생겨 단일 씬 언로드 에러가 납니다.
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

    /// <summary>
    /// 지금 떠 있는 화면이라도 처음부터 다시 올립니다.
    ///
    /// LoadSceneAsync는 같은 화면을 요청하면 아무 일도 하지 않습니다. 세이브를 새로 만든 뒤에는
    /// 화면에 그려 둔 재화와 진행 표시가 옛 값이라, 같은 화면이어도 다시 올려야 합니다.
    ///
    /// 같은 화면은 내린 뒤에 올립니다. 같은 이름의 씬이 둘 겹치면 이름으로 내릴 때 어느 쪽이 내려갈지
    /// 정해지지 않기 때문입니다. 이 매니저가 있다는 것은 PersistentScene이 떠 있다는 뜻이라,
    /// 먼저 내려도 씬이 하나도 남지 않는 순간은 생기지 않습니다.
    /// </summary>
    public async UniTask ReloadSceneAsync(ESceneNames eSceneName, CancellationToken cancellationToken = default)
    {
        if (_isTransitioning)
        {
            return;
        }

        string actualSceneName = GetActualSceneName(eSceneName);
        if (_currentLoadedSubScene != actualSceneName)
        {
            await LoadSceneAsync(eSceneName, cancellationToken);
            return;
        }

        try
        {
            PrepareTransition();

            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(actualSceneName);
            if (unloadOp != null)
            {
                await unloadOp.WithCancellation(cancellationToken);
            }

            // 다시 올리기에 실패했을 때 내린 화면의 이름이 남으면, 이후의 같은 화면 요청이 전부 무시됩니다.
            _currentLoadedSubScene = null;

            await LoadAndActivateSceneAsync(actualSceneName, cancellationToken);
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
            string cleanedName = CleanSceneName(sceneName);
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

        // 떠나는 화면이 걸어 둔 입력 모드(팝업의 UI 모드 등)가 다음 씬으로 새지 않도록 전환 시점마다 초기화합니다.
        InputHandler.Instance.Reset();
        InputHandler.Instance.BlockInput();
        Time.timeScale = 1f;

        OnTransitionStarted?.Invoke();
    }

    private async UniTask<bool> LoadAndActivateSceneAsync(string sceneName, CancellationToken cancellationToken)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null)
        {
            Debug.LogError($"'{sceneName}' 로드 실패. 빌드 프로필 확인해보세요");
            return false;
        }

        await loadOp.WithCancellation(cancellationToken);

        Scene newlyLoadedScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newlyLoadedScene);
        _currentLoadedSubScene = sceneName;
        await UniTask.Yield(cancellationToken);
        return true;
    }

    private void CleanupTransition()
    {
        _isTransitioning = false;
        InputHandler.Instance.UnblockInput();

        OnTransitionFinished?.Invoke();
    }
}