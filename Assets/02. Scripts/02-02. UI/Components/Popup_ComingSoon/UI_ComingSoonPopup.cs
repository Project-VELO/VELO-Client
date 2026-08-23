using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 아직 만들어지지 않은 기능을 눌렀을 때 뜨는 안내 팝업입니다.
///
/// 닫기만 하면 되므로 동작이 없습니다. 닫기 버튼은 UI_Popup이 이미 처리합니다.
///
/// Persistent Canvas에 하나만 두고 모든 화면이 나눠 씁니다. 화면마다 복제하면 문구가 갈라지고,
/// 기능이 완성될 때마다 지워야 할 팝업이 화면 수만큼 늘어납니다.
///
/// 여는 경로를 UIManager에 메서드로 얹지 않고 여기 정적 진입점으로 둔 이유는, UIManager가 이미
/// 196줄로 클래스 길이 한계(200줄)에 닿아 있기 때문입니다. 팝업 종류가 늘 때마다 그 파일이
/// 커지는 구조를 여기서 더 밀지 않았습니다.
/// </summary>
public class UI_ComingSoonPopup : UI_Popup
{
    private const string MESSAGE = "준비 중인 기능입니다.";

    /// <summary>
    /// Persistent Canvas는 게임이 끝날 때까지 살아 있어 참조가 씬 전환에도 끊기지 않습니다.
    /// 다만 도메인 리로드를 끄면 정적 값이 남으므로 파괴 시점에 반드시 비웁니다.
    /// </summary>
    private static UI_ComingSoonPopup _instance;

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _messageText;

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public override void InitPopup()
    {
        base.InitPopup();

        if (_messageText != null)
        {
            _messageText.text = MESSAGE;
        }
    }

    /// <summary>
    /// 팝업이 없는 씬에서 눌렸다면 조용히 넘어가지 않고 알립니다. 배선이 빠진 것을 모르고
    /// 지나가면 "눌러도 아무 일이 없다"는 원래 문제로 되돌아갑니다.
    /// </summary>
    public static void Open()
    {
        if (_instance == null)
        {
            Debug.LogWarning($"[{nameof(UI_ComingSoonPopup)}] 팝업이 없습니다. Persistent Canvas에 올라와 있는지 확인해 주세요.");
            return;
        }

        UIManager.Instance.OpenPopup(_instance);
    }
}
