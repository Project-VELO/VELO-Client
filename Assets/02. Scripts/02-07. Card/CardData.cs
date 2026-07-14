using System;
using UnityEngine;

/// <summary>
/// 포토카드 메타데이터 정보를 저장하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class CardData
{
    [SerializeField]
    private string _cardId;

    [SerializeField]
    private string _characterId;

    [SerializeField]
    private string _cardName;

    [SerializeField]
    private string _imagePath;

    [SerializeField]
    private int _grade = 1;

    [SerializeField]
    private CardStats _stats;

    [SerializeField]
    private string _skillId;

    public string CardId { get => _cardId; set => _cardId = value; }
    public string CharacterId { get => _characterId; set => _characterId = value; }
    public string CardName { get => _cardName; set => _cardName = value; }
    public string ImagePath { get => _imagePath; set => _imagePath = value; }
    public int Grade { get => _grade; set => _grade = value; }
    public CardStats Stats { get => _stats; set => _stats = value; }
    public string SkillId { get => _skillId; set => _skillId = value; }
}
