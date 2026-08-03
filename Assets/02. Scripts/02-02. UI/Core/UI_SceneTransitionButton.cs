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

    /// <summary>
    /// 뒤로가기 입력을 잠그거나 풉니다.
    ///
    /// 하루 마무리처럼 전환 연출이 재생되는 동안에는 추가 입력을 막아야 합니다(기획서.txt:734-735, 758).
    /// 이 프리팹은 7개 화면이 공유하므로, 화면마다 Button을 따로 찾아 끄지 않고 여기서 받습니다.
    ///
    /// Awake 이전에 호출될 수 있어 _button을 그 자리에서 채웁니다.
    /// </summary>
    public void SetInteractable(bool isInteractable)
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_button == null)
        {
            return;
        }

        _button.interactable = isInteractable;
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
