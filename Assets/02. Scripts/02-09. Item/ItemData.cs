using System;
using UnityEngine;

/// <summary>
/// 의상·악세사리 아이템의 기획 메타데이터입니다(items.json).
///
/// 아직 선택 상태 표시에만 쓰이며, 능력 효과와 외형 변경은 구현하지 않습니다(기획서 3-H-6).
/// 능력 수치 필드는 기획이 확정된 뒤 추가하되, 기존 필드 뒤에만 붙입니다.
///
/// 필드가 public인 것은 의도적입니다. 사람이 직접 편집하는 JSON이라
/// [SerializeField] private 방식의 밑줄 접두사가 키에 노출되면 곤란합니다(Convention 1-b-(2)).
/// </summary>
[Serializable]
public class ItemData : ISerializationCallbackReceiver
{
    public string ItemId;

    /// <summary>
    /// JSON에 적는 종류 이름입니다("COSTUME" 또는 "ACCESSORY"). 읽는 쪽은 ItemType을 사용하십시오.
    /// </summary>
    public string ItemTypeName;

    public string DisplayName;
    public string Description;
    public string IconPath;

    /// <summary>
    /// ItemTypeName을 변환한 값입니다. 역직렬화 직후 채워집니다.
    /// </summary>
    [NonSerialized]
    public EItemType ItemType;

    public void OnBeforeSerialize()
    {
        ItemTypeName = MasterDataEnum.ToName(ItemType);
    }

    public void OnAfterDeserialize()
    {
        ItemType = MasterDataEnum.Parse(ItemTypeName, EItemType.COSTUME, $"{nameof(ItemData)}({ItemId}).{nameof(ItemTypeName)}");
    }
}
