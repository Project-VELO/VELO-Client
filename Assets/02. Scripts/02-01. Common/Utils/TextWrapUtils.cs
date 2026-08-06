using System.Text;

/// <summary>
/// 글자 폭 기준으로 문자열에 줄바꿈을 넣는 유틸리티입니다.
///
/// 폰트 실측 폭이 아니라 "한글·대문자 1자, 소문자 0.5자"라는 기획 규칙을 그대로 쓰는 이유는,
/// 대사 길이를 기획 단계에서 세어 맞출 수 있어야 하기 때문입니다. TMP의 자동 워드랩은
/// RectTransform 폭에 따라 결과가 달라져 기획서에 적힌 줄 수를 보장하지 못합니다.
/// </summary>
public static class TextWrapUtils
{
    private const float LOWERCASE_CHAR_WIDTH = 0.5f;
    private const float DEFAULT_CHAR_WIDTH = 1f;

    /// <summary>
    /// 단어 단위로 줄바꿈을 넣어 반환합니다. 한 줄의 누적 폭이 <paramref name="maxWidthPerLine"/>을
    /// 넘기는 단어는 다음 줄로 내려가며, 줄 수가 <paramref name="maxLineCount"/>에 도달하면
    /// 남은 단어는 폭과 무관하게 마지막 줄에 이어 붙입니다.
    ///
    /// 공백은 폭 계산에서 제외하고, 연속 공백과 기존 줄바꿈은 단일 공백으로 정규화합니다.
    /// 한 단어가 그 자체로 최대 폭을 넘더라도 단어 중간을 자르지 않습니다.
    /// </summary>
    public static string Wrap(string text, float maxWidthPerLine, int maxLineCount)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        if (maxLineCount <= 1 || maxWidthPerLine <= 0f)
        {
            return text;
        }

        StringBuilder builder = new StringBuilder(text.Length + maxLineCount);
        float lineWidth = 0f;
        int lineCount = 1;
        bool isFirstWord = true;
        int index = 0;

        while (index < text.Length)
        {
            index = SkipWhiteSpaces(text, index);
            if (text.Length <= index)
            {
                break;
            }

            int wordEnd = FindWordEnd(text, index);
            float wordWidth = MeasureWidth(text, index, wordEnd);

            if (isFirstWord)
            {
                builder.Append(text, index, wordEnd - index);
                lineWidth = wordWidth;
                isFirstWord = false;
            }
            else
            {
                float appendedWidth = lineWidth + wordWidth;
                bool canBreakLine = lineCount < maxLineCount;

                if (canBreakLine && maxWidthPerLine < appendedWidth)
                {
                    builder.Append('\n');
                    builder.Append(text, index, wordEnd - index);
                    lineWidth = wordWidth;
                    lineCount++;
                }
                else
                {
                    builder.Append(' ');
                    builder.Append(text, index, wordEnd - index);
                    lineWidth = appendedWidth;
                }
            }

            index = wordEnd;
        }

        return builder.ToString();
    }

    /// <summary>
    /// 공백을 제외한 문자열의 폭을 계산합니다.
    /// </summary>
    public static float MeasureWidth(string text, int startIndex, int endIndex)
    {
        float width = 0f;

        for (int i = startIndex; i < endIndex; i++)
        {
            width += GetCharWidth(text[i]);
        }

        return width;
    }

    private static float GetCharWidth(char character)
    {
        if ('a' <= character && character <= 'z')
        {
            return LOWERCASE_CHAR_WIDTH;
        }

        return DEFAULT_CHAR_WIDTH;
    }

    private static int SkipWhiteSpaces(string text, int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        return index;
    }

    private static int FindWordEnd(string text, int index)
    {
        while (index < text.Length && !char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        return index;
    }
}
