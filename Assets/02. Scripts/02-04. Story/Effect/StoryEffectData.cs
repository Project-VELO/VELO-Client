using System;
using UnityEngine;

/// <summary>
/// 화면 연출 하나의 설정입니다(effects.json).
///
/// 대사 데이터는 연출표에 적힌 ID("FX_SCREEN_SHAKE_HARD")만 들고 있고,
/// 그 ID가 화면에서 무엇을 얼마나 하는지는 이 표가 정합니다.
/// 세기를 대사 쪽에 적지 않는 이유는, 같은 이펙트가 여러 회차에 반복 등장하기 때문입니다.
/// 진폭 하나를 바꾸려고 대본 356줄을 고칠 수는 없습니다.
///
/// 필드가 public인 것은 의도적입니다. 사람이 직접 편집하는 JSON이라
/// [SerializeField] private 방식의 밑줄 접두사가 키에 노출되면 곤란합니다(Convention 1-b-(2)).
/// </summary>
[Serializable]
public class StoryEffectData : ISerializationCallbackReceiver
{
    public string EffectId;

    /// <summary>
    /// JSON에 적는 연출 종류 이름입니다("SHAKE" / "ZOOM" / "PAN" / "TINT" / "FLASH" / "STOP" / "NONE").
    /// </summary>
    public string KindName;

    /// <summary>
    /// JSON에 적는 덮개 이름입니다("OVERLAY" / "VIGNETTE"). TINT·FLASH만 씁니다.
    /// </summary>
    public string TargetName;

    /// <summary>
    /// 종류마다 뜻이 다른 주 세기입니다. 종류별로 단위가 갈리는 필드를 따로 두면
    /// 대부분의 행이 빈 칸으로 남아 표가 읽기 어려워지므로 하나로 묶었습니다.
    ///
    /// SHAKE: 진폭(px) / ZOOM: 목표 배율(1보다 크면 확대) / PAN: 가로 이동(px) / TINT·FLASH: 목표 알파(0~1)
    /// </summary>
    public float Strength;

    /// <summary>
    /// PAN의 세로 이동(px)입니다. 양수면 위로 올려다보고 음수면 아래를 내려다봅니다.
    /// 다른 종류에서는 쓰이지 않습니다.
    /// </summary>
    public float StrengthY;

    /// <summary>
    /// 목표 상태까지 가는 데 걸리는 시간입니다. 0이면 즉시 적용합니다.
    /// </summary>
    public float DurationSeconds;

    /// <summary>
    /// SHAKE가 다음 연출 지시까지 계속 떨리는지입니다.
    /// 연출표의 "흔들림 지속"과 "짧게 한 번 덜컹"을 가르는 값입니다.
    /// </summary>
    public bool IsLooping;

    /// <summary>
    /// TINT·FLASH의 덮개 색입니다("#RRGGBB"). 비어 있으면 검정입니다.
    /// </summary>
    public string ColorHex;

    /// <summary>
    /// 연출표에 적혀 있던 원문입니다. 데이터만 보고도 의도를 확인할 수 있게 남깁니다.
    /// 런타임은 읽지 않습니다.
    /// </summary>
    public string Note;

    [NonSerialized]
    public EStoryEffectKind Kind;

    [NonSerialized]
    public EStoryEffectTarget Target;

    /// <summary>
    /// ColorHex를 변환한 값입니다. 역직렬화 직후 채워집니다.
    /// </summary>
    [NonSerialized]
    public Color Color;

    public void OnBeforeSerialize()
    {
        KindName = MasterDataEnum.ToName(Kind);
        TargetName = MasterDataEnum.ToName(Target);
    }

    public void OnAfterDeserialize()
    {
        Kind = MasterDataEnum.Parse(KindName, EStoryEffectKind.NONE, $"{nameof(StoryEffectData)}({EffectId}).{nameof(KindName)}");
        Target = MasterDataEnum.Parse(TargetName, EStoryEffectTarget.OVERLAY, $"{nameof(StoryEffectData)}({EffectId}).{nameof(TargetName)}");
        Color = ParseColor(ColorHex);
    }

    /// <summary>
    /// "#RRGGBB"를 색으로 바꿉니다. 알파는 Strength가 따로 정하므로 여기서는 무시합니다.
    /// 읽지 못하면 검정으로 떨어뜨립니다. 암전이 기본 동작이라 눈에 덜 튑니다.
    /// </summary>
    private Color ParseColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return UnityEngine.Color.black;
        }

        if (ColorUtility.TryParseHtmlString(hex, out Color parsed))
        {
            return parsed;
        }

        Debug.LogWarning($"[StoryEffectData] {EffectId}의 색 '{hex}'을 읽지 못해 검정으로 대체합니다.");
        return UnityEngine.Color.black;
    }
}
