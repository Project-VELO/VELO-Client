using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 사무실의 캐릭터 대사 패널입니다. 리아의 고정 대사 한 줄만 표시합니다(기획서 9.6 결정표).
///
/// 문구를 인스펙터에 두는 것은 기획서 안에서 대사가 두 곳(9번 서두와 9.6 결정표)에 다르게
/// 적혀 있어 확정 시 코드 수정 없이 바꿀 수 있어야 하기 때문입니다. 기본값은 9.6 결정표를 따릅니다 —
/// 서두의 "P님! 어디 있다가…"는 홈 화면 확정 대사와 동일해 사무실에 쓰면 두 화면이 같은 대사가 됩니다.
/// </summary>
public class UI_OfficeCharacterPanel : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _dialogText;

    [Foldout("Settings")]
    [SerializeField]
    private string _speakerName = "리아";

    [SerializeField]
    [TextArea]
    private string _fixedDialog = "안녕! 오늘은 어떤 오싹한 일이 기다리고 있을까?";

    [Header("Word Wrap")]
    [SerializeField]
    private float _maxWidthPerLine = 15f;

    [SerializeField]
    private int _maxLineCount = 3;

    private void Start()
    {
        _nameText.text = _speakerName;
        _dialogText.text = TextWrapUtils.Wrap(_fixedDialog, _maxWidthPerLine, _maxLineCount);
    }
}
