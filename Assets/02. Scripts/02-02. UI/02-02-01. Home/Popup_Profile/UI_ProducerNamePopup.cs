using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 프로듀서 이름 변경 팝업(P12, P13)입니다. 제목은 원문 그대로 "프로듀서 이름 변경"입니다(개발 기획서 3장).
///
/// 확인하면 상위 프로필 팝업의 임시값만 바꾸고, 저장은 상위 팝업의 확인(P08)에서만 합니다(검수 1 답).
/// 이름 길이와 빈 값 규칙은 기획자 입력이라 길이는 막지 않고, 비운 채 확인하면 이전 이름을 그대로 둡니다.
/// </summary>
public class UI_ProducerNamePopup : UI_Popup
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_InputField _nameInput;

    [SerializeField]
    private Button _confirmButton;

    private Action<string> _onConfirmed;
    private string _currentName;

    protected override void Awake()
    {
        base.Awake();
        _confirmButton.onClick.AddListener(Confirm);
    }

    public void SetProducerName(string currentName, Action<string> onConfirmed)
    {
        _currentName = currentName;
        _onConfirmed = onConfirmed;
    }

    /// <summary>
    /// 켜진 뒤에 입력칸을 채웁니다. 기반 클래스가 켜면서 글자를 번역하므로, 먼저 넣으면 이름이 번역표 낱말과 같을 때 바뀝니다.
    /// </summary>
    public override async UniTask OpenAsync()
    {
        UniTask openTask = base.OpenAsync();
        _nameInput.text = _currentName;
        await openTask;
    }

    /// <summary>
    /// 확인하지 않고 닫으면 입력을 버립니다(개발 기획서 3장 "확인하지 않은 편집값 폐기").
    /// </summary>
    public override async UniTask CloseAsync()
    {
        _onConfirmed = null;
        await base.CloseAsync();
    }

    /// <summary>
    /// 이름을 바꾸지 않았으면 상위 팝업에 알리지 않습니다. 정하지 않은 이름의 기본 문구가 이름으로 굳어 저장되지 않게 하기 위해서입니다.
    /// </summary>
    private void Confirm()
    {
        string producerName = _nameInput.text;

        if (!string.IsNullOrEmpty(producerName) && producerName != _currentName)
        {
            _onConfirmed?.Invoke(producerName);
        }

        RequestClose();
    }
}
