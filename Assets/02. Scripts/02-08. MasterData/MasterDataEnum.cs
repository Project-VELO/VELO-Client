using System;
using UnityEngine;

/// <summary>
/// 마스터 데이터 JSON의 열거형 값을 문자열로 주고받기 위한 변환 헬퍼입니다.
///
/// JsonUtility는 열거형을 선언 순서에 따른 정수로만 직렬화하는데, 마스터 데이터는 사람이 직접
/// 편집하므로 "MinimumRank": 3 처럼 숫자를 적으면 의미를 알 수 없고 오타가 조용히 통과합니다.
/// 그래서 DTO는 열거형을 문자열 필드로 받아 두고 역직렬화 직후 이 헬퍼로 변환합니다.
/// 세이브 데이터는 사람이 편집하지 않으므로 정수 직렬화를 유지합니다.
/// </summary>
public static class MasterDataEnum
{
    /// <summary>
    /// 문자열을 열거형으로 바꿉니다. 값이 비어 있으면 기본값을 쓰고, 알 수 없는 값이면 경고 후 기본값을 씁니다.
    /// 오타 하나로 데이터 전체를 버리지 않되, 조용히 넘어가지도 않도록 합니다.
    /// </summary>
    public static T Parse<T>(string value, T fallback, string context) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        if (Enum.TryParse(value.Trim(), true, out T parsed) && Enum.IsDefined(typeof(T), parsed))
        {
            return parsed;
        }

        Debug.LogWarning($"[MasterDataEnum] {context}: '{value}'는 {typeof(T).Name}에 없는 값이라 기본값 {fallback}을 사용합니다.");
        return fallback;
    }

    /// <summary>
    /// 열거형을 마스터 데이터에 기록할 문자열로 바꿉니다. 대본 가져오기 도구가 JSON을 쓸 때 사용합니다.
    /// </summary>
    public static string ToName<T>(T value) where T : struct, Enum
    {
        return value.ToString();
    }
}
