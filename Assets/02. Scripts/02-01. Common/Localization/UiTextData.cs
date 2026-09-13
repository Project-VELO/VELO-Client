using System;

/// <summary>
/// UI 글자 한 줄의 번역입니다.
///
/// 다른 마스터 데이터와 달리 Id가 없고 한국어 원문 자체가 키입니다.
/// UI 글자는 프리팹에 박혀 있어 항목마다 Id를 달려면 프리팹을 아흔 곳 넘게 고쳐야 하고,
/// 앞으로 글자를 더할 때마다 Id를 붙이는 일이 따라붙습니다.
/// 원문을 키로 두면 표에 한 줄 적는 것으로 같은 문구가 쓰인 모든 자리가 함께 바뀝니다.
///
/// 대신 같은 단어가 자리마다 다른 뜻이면 갈라낼 수 없습니다. 그런 문구가 생기면
/// 그때 Id 방식으로 옮기면 됩니다.
/// </summary>
[Serializable]
public class UiTextData
{
    /// <summary>
    /// 한국어 원문입니다. 프리팹에 적힌 글자와 한 글자도 다르면 안 됩니다(줄바꿈 포함).
    /// </summary>
    public string Source;

    public string Text;
}
