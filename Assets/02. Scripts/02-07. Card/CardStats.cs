using System;
using UnityEngine;

/// <summary>
/// 포토카드가 가질 수 있는 상세 능력치 정보를 정의하는 DTO 클래스입니다.
/// </summary>
[Serializable]
public class CardStats
{
    [SerializeField]
    private int _scoreBonus = 0;

    [SerializeField]
    private int _hp = 100;

    [SerializeField]
    private float _healingAmount = 0f;

    public int ScoreBonus { get => _scoreBonus; set => _scoreBonus = value; }
    public int Hp { get => _hp; set => _hp = value; }
    public float HealingAmount { get => _healingAmount; set => _healingAmount = value; }
}
