using System;
using UnityEngine;

/// <summary>
/// 홈 프로필 팝업(P01~P13)에서 고른 세 값입니다: 프로필 초상(H01), 홈 캐릭터(H06), 프로듀서 이름.
///
/// 본편 진행(GameProgressData)과 따로 둡니다. 개발 기획서 3장은 이 선택이 본편 인물, 진행, 호감도,
/// 곡, 조각 기록에 영향을 주지 않는다고 정하고, 15장 저장 시점 표도 "본편 기록과 분리"로 적습니다.
///
/// 빈 값은 "아직 고르지 않음"입니다. 옛 세이브에는 이 객체가 없어 JsonUtility가 빈 값으로 채우므로,
/// 기본값 해석은 읽는 쪽(HomeProfileService)이 한 곳에서 맡습니다.
/// </summary>
[Serializable]
public class HomeProfileData
{
    [SerializeField]
    private string _portraitId = string.Empty;

    [SerializeField]
    private string _homeCharacterId = string.Empty;

    [SerializeField]
    private string _producerName = string.Empty;

    public string PortraitId { get => _portraitId; set => _portraitId = value; }
    public string HomeCharacterId { get => _homeCharacterId; set => _homeCharacterId = value; }
    public string ProducerName { get => _producerName; set => _producerName = value; }
}
