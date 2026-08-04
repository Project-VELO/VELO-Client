using TMPro;
using UnityEngine;
using VInspector;

/// <summary>
/// 편성 슬롯 1칸입니다(P_UI_Photocard). 배치된 카드의 이름과 등급을 표시합니다.
///
/// 클릭 대상이 아닙니다 — 교체는 보유 카드 클릭으로 일어나고 슬롯은 카드의 멤버가 결정하므로(기획서 3-H-3)
/// 슬롯 쪽에는 입력이 없습니다. 카드 일러스트는 실물 에셋이 아직 없어 이름 텍스트가 카드를 대신합니다(기획서 11.6 임시 이미지).
/// </summary>
public class UI_PhotocardSlotItem : MonoBehaviour
{
    /// <summary>
    /// 등급 표기입니다(기획서 11.6 "1★을 실제로 사용, 표시도"). ★ 글리프가 폰트에 없으면 "1성"으로 교체합니다.
    /// </summary>
    private const string GRADE_FORMAT = "{0}★";

    [Foldout("Hierarchy")]
    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _gradeText;

    public void SetCard(CardData card)
    {
        _nameText.text = card.CardName;
        _gradeText.text = string.Format(GRADE_FORMAT, card.Grade);
    }

    /// <summary>
    /// 기본 편성이 손상되어 카드를 찾지 못한 슬롯입니다. 빈칸 대신 ID를 그대로 보여 원인을 드러냅니다.
    /// </summary>
    public void SetUnknownCard(string cardId)
    {
        _nameText.text = cardId;
        _gradeText.text = string.Empty;
    }
}
