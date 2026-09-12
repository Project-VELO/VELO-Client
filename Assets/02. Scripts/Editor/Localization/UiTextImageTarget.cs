using UnityEngine;

/// <summary>
/// 글자 그림 하나를 TMP로 바꿀 때 필요한 값입니다.
///
/// 크기와 색을 표에 적어 두는 것은 원본 그림에서 뽑은 값이기 때문입니다.
/// 그림을 지우고 나면 다시 잴 수 없으므로 바꾸기 전에 옮겨 적어 둡니다.
/// </summary>
public struct UiTextImageTarget
{
    /// <summary>프리팹 또는 씬 파일 경로입니다.</summary>
    public string AssetPath;

    /// <summary>루트를 뺀 오브젝트 경로입니다. 씬은 루트 오브젝트 이름부터 적습니다.</summary>
    public string NodePath;

    /// <summary>한국어 원문입니다. 일본어는 UiText가 실행 중에 바꿔 줍니다.</summary>
    public string Korean;

    public int FontSize;

    /// <summary>원본 그림에서 뽑은 대표 글자색입니다.</summary>
    public string ColorHex;

    /// <summary>TMP의 HorizontalAlignmentOptions 값입니다(1 왼쪽, 2 가운데, 4 오른쪽).</summary>
    public int HorizontalAlignment;

    public bool IsBold;

    /// <summary>
    /// 상자 크기를 다시 잡을 때 씁니다. 0이면 있던 크기를 그대로 둡니다.
    ///
    /// 그림으로 두던 자리는 상자가 그림의 원본 크기로 잡혀 있습니다(SetNativeSize).
    /// 글자는 그보다 넓게 잡아야 짧은 문구와 긴 문구가 같은 자리에 들어갑니다.
    /// </summary>
    public Vector2 RectSize;

    public UiTextImageTarget(string assetPath, string nodePath, string korean,
        int fontSize, string colorHex, int horizontalAlignment = 2, bool isBold = false,
        float rectWidth = 0f, float rectHeight = 0f)
    {
        RectSize = new Vector2(rectWidth, rectHeight);
        AssetPath = assetPath;
        NodePath = nodePath;
        Korean = korean;
        FontSize = fontSize;
        ColorHex = colorHex;
        HorizontalAlignment = horizontalAlignment;
        IsBold = isBold;
    }

    public Color Color
    {
        get
        {
            return ColorUtility.TryParseHtmlString(ColorHex, out Color parsed) ? parsed : Color.white;
        }
    }
}
