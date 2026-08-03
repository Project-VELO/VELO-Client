using System;

/// <summary>
/// 포토카드의 상세 능력치입니다.
/// 1차 MVP에서는 점수·콤보·판정 어디에도 반영하지 않으며(기획서 3-H-1), 이후 확장을 위해 스키마만 유지합니다.
/// </summary>
[Serializable]
public class CardStats
{
    public int ScoreBonus = 0;
    public int Hp = 100;
    public float HealingAmount = 0f;
}
