using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using VInspector;

public class UI_Home : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [Header("Top Layout Buttons")]
    [SerializeField]
    private Button _profileButton;

    [SerializeField]
    private Button _notificationButton;

    [SerializeField]
    private Button _mailboxButton;

    [SerializeField]
    private Button _settingButton;

    [Foldout("Hierarchy")]
    [Header("Navigation Buttons")]
    [SerializeField]
    private Button _spaceButton;

    [SerializeField]
    private Button _shopButton;

    [SerializeField]
    private Button _collectionButton;

    [SerializeField]
    private Button _enterStoryButton;

    [Foldout("Hierarchy")]
    [Header("Popups")]
    [SerializeField]
    private UI_ProfileSettingPopup _profileSettingPopup;

    private void Start()
    {
        InitButtons();
    }

    private void InitButtons()
    {
        if (_profileButton != null)
        {
            _profileButton.onClick.AddListener(OpenProfileSettingPopup);
        }

        if (_notificationButton != null)
        {
            _notificationButton.onClick.AddListener(OpenNotificationPopup);
        }
        if (_mailboxButton != null)
        {
            _mailboxButton.onClick.AddListener(OpenMailboxPopup);
        }
        if (_settingButton != null)
        {
            _settingButton.onClick.AddListener(OpenSettingPopup);
        }

        if (_spaceButton != null)
        {
            _spaceButton.onClick.AddListener(() => LoadScene(ESceneNames.SpaceScene));
        }
        if (_shopButton != null)
        {
            _shopButton.onClick.AddListener(() => LoadScene(ESceneNames.ShopScene));
        }
        if (_collectionButton != null)
        {
            _collectionButton.onClick.AddListener(() => LoadScene(ESceneNames.CollectionScene));
        }

        if (_enterStoryButton != null)
        {
            _enterStoryButton.onClick.AddListener(() => LoadScene(ESceneNames.SelectStoryScene));
        }
    }

    private void OpenProfileSettingPopup()
    {
        if (_profileSettingPopup != null)
        {
            UIManager.Instance.PopupHandler.OpenPopup(_profileSettingPopup);
        }
    }

    private void OpenNotificationPopup()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenNotificationPopup();
        }
    }

    private void OpenMailboxPopup()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenMailboxPopup();
        }
    }

    private void OpenSettingPopup()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenSettingPopup();
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
