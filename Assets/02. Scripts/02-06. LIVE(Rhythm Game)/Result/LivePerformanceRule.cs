/// <summary>
/// 결과 화면 퍼포먼스 별의 개수 규칙입니다.
///
/// 등급을 그대로 별로 옮깁니다. 정확도 구간을 따로 잘라 쓰면 등급 경계와 어긋나,
/// S인데 별이 A와 같은 개수로 나오는 줄이 생깁니다.
/// </summary>
public static class LivePerformanceRule
{
    public const int MAX_STAR_COUNT = 5;

    public static int GetStarCount(ELiveRank rank)
    {
        switch (rank)
        {
            case ELiveRank.PERFECT_S:
                return 5;

            case ELiveRank.S:
                return 4;

            case ELiveRank.A:
                return 3;

            case ELiveRank.B:
                return 2;

            case ELiveRank.C:
                return 1;

            default:
                return 0;
        }
    }
}
