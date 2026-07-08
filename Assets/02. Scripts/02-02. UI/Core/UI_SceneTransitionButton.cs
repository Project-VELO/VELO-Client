using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using VInspector;

public class UI_SceneTransitionButton : MonoBehaviour
{
    [Foldout("Project")]
    [SerializeField]
    private ESceneNames _targetScene = ESceneNames.HomeScene;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void Start()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(LoadScene);
        }
    }

    private void LoadScene()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneAsync(_targetScene, this.GetCancellationTokenOnDestroy()).Forget();
        }
        else
        {
            Debug.LogWarning($"[UI_SceneTransitionButton] SceneTransitionManager.Instance is null. Cannot transition to {_targetScene}.");
        }
    }
}
