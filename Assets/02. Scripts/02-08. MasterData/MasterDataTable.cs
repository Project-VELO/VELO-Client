using System;
using System.Collections.Generic;

/// <summary>
/// 마스터 데이터 JSON 파일의 최상위 래퍼입니다.
/// JsonUtility는 List를 최상위로 직렬화하지 못하므로, 모든 테이블 파일은 이 객체를 루트로 갖습니다.
///
/// 제네릭 필드 직렬화가 지원되므로 테이블마다 구체 래퍼 클래스를 만들지 않고 이 하나를 재사용합니다.
/// 사용 예: JsonUtility.FromJson&lt;MasterDataTable&lt;StoryData&gt;&gt;(json)
///
/// 필드가 public인 것은 의도적입니다. 마스터 데이터는 사람이 직접 편집하는 파일이라
/// [SerializeField] private 방식의 밑줄 접두사가 JSON 키에 그대로 노출되면 곤란합니다.
/// (Convention 1-b-(2)가 Serializable·DTO 클래스에 한해 public 필드를 허용합니다)
/// </summary>
[Serializable]
public class MasterDataTable<T>
{
    /// <summary>
    /// 스키마가 바뀌었을 때 로더가 구버전 파일을 식별하기 위한 값입니다.
    /// 필드를 추가할 때는 이 값을 올리고, 기존 필드의 이름은 바꾸지 않습니다.
    /// JsonUtility에는 필드 별칭 기능이 없어 이름을 바꾸면 기존 파일의 값이 조용히 사라집니다.
    /// </summary>
    public int SchemaVersion = MasterDataSchema.CURRENT_VERSION;

    public List<T> Items = new List<T>();
}
