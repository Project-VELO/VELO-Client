using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 공용 오류 안내 팝업입니다(SCREEN-011 "오류 안내").
///
/// 기획서 3-L이 오류 안내를 요구하는 세 상황(곡 데이터 없음, 스토리 대사 없음, 저장 실패)이
/// 모두 이 팝업 하나를 씁니다. 화면마다 인스턴스를 두지 않는 것은 저장 실패가 어느 화면에서든
/// 발생할 수 있고, 복제하면 문구와 동작이 화면별로 갈라지기 때문입니다.
///
/// 확인 버튼은 기반 클래스의 닫기 버튼 자리에 그대로 배선합니다.
/// 그래야 확인·ESC·뒤로가기 어느 경로로 닫아도 뒤처리가 한 곳에서만 일어납니다.
/// </summary>
public class UI_ErrorPopup : UI_Popup
{
    /// <summary>
    /// 안내가 완전히 닫혔음을 알립니다. 팝업이 하나뿐이라, 밀려 있는 다음 안내는 이 시점에야 열 수 있습니다.
    /// </summary>
    public Action OnClosed;

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _messageText;

    /// <summary>
    /// 안내를 닫은 뒤에 이어서 할 일입니다. 기획서 3-L은 스토리 대사 없음을
    /// "오류 팝업 <b>후</b> 스토리 목록 복귀"로 정하고 있어, 복귀 시점이 닫기에 매여 있습니다.
    /// </summary>
    private Action _onConfirmed;

    public void SetError(string message, Action onConfirmed)
    {
        _messageText.text = message;
        _onConfirmed = onConfirmed;
    }

    /// <summary>
    /// 콜백은 실행하기 전에 비웁니다. 이 팝업은 씬을 건너 재사용되므로 남겨 두면
    /// 다음 오류를 닫을 때 이미 끝난 복귀가 한 번 더 실행됩니다.
    /// </summary>
    public override async UniTask CloseAsync()
    {
        Action onConfirmed = _onConfirmed;
        _onConfirmed = null;

        await base.CloseAsync();

        onConfirmed?.Invoke();
        OnClosed?.Invoke();
    }
}
