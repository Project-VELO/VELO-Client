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
    public string CharacterId;
    public string ExpressionId;

    /// <summary>
    /// 컷씬 일러스트 ID입니다. 원고의 [일러스트] 지시에 대응하며, 없으면 비어 있습니다.
    /// </summary>
    public string IllustrationId;

    /// <summary>
    /// LineTypeName을 변환한 값입니다. 역직렬화 직후 채워집니다.
    /// </summary>
    [NonSerialized]
    public ELineType LineType;

    public void OnBeforeSerialize()
    {
        LineTypeName = MasterDataEnum.ToName(LineType);
    }

    public void OnAfterDeserialize()
    {
        LineType = MasterDataEnum.Parse(LineTypeName, ELineType.NARRATION, $"{nameof(StoryLineData)}(line {LineId}).{nameof(LineTypeName)}");
    }
}
