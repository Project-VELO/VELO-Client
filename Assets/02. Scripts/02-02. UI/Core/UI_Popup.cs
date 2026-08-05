using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VInspector;

public abstract class UI_Popup : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private UnityEngine.UI.Button _closeButton;

    public bool IsClosing { get; private set; }

    private UI_SpriteAnimator[] _spriteAnimators;

    protected virtual void Awake()
    {
        _spriteAnimators = GetComponentsInChildren<UI_SpriteAnimator>(true);
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    public virtual void InitPopup()
    {
    }

    public virtual async UniTask OpenAsync()
    {
        gameObject.SetActive(true);
        if (TryGetComponent<UI_ScaleAnimator>(out var animator))
        {
            await animator.PlayOpenAsync(this.GetCancellationTokenOnDestroy());
        }
    }

    public virtual async UniTask CloseAsync()
    {
        IsClosing = true;

        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (_spriteAnimators != null && 0 < _spriteAnimators.Length)
        {
            var tasks = new List<UniTask>();
            foreach (var spriteAnimator in _spriteAnimators)
            {
                if (spriteAnimator != null && spriteAnimator.isActiveAndEnabled)
                {
                    tasks.Add(spriteAnimator.PlayReverseAsync(this.GetCancellationTokenOnDestroy()));
                }
            }
            
            if (0 < tasks.Count)
            {
                await UniTask.WhenAll(tasks);
            }
        }

        if (TryGetComponent<UI_ScaleAnimator>(out var animator))
        {
            await animator.PlayCloseAsync(this.GetCancellationTokenOnDestroy());
        }

        gameObject.SetActive(false);
        IsClosing = false;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnCloseButtonClicked()
    {
        if (UIManager.Instance != null && UIManager.Instance.PopupHandler != null)
        {
            UIManager.Instance.PopupHandler.ClosePopup(this);
        }
    }
}
