using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 스튜디오 화면입니다. 지금 맡는 일은 뒤로가기뿐입니다.
///
/// 편성 기능은 곡 선택 화면의 포토카드 선택 팝업이 갖고 있습니다. 이 화면은 같은 배치를
/// 보여 주기만 하므로, 여기에 편성 로직을 두면 두 벌이 되어 어긋납니다.
/// </summary>
public class UI_Studio : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private Button _backButton;

    private void Start()
    {
        _backButton.onClick.AddListener(MoveToSpace);
    }

    private void MoveToSpace()
    {
        if (SceneTransitionManager.Instance == null)
        {
            return;
        }

        SceneTransitionManager.Instance.LoadSceneAsync(ESceneNames.SpaceScene, this.GetCancellationTokenOnDestroy()).Forget();
    }
}
