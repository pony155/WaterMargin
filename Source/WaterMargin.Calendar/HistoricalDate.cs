namespace WaterMargin.Calendar;

/// <summary>
/// A civil date using the Julian calendar through 1582-10-04 and the Gregorian
/// calendar from 1582-10-15. The intervening reform dates are invalid.
/// </summary>
public readonly record struct HistoricalDate : IComparable<HistoricalDate>
{
    public HistoricalDate(int year, int month, int day)
    {
        CalendarMath.ValidateDate(year, month, day);
        Year = year;
        Month = month;
        Day = day;
    }

    public int Year { get; }

    public int Month { get; }

    public int Day { get; }

    public bool UsesGregorianCalendar => CalendarMath.UsesGregorianCalendar(Year, Month, Day);

    public DayOfWeek DayOfWeek => (DayOfWeek)MathUtils.FloorMod(CalendarMath.ToDayNumber(this) + 1, 7);

    public HistoricalDate AddDays(int days) =>
        CalendarMath.FromDayNumber(checked(CalendarMath.ToDayNumber(this) + days));

    public int CompareTo(HistoricalDate other) =>
        CalendarMath.ToDayNumber(this).CompareTo(CalendarMath.ToDayNumber(other));

    public static int DaysBetween(HistoricalDate start, HistoricalDate end) =>
        checked(CalendarMath.ToDayNumber(end) - CalendarMath.ToDayNumber(start));

    public static bool operator <(HistoricalDate left, HistoricalDate right) => left.CompareTo(right) < 0;

    public static bool operator <=(HistoricalDate left, HistoricalDate right) => left.CompareTo(right) <= 0;

    public static bool operator >(HistoricalDate left, HistoricalDate right) => left.CompareTo(right) > 0;

    public static bool operator >=(HistoricalDate left, HistoricalDate right) => left.CompareTo(right) >= 0;

    public override string ToString() => $"{Year:D4}-{Month:D2}-{Day:D2}";
}

internal static class CalendarMath
{
    internal const int GregorianCutoverDayNumber = 2_299_161;
    internal const double J2000JulianDay = 2_451_545.0;

    internal static void ValidateDate(int year, int month, int day)
    {
        if (year is < 1 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be in the range 1 through 9999.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be in the range 1 through 12.");
        }

        int daysInMonth = GetDaysInMonth(year, month, UsesGregorianCalendar(year, month, day));
        if (day is < 1 || day > daysInMonth)
        {
            throw new ArgumentOutOfRangeException(nameof(day), day, $"Day must be valid for {year:D4}-{month:D2}.");
        }

        if (year == 1582 && month == 10 && day is >= 5 and <= 14)
        {
            throw new ArgumentOutOfRangeException(nameof(day), day, "Dates 1582-10-05 through 1582-10-14 were skipped by the configured civil-calendar reform.");
        }
    }

    internal static bool UsesGregorianCalendar(int year, int month, int day) =>
        year > 1582 ||
        (year == 1582 && (month > 10 || (month == 10 && day >= 15)));

    internal static int ToDayNumber(HistoricalDate date)
    {
        ValidateDate(date.Year, date.Month, date.Day);
        int a = (14 - date.Month) / 12;
        int y = date.Year + 4800 - a;
        int m = date.Month + (12 * a) - 3;

        return date.UsesGregorianCalendar
            ? date.Day + ((153 * m + 2) / 5) + (365 * y) + (y / 4) - (y / 100) + (y / 400) - 32045
            : date.Day + ((153 * m + 2) / 5) + (365 * y) + (y / 4) - 32083;
    }

    internal static HistoricalDate FromDayNumber(int dayNumber)
    {
        if (dayNumber >= GregorianCutoverDayNumber)
        {
            int a = dayNumber + 32044;
            int b = (4 * a + 3) / 146097;
            int c = a - ((146097 * b) / 4);
            int d = (4 * c + 3) / 1461;
            int e = c - ((1461 * d) / 4);
            int m = (5 * e + 2) / 153;
            int day = e - ((153 * m + 2) / 5) + 1;
            int month = m + 3 - (12 * (m / 10));
            int year = (100 * b) + d - 4800 + (m / 10);
            return new HistoricalDate(year, month, day);
        }

        int cJulian = dayNumber + 32082;
        int dJulian = (4 * cJulian + 3) / 1461;
        int eJulian = cJulian - ((1461 * dJulian) / 4);
        int mJulian = (5 * eJulian + 2) / 153;
        int julianDay = eJulian - ((153 * mJulian + 2) / 5) + 1;
        int julianMonth = mJulian + 3 - (12 * (mJulian / 10));
        int julianYear = dJulian - 4800 + (mJulian / 10);
        return new HistoricalDate(julianYear, julianMonth, julianDay);
    }

    internal static int GetDaysInMonth(int year, int month, bool gregorian)
    {
        if (month == 2)
        {
            bool leap = gregorian
                ? year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)
                : year % 4 == 0;
            return leap ? 29 : 28;
        }

        return month is 4 or 6 or 9 or 11 ? 30 : 31;
    }
}

internal static class MathUtils
{
    internal static int FloorMod(int value, int modulus)
    {
        int result = value % modulus;
        return result < 0 ? result + modulus : result;
    }
}
