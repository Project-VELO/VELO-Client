using System;
using UnityEngine;

/// <summary>
/// 스토리 대사 한 줄입니다. 감상 화면이 NEXT 한 번에 출력하는 단위와 1:1로 대응합니다.
///
/// 배경·캐릭터·표정은 "바뀌는 줄에만" 값을 적고 나머지는 비워 두는 것을 원칙으로 합니다.
/// 빈 값은 직전 상태를 유지하라는 뜻이며, StoryScriptLoader가 읽는 시점에 채워 넣습니다.
/// 이렇게 하면 기획이 시트에 입력할 양이 크게 줄어듭니다.
/// </summary>
[Serializable]
public class StoryLineData : ISerializationCallbackReceiver
{
    /// <summary>
    /// 회차 안에서의 출력 순서입니다. 1부터 시작합니다.
    /// </summary>
    public int LineId;

    /// <summary>
    /// JSON에 적는 줄 유형 이름입니다("NARRATION" / "DIALOGUE" / "MONOLOGUE").
    /// </summary>
    public string LineTypeName;

    /// <summary>
    /// 화자의 CharacterId입니다. 지문(NARRATION)에서는 비어 있습니다.
    /// </summary>
    public string SpeakerId;

    /// <summary>
    /// 원고에 적힌 화자 표기입니다. 단역을 위해 존재합니다.
    ///
    /// characters.json에는 주요 인물만 있고 "막내 작가"·"메인 PD"·"귀신1" 같은 단역은 없습니다.
    /// 그런 화자에게 억지로 ID를 부여하면 초상과 표정 목록까지 딸려 와야 해서, 원고 표기를 그대로 씁니다.
    /// SpeakerId가 있으면 characters.json의 표시명이 우선하고, 없을 때만 이 값이 화자명으로 나갑니다.
    /// </summary>
    public string SpeakerName;

    /// <summary>
    /// 화면에 출력할 문장입니다. 한 화면에 한 문장만 표시하므로(기획서 6.7) 여기서 이미 끊겨 있어야 합니다.
    /// </summary>
    public string Text;

    public string BackgroundId;

    /// <summary>
    /// 왼쪽에 세울 캐릭터입니다. 대부분의 줄이 한 명만 세우므로 이쪽이 기본 자리입니다.
    /// 이름에 Left가 없는 것은 기존 대본과의 호환 때문입니다(회차 13편이 이미 이 이름으로 기록되어 있습니다).
    /// </summary>
    public string CharacterId;

    public string ExpressionId;

    /// <summary>
    /// 가운데에 세울 캐릭터입니다. 세 인물이 한 화면에 서는 장면에서만 채웁니다.
    /// 왼쪽과 마찬가지로 빈 칸은 직전 유지, NONE은 내리라는 지시입니다.
    /// </summary>
    public string CenterCharacterId;

    public string CenterExpressionId;

    /// <summary>
    /// 오른쪽에 세울 캐릭터입니다. 2인 대화에서만 채웁니다.
    /// 왼쪽과 마찬가지로 빈 칸은 직전 유지, NONE은 내리라는 지시입니다.
    /// </summary>
    public string RightCharacterId;

    public string RightExpressionId;

    /// <summary>
    /// 컷씬 일러스트 ID입니다. 원고의 [일러스트] 지시에 대응하며, 없으면 비어 있습니다.
    /// </summary>
    public string IllustrationId;

    /// <summary>
    /// JSON에 적는 출력 속도 이름입니다("NORMAL" / "SLOW" / "FAST" / "INSTANT" / "WORD_BY_WORD").
    /// 비어 있으면 NORMAL입니다.
    /// </summary>
    public string TextSpeedName;

    /// <summary>
    /// 대사 문구의 글꼴·크기·색 묶음 ID입니다(연출표의 [디자인] 열).
    ///
    /// 세 값을 따로 두지 않고 ID 하나로 묶는 이유는, 같은 조합이 여러 줄에 반복되기 때문입니다.
    /// 시트에 매번 "명조체 24pt 밝은 회색"을 적으면 색 하나 바꿀 때 대본 전체를 고쳐야 합니다.
    /// </summary>
    public string TextStyleId;

    /// <summary>
    /// 화면 이펙트 ID입니다(연출표의 [화면 이펙트] 열). 진동·줌·글리치 같은 연출 하나를 가리킵니다.
    /// </summary>
    public string EffectId;

    /// <summary>
    /// 이 줄에서 새로 시작할 BGM ID입니다. 비어 있으면 직전 BGM을 그대로 둡니다.
    /// 배경과 달리 캐리오버 대상이 아닙니다. 매 줄 같은 값을 채우면 같은 곡을 계속 다시 트는 지시가 됩니다.
    ///
    /// 재생을 멈추려면 BGM_NONE을 적습니다. 빈 값이 "유지"를 뜻하므로 중지에는 별도의 값이 필요합니다.
    /// </summary>
    public string BgmId;

    /// <summary>
    /// 이 줄에서 한 번 재생할 효과음 ID입니다.
    /// </summary>
    public string SfxId;

    /// <summary>
    /// LineTypeName을 변환한 값입니다. 역직렬화 직후 채워집니다.
    /// </summary>
    [NonSerialized]
    public ELineType LineType;

    /// <summary>
    /// TextSpeedName을 변환한 값입니다. 역직렬화 직후 채워집니다.
    /// </summary>
    [NonSerialized]
    public ETextSpeed TextSpeed;

    public void OnBeforeSerialize()
    {
        LineTypeName = MasterDataEnum.ToName(LineType);
        TextSpeedName = MasterDataEnum.ToName(TextSpeed);
    }

    public void OnAfterDeserialize()
    {
        LineType = MasterDataEnum.Parse(LineTypeName, ELineType.NARRATION, $"{nameof(StoryLineData)}(line {LineId}).{nameof(LineTypeName)}");
        TextSpeed = MasterDataEnum.Parse(TextSpeedName, ETextSpeed.NORMAL, $"{nameof(StoryLineData)}(line {LineId}).{nameof(TextSpeedName)}");
    }
}
