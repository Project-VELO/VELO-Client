using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using VInspector;

public class UI_Space : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _backButton;

    private void Start()
    {
        InitButtons();
    }

    private void InitButtons()
    {
        if (_backButton != null)
        {
            _backButton.onClick.AddListener(() => LoadScene(ESceneNames.HomeScene));
        }
    }

    private void LoadScene(ESceneNames sceneName)
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneAsync(sceneName, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
