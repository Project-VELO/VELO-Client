using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// 주차·날짜 표시 문구를 만듭니다.
///
/// 홈과 사무실이 같은 스케줄 목록을 보여주므로, 화면마다 문구가 갈라지지 않도록 여기 하나로 모읍니다.
/// </summary>
public static class ScheduleDayLabel
{
    private const string UNKNOWN_DAY = "-";

    private const int DAYS_PER_WEEK = 7;

    private const string START_DATE_FORMAT = "yyyy-MM-dd";

    /// <summary>
    /// 화면에 찍는 날짜 형태입니다. 시안이 "3/14"처럼 0을 채우지 않아 M/d를 씁니다.
    /// </summary>
    private const string DISPLAY_DATE_FORMAT = "M/d";

    /// <summary>
    /// 일차 순번을 그대로 요일로 읽습니다. 주차가 7일 고정이고 1일차가 주의 시작이므로(기획서 3-E-1)
    /// 달력 날짜 없이도 요일이 결정됩니다.
    /// </summary>
    private static readonly List<string> WEEKDAYS = new List<string>
    {
        "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"
    };

    /// <summary>
    /// dayNumber가 0이면 주차 목록에서 날짜를 찾지 못한 경우입니다.
    /// 그대로 적으면 "0일차"라는 없는 날짜가 화면에 뜨므로 빈 표시로 대신합니다.
    /// </summary>
    public static string Format(int weekOrder, int dayNumber)
    {
        if (dayNumber <= 0)
        {
            return UNKNOWN_DAY;
        }

        return $"{weekOrder}주차 {dayNumber}일차";
    }

    /// <summary>
    /// 주간 스케줄 표의 칸에 적을 달력 날짜입니다("3/14").
    ///
    /// 극중 첫날(newgame_config의 CalendarStartDate)에서 지난 일수만큼 더해 구합니다.
    /// 월말을 넘길 때 며칠까지 있는지는 DateTime이 알아서 처리하므로 여기서 따지지 않습니다.
    /// </summary>
    public static string FormatDate(int weekOrder, int dayOrder)
    {
        if (weekOrder <= 0 || dayOrder <= 0)
        {
            return UNKNOWN_DAY;
        }

        if (!TryGetStartDate(out DateTime startDate))
        {
            return UNKNOWN_DAY;
        }

        int elapsedDays = (weekOrder - 1) * DAYS_PER_WEEK + (dayOrder - 1);
        return startDate.AddDays(elapsedDays).ToString(DISPLAY_DATE_FORMAT, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 값이 비었거나 형식이 어긋나면 날짜를 지어내지 않고 빈 표시로 넘깁니다.
    /// 잘못된 날짜가 화면에 뜨는 것보다, 비어 있는 편이 데이터가 틀렸다는 것을 알아채기 쉽습니다.
    /// </summary>
    private static bool TryGetStartDate(out DateTime startDate)
    {
        startDate = default;

        NewGameConfigData config = MasterDataProvider.Instance.NewGameConfig;

        if (config == null || string.IsNullOrWhiteSpace(config.CalendarStartDate))
        {
            return false;
        }

        return DateTime.TryParseExact(config.CalendarStartDate, START_DATE_FORMAT,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate);
    }

    /// <summary>
    /// 주간 스케줄 표의 칸에 적을 요일입니다(기획서 3-E-3-1).
    /// 범위를 벗어난 일차는 없는 요일을 지어내지 않고 빈 표시로 대신합니다.
    /// </summary>
    public static string FormatWeekday(int dayOrder)
    {
        if (dayOrder <= 0 || WEEKDAYS.Count < dayOrder)
        {
            return UNKNOWN_DAY;
        }

        return WEEKDAYS[dayOrder - 1];
    }
}
