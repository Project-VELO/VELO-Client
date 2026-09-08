/// <summary>
/// 언어를 파일 경로에 쓰는 짧은 코드로 바꿉니다.
///
/// 열거형 이름을 그대로 폴더명에 쓰지 않는 것은, 열거형은 코드를 읽기 위한 이름이고
/// 폴더명은 기획이 파일을 넣는 자리이기 때문입니다. 둘을 묶어 두면 열거형 이름을 다듬는 순간
/// 데이터 폴더까지 옮겨야 합니다.
/// </summary>
public static class LanguageCode
{
    public const string KOREAN = "KO";
    public const string JAPANESE = "JA";

    /// <summary>
    /// 번역문 폴더 이름입니다. 한국어는 원본이라 폴더가 없으며 빈 문자열을 돌려줍니다.
    /// </summary>
    public static string GetFolderName(ELanguage language)
    {
        switch (language)
        {
            case ELanguage.JAPANESE: return JAPANESE;
            default: return string.Empty;
        }
    }
}
