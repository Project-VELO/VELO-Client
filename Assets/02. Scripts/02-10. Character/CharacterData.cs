using System;
using System.Collections.Generic;

/// <summary>
/// 캐릭터의 기획 메타데이터입니다(characters.json).
///
/// 편성 대상인 멤버 5명뿐 아니라 스토리에 등장하는 모든 화자(프로듀서 P, 무명, 임원 등)를 포함합니다.
/// 원고의 화자가 멤버를 크게 넘어서므로, 대사의 화자 표시명과 초상화를 이 테이블 하나로 해결합니다.
///
/// 편성 슬롯(기획서 3-H-2의 MEMBER_SLOT_01~05)은 별도 열거형 대신 MemberOrder로 표현합니다.
/// 슬롯 열거형을 따로 두면 캐릭터와 슬롯의 대응이 두 곳에 생겨 어긋날 여지가 됩니다.
/// </summary>
[Serializable]
public class CharacterData
{
    public string CharacterId;
    public string DisplayName;

    /// <summary>
    /// LIVE 편성에 참여하는 멤버인지 여부입니다. false면 스토리 화자로만 등장합니다.
    /// </summary>
    public bool IsMember;

    /// <summary>
    /// 편성 슬롯의 표시 순서입니다(0부터). 멤버가 아니면 MasterDataSchema.NOT_MEMBER_ORDER입니다.
    /// </summary>
    public int MemberOrder = MasterDataSchema.NOT_MEMBER_ORDER;

    public string PortraitPath;

    /// <summary>
    /// 이 캐릭터가 가질 수 있는 표정 ID 목록입니다.
    /// 대사 데이터의 표정 값이 이 목록에 있는지 검증하는 데 사용합니다.
    /// </summary>
    public List<string> ExpressionIds = new List<string>();
}
