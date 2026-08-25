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
    /// 화면 위쪽에 띄울 캐릭터입니다. 바닥에 서지 않고 공중에 걸리는 인물만 여기에 옵니다.
    /// 나머지 세 자리와 마찬가지로 빈 칸은 직전 유지, NONE은 내리라는 지시입니다.
    /// </summary>
    public string UpperCharacterId;

    public string UpperExpressionId;

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
    /// JSON에 적는 텍스트 위치 이름입니다("DIALOG_BOX" / "SCREEN_CENTER").
    /// 비어 있으면 DIALOG_BOX입니다.
    /// </summary>
    public string TextPlacementName;

    /// <summary>
    /// 대사 문구의 글꼴·크기·색 묶음 ID입니다(연출표의 [디자인] 열).
    ///
    /// 세 값을 따로 두지 않고 ID 하나로 묶는 이유는, 같은 조합이 여러 줄에 반복되기 때문입니다.
    /// 시트에 매번 "명조체 24pt 밝은 회색"을 적으면 색 하나 바꿀 때 대본 전체를 고쳐야 합니다.
    /// </summary>
    public string TextStyleId;

    /// <summary>
    /// 화면 이펙트 ID입니다(연출표의 [화면 이펙트] 열). 진동·줌·글리치 같은 연출 하나를 가리킵니다.
    /// 컷씬에서는 카메라가 하는 일(줌·팬·흔들림)을 맡고, 등장과 전환은 아래 둘이 따로 맡습니다.
    /// </summary>
    public string EffectId;

    /// <summary>
    /// 컷이 나타날 때 거는 연출입니다(페이드인·디졸브인·하드컷).
    ///
    /// EffectId와 나눈 이유는 한 컷이 등장·카메라·전환 셋을 동시에 갖기 때문입니다.
    /// 하나로 합치면 "페이드인하며 천천히 줌인"을 적을 자리가 없습니다.
    /// </summary>
    public string EntryEffectId;

    /// <summary>
    /// 컷이 끝나 다음으로 넘어갈 때 거는 연출입니다(페이드아웃·암전·글리치).
    /// </summary>
    public string ExitEffectId;

    /// <summary>
    /// 컷이 화면에 머무는 시간(초)입니다. 0이면 NEXT를 기다리는 보통 줄입니다.
    ///
    /// 컷씬은 읽는 속도가 아니라 연출이 진행을 정합니다. 값이 있는 줄만 스스로 넘어가므로,
    /// 이미 있는 회차 13편은 이 값이 비어 있어 지금까지와 똑같이 동작합니다.
    /// </summary>
    public float CutSeconds;

    /// <summary>
    /// 컷이 시작되고 대사가 뜨기까지 기다리는 시간(초)입니다.
    /// 그림이 먼저 자리를 잡고 글이 얹히는 연출이라, 0이면 지금처럼 곧바로 출력합니다.
    /// </summary>
    public float TextDelaySeconds;

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

    /// <summary>
    /// TextPlacementName을 변환한 값입니다. 역직렬화 직후 채워집니다.
    /// </summary>
    [NonSerialized]
    public EStoryTextPlacement TextPlacement;

    public void OnBeforeSerialize()
    {
        LineTypeName = MasterDataEnum.ToName(LineType);
        TextSpeedName = MasterDataEnum.ToName(TextSpeed);
        TextPlacementName = MasterDataEnum.ToName(TextPlacement);
    }

    public void OnAfterDeserialize()
    {
        LineType = MasterDataEnum.Parse(LineTypeName, ELineType.NARRATION, $"{nameof(StoryLineData)}(line {LineId}).{nameof(LineTypeName)}");
        TextSpeed = MasterDataEnum.Parse(TextSpeedName, ETextSpeed.NORMAL, $"{nameof(StoryLineData)}(line {LineId}).{nameof(TextSpeedName)}");
        TextPlacement = MasterDataEnum.Parse(TextPlacementName, EStoryTextPlacement.DIALOG_BOX, $"{nameof(StoryLineData)}(line {LineId}).{nameof(TextPlacementName)}");
    }
}
