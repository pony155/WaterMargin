using System.Collections.ObjectModel;

namespace WaterMargin.Calendar;

public enum SolarTerm
{
    MinorCold,
    MajorCold,
    BeginningOfSpring,
    RainWater,
    AwakeningOfInsects,
    SpringEquinox,
    ClearAndBright,
    GrainRain,
    BeginningOfSummer,
    GrainFull,
    GrainInEar,
    SummerSolstice,
    MinorHeat,
    MajorHeat,
    BeginningOfAutumn,
    EndOfHeat,
    WhiteDew,
    AutumnEquinox,
    ColdDew,
    FrostDescent,
    BeginningOfWinter,
    MinorSnow,
    MajorSnow,
    WinterSolstice,
}

public static class SolarTermRules
{
    public static int GetSolarLongitudeDegrees(this SolarTerm term)
    {
        int index = (int)term;
        if (index is < 0 or >= 24)
        {
            throw new ArgumentOutOfRangeException(nameof(term), term, "Unknown solar term.");
        }

        return (285 + (15 * index)) % 360;
    }

    public static bool IsPrincipalTerm(this SolarTerm term) =>
        term.GetSolarLongitudeDegrees() % 30 == 0;
}

public readonly record struct SolarTermOccurrence(
    SolarTerm Term,
    HistoricalDate Date,
    TimeSpan LocalTimeOfDay,
    double JulianDayUtc);

public enum HeavenlyStem
{
    Jia,
    Yi,
    Bing,
    Ding,
    Wu,
    Ji,
    Geng,
    Xin,
    Ren,
    Gui,
}

public enum EarthlyBranch
{
    Zi,
    Chou,
    Yin,
    Mao,
    Chen,
    Si,
    Wu,
    Wei,
    Shen,
    You,
    Xu,
    Hai,
}

public enum ChineseZodiac
{
    Rat,
    Ox,
    Tiger,
    Rabbit,
    Dragon,
    Snake,
    Horse,
    Goat,
    Monkey,
    Rooster,
    Dog,
    Pig,
}

public readonly record struct SexagenaryYear(HeavenlyStem Stem, EarthlyBranch Branch)
{
    public ChineseZodiac Zodiac => (ChineseZodiac)Branch;

    public static SexagenaryYear FromChineseYear(int year)
    {
        if (year < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be positive.");
        }

        return new SexagenaryYear(
            (HeavenlyStem)MathUtils.FloorMod(year - 4, 10),
            (EarthlyBranch)MathUtils.FloorMod(year - 4, 12));
    }
}

public readonly record struct ChineseDate
{
    public ChineseDate(int year, int month, int day, bool isLeapMonth = false)
    {
        if (year < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be positive.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be in the range 1 through 12.");
        }

        if (day is < 1 or > 30)
        {
            throw new ArgumentOutOfRangeException(nameof(day), day, "Day must be in the range 1 through 30.");
        }

        Year = year;
        Month = month;
        Day = day;
        IsLeapMonth = isLeapMonth;
    }

    public int Year { get; }

    public int Month { get; }

    public int Day { get; }

    public bool IsLeapMonth { get; }

    public SexagenaryYear SexagenaryYear => SexagenaryYear.FromChineseYear(Year);
}

public sealed class ChineseMonth
{
    internal ChineseMonth(
        int year,
        int number,
        bool isLeapMonth,
        HistoricalDate startDate,
        HistoricalDate endDateExclusive)
    {
        Year = year;
        Number = number;
        IsLeapMonth = isLeapMonth;
        StartDate = startDate;
        EndDateExclusive = endDateExclusive;
        DayCount = HistoricalDate.DaysBetween(startDate, endDateExclusive);

        if (DayCount is not (29 or 30))
        {
            throw new InvalidOperationException($"Calculated lunar month has invalid length {DayCount}.");
        }
    }

    public int Year { get; }

    public int Number { get; }

    public bool IsLeapMonth { get; }

    public int DayCount { get; }

    public HistoricalDate StartDate { get; }

    public HistoricalDate EndDateExclusive { get; }

    public bool Contains(HistoricalDate date) => date >= StartDate && date < EndDateExclusive;
}

public sealed class ChineseCalendarYear
{
    private readonly ReadOnlyCollection<ChineseMonth> _months;

    internal ChineseCalendarYear(int year, ChineseMonth[] months)
    {
        if (months.Length is not (12 or 13))
        {
            throw new InvalidOperationException($"Calculated Chinese year has invalid month count {months.Length}.");
        }

        Year = year;
        _months = Array.AsReadOnly(months);
    }

    public int Year { get; }

    public SexagenaryYear SexagenaryYear => SexagenaryYear.FromChineseYear(Year);

    public IReadOnlyList<ChineseMonth> Months => _months;

    public HistoricalDate StartDate => _months[0].StartDate;

    public HistoricalDate EndDateExclusive => _months[^1].EndDateExclusive;

    public ChineseMonth GetMonth(int number, bool isLeapMonth = false)
    {
        ChineseMonth? month = _months.FirstOrDefault(candidate =>
            candidate.Number == number && candidate.IsLeapMonth == isLeapMonth);

        return month ?? throw new ArgumentOutOfRangeException(
            nameof(number),
            number,
            $"Chinese year {Year} does not contain the requested {(isLeapMonth ? "leap " : string.Empty)}month.");
    }
}
