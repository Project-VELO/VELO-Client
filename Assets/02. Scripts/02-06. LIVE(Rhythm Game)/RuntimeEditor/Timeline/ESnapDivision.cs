/// <summary>
/// 채보 에디터의 그리드 스냅 분박 단위입니다. 값은 4분음표 1비트당 그리드 칸 수를 의미합니다.
/// 열거형 이름은 4/4박자 기준 마디 분할 표기(1/4박 ~ 1/32박)를 따르며,
/// 모든 값은 LiveEditorBpmTimeConverter의 비트당 24칸 해상도를 나눠 떨어지게 합니다.
/// </summary>
public enum ESnapDivision
{
    Quarter = 1,
    Eighth = 2,
    Twelfth = 3,
    Sixteenth = 4,
    ThirtySecond = 8,
}
