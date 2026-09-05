using CosineKitty;

namespace WaterMargin.Calendar;

/// <summary>
/// Deterministic astronomical Chinese calendar for WaterMargin's supported
/// historical span. Calendar days use a fixed UTC+08:00 boundary.
/// </summary>
public sealed class AncientChineseCalendar
{
    public const int MinimumSupportedYear = 960;
    public const int MaximumSupportedYear = 1644;
    public const int LocalUtcOffsetHours = 8;

    private const int YearCacheCapacity = 8;
    private const double LocalUtcOffsetDays = LocalUtcOffsetHours / 24.0;

    private readonly object _cacheGate = new();
    private readonly Dictionary<int, ChineseCalendarYear> _yearCache = [];
    private readonly Queue<int> _cacheInsertionOrder = [];

    public ChineseCalendarYear GetYear(int chineseYear)
    {
        ValidateSupportedYear(chineseYear);

        lock (_cacheGate)
        {
            if (_yearCache.TryGetValue(chineseYear, out ChineseCalendarYear? cached))
            {
                return cached;
            }

            ChineseCalendarYear calculated = CalculateYear(chineseYear);
            if (_yearCache.Count == YearCacheCapacity)
            {
                int oldestYear = _cacheInsertionOrder.Dequeue();
                _yearCache.Remove(oldestYear);
            }

            _yearCache.Add(chineseYear, calculated);
            _cacheInsertionOrder.Enqueue(chineseYear);
            return calculated;
        }
    }

    public ChineseDate FromCivilDate(HistoricalDate date)
    {
        int candidateYear = Math.Clamp(date.Year, MinimumSupportedYear, MaximumSupportedYear);
        ChineseCalendarYear year = GetYear(candidateYear);

        if (date < year.StartDate)
        {
            if (candidateYear == MinimumSupportedYear)
            {
                throw OutsideSupportedDateRange(date);
            }

            year = GetYear(candidateYear - 1);
        }
        else if (date >= year.EndDateExclusive)
        {
            if (candidateYear == MaximumSupportedYear)
            {
                throw OutsideSupportedDateRange(date);
            }

            year = GetYear(candidateYear + 1);
        }

        ChineseMonth? month = year.Months.FirstOrDefault(candidate => candidate.Contains(date));
        if (month is null)
        {
            throw OutsideSupportedDateRange(date);
        }

        int day = HistoricalDate.DaysBetween(month.StartDate, date) + 1;
        return new ChineseDate(year.Year, month.Number, day, month.IsLeapMonth);
    }

    public HistoricalDate ToCivilDate(ChineseDate date)
    {
        ValidateSupportedYear(date.Year);
        ChineseMonth month = GetYear(date.Year).GetMonth(date.Month, date.IsLeapMonth);
        if (date.Day > month.DayCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(date),
                date,
                $"The requested lunar month has only {month.DayCount} days.");
        }

        return month.StartDate.AddDays(date.Day - 1);
    }

    public IReadOnlyList<SolarTermOccurrence> GetSolarTerms(int civilYear)
    {
        ValidateSupportedYear(civilYear);
        SolarEvent[] events = CalculateSolarTerms(civilYear);
        return Array.AsReadOnly(events.Select(value => value.Occurrence).ToArray());
    }

    public bool TryGetSolarTerm(HistoricalDate date, out SolarTermOccurrence occurrence)
    {
        if (date < GetYear(MinimumSupportedYear).StartDate ||
            date >= GetYear(MaximumSupportedYear).EndDateExclusive)
        {
            occurrence = default;
            return false;
        }

        int dayNumber = CalendarMath.ToDayNumber(date);
        int firstCycleYear = Math.Max(date.Year - 1, MinimumSupportedYear - 1);
        int lastCycleYear = Math.Min(date.Year + 1, MaximumSupportedYear + 1);

        for (int cycleYear = firstCycleYear; cycleYear <= lastCycleYear; ++cycleYear)
        {
            foreach (SolarEvent candidate in CalculateSolarTerms(cycleYear))
            {
                if (candidate.LocalDayNumber == dayNumber)
                {
                    occurrence = candidate.Occurrence;
                    return true;
                }
            }
        }

        occurrence = default;
        return false;
    }

    private static ChineseCalendarYear CalculateYear(int chineseYear)
    {
        SolarEvent[] previousTerms = CalculateSolarTerms(chineseYear - 1);
        SolarEvent[] currentTerms = CalculateSolarTerms(chineseYear);
        SolarEvent[] nextTerms = CalculateSolarTerms(chineseYear + 1);
        SolarEvent[] allTerms = [.. previousTerms, .. currentTerms, .. nextTerms];

        SolarEvent previousSolstice = FindWinterSolstice(previousTerms);
        SolarEvent currentSolstice = FindWinterSolstice(currentTerms);
        SolarEvent nextSolstice = FindWinterSolstice(nextTerms);

        MoonEvent[] moons = CalculateNewMoons(
            previousSolstice.Time.AddDays(-40.0),
            nextSolstice.Time.AddDays(60.0));

        int previousMonthEleven = FindContainingMonth(moons, previousSolstice.LocalDayNumber);
        int currentMonthEleven = FindContainingMonth(moons, currentSolstice.LocalDayNumber);
        int nextMonthEleven = FindContainingMonth(moons, nextSolstice.LocalDayNumber);

        Dictionary<int, MonthLabel> labels = [];
        LabelSui(moons, allTerms, previousMonthEleven, currentMonthEleven, labels);
        LabelSui(moons, allTerms, currentMonthEleven, nextMonthEleven, labels);

        int newYearStart = FindNewYearMonth(labels, previousMonthEleven, currentMonthEleven);
        int nextNewYearStart = FindNewYearMonth(labels, currentMonthEleven, nextMonthEleven);

        ChineseMonth[] months = new ChineseMonth[nextNewYearStart - newYearStart];
        for (int index = newYearStart; index < nextNewYearStart; ++index)
        {
            MonthLabel label = labels[index];
            months[index - newYearStart] = new ChineseMonth(
                chineseYear,
                label.Number,
                label.IsLeap,
                CalendarMath.FromDayNumber(moons[index].LocalDayNumber),
                CalendarMath.FromDayNumber(moons[index + 1].LocalDayNumber));
        }

        return new ChineseCalendarYear(chineseYear, months);
    }

    private static SolarEvent[] CalculateSolarTerms(int civilYear)
    {
        SolarEvent[] events = new SolarEvent[24];
        // Solar-term cycle years follow the conventional proleptic Gregorian
        // seasonal year. The public HistoricalDate conversion may therefore
        // place early terms in December of the preceding Julian civil year.
        AstroTime searchTime = new AstroTime(civilYear, 1, 1, 0, 0, 0.0).AddDays(-2.0);

        for (int index = 0; index < events.Length; ++index)
        {
            SolarTerm term = (SolarTerm)index;
            AstroTime? eventTime = Astronomy.SearchSunLongitude(
                term.GetSolarLongitudeDegrees(),
                searchTime,
                25.0);

            if (eventTime is null)
            {
                throw new InvalidOperationException($"Unable to calculate {term} for civil year {civilYear}.");
            }

            events[index] = CreateSolarEvent(term, eventTime);
            searchTime = eventTime.AddDays(1.0);
        }

        return events;
    }

    private static MoonEvent[] CalculateNewMoons(AstroTime startTime, AstroTime endTime)
    {
        List<MoonEvent> moons = new(32);
        AstroTime searchTime = startTime;

        while (searchTime.ut <= endTime.ut)
        {
            AstroTime? eventTime = Astronomy.SearchMoonPhase(0.0, searchTime, 40.0);
            if (eventTime is null)
            {
                throw new InvalidOperationException("Unable to calculate a required new moon.");
            }

            if (eventTime.ut > endTime.ut)
            {
                break;
            }

            moons.Add(new MoonEvent(eventTime, GetLocalDayNumber(eventTime)));
            searchTime = eventTime.AddDays(1.0);
        }

        if (moons.Count < 26)
        {
            throw new InvalidOperationException("Astronomical span did not contain enough new moons.");
        }

        return [.. moons];
    }

    private static void LabelSui(
        IReadOnlyList<MoonEvent> moons,
        IReadOnlyList<SolarEvent> terms,
        int firstMonthEleven,
        int nextMonthEleven,
        IDictionary<int, MonthLabel> labels)
    {
        int monthCount = nextMonthEleven - firstMonthEleven;
        if (monthCount is not (12 or 13))
        {
            throw new InvalidOperationException($"Calculated sui has invalid month count {monthCount}.");
        }

        int leapMonthIndex = -1;
        if (monthCount == 13)
        {
            for (int index = firstMonthEleven + 1; index < nextMonthEleven; ++index)
            {
                if (!ContainsPrincipalTerm(moons, terms, index))
                {
                    leapMonthIndex = index;
                    break;
                }
            }

            if (leapMonthIndex < 0)
            {
                throw new InvalidOperationException("Thirteen-month sui has no leap-month candidate.");
            }
        }

        int number = 11;
        AddOrValidateLabel(labels, firstMonthEleven, new MonthLabel(number, false));

        for (int index = firstMonthEleven + 1; index <= nextMonthEleven; ++index)
        {
            bool isLeap = index == leapMonthIndex;
            if (!isLeap)
            {
                number = number == 12 ? 1 : number + 1;
            }

            AddOrValidateLabel(labels, index, new MonthLabel(number, isLeap));
        }
    }

    private static bool ContainsPrincipalTerm(
        IReadOnlyList<MoonEvent> moons,
        IReadOnlyList<SolarEvent> terms,
        int monthIndex)
    {
        int startDay = moons[monthIndex].LocalDayNumber;
        int endDay = moons[monthIndex + 1].LocalDayNumber;
        return terms.Any(term =>
            term.Term.IsPrincipalTerm() &&
            term.LocalDayNumber >= startDay &&
            term.LocalDayNumber < endDay);
    }

    private static void AddOrValidateLabel(
        IDictionary<int, MonthLabel> labels,
        int index,
        MonthLabel label)
    {
        if (labels.TryGetValue(index, out MonthLabel existing))
        {
            if (existing != label)
            {
                throw new InvalidOperationException("Adjacent sui calculations disagree on month eleven.");
            }

            return;
        }

        labels.Add(index, label);
    }

    private static int FindContainingMonth(IReadOnlyList<MoonEvent> moons, int localDayNumber)
    {
        for (int index = moons.Count - 2; index >= 0; --index)
        {
            if (moons[index].LocalDayNumber <= localDayNumber &&
                localDayNumber < moons[index + 1].LocalDayNumber)
            {
                return index;
            }
        }

        throw new InvalidOperationException("Solar term lies outside the calculated new-moon span.");
    }

    private static int FindNewYearMonth(
        IReadOnlyDictionary<int, MonthLabel> labels,
        int firstMonthEleven,
        int nextMonthEleven)
    {
        for (int index = firstMonthEleven + 1; index < nextMonthEleven; ++index)
        {
            MonthLabel label = labels[index];
            if (label.Number == 1 && !label.IsLeap)
            {
                return index;
            }
        }

        throw new InvalidOperationException("Calculated sui does not contain a regular first month.");
    }

    private static SolarEvent FindWinterSolstice(IEnumerable<SolarEvent> terms) =>
        terms.Single(term => term.Term == SolarTerm.WinterSolstice);

    private static SolarEvent CreateSolarEvent(SolarTerm term, AstroTime time)
    {
        double localJulianDay = CalendarMath.J2000JulianDay + time.ut + LocalUtcOffsetDays;
        int localDayNumber = checked((int)Math.Floor(localJulianDay + 0.5));
        double fractionOfDay = localJulianDay + 0.5 - localDayNumber;
        long ticks = checked((long)Math.Round(fractionOfDay * TimeSpan.TicksPerDay));

        if (ticks == TimeSpan.TicksPerDay)
        {
            ++localDayNumber;
            ticks = 0;
        }

        SolarTermOccurrence occurrence = new(
            term,
            CalendarMath.FromDayNumber(localDayNumber),
            TimeSpan.FromTicks(ticks),
            CalendarMath.J2000JulianDay + time.ut);
        return new SolarEvent(term, time, localDayNumber, occurrence);
    }

    private static int GetLocalDayNumber(AstroTime time) =>
        checked((int)Math.Floor(CalendarMath.J2000JulianDay + time.ut + LocalUtcOffsetDays + 0.5));

    private static void ValidateSupportedYear(int year)
    {
        if (year is < MinimumSupportedYear or > MaximumSupportedYear)
        {
            throw new ArgumentOutOfRangeException(
                nameof(year),
                year,
                $"WaterMargin's ancient Chinese calendar supports years {MinimumSupportedYear} through {MaximumSupportedYear}.");
        }
    }

    private static ArgumentOutOfRangeException OutsideSupportedDateRange(HistoricalDate date) =>
        new(
            nameof(date),
            date,
            $"Date is outside Chinese years {MinimumSupportedYear} through {MaximumSupportedYear}.");

    private readonly record struct MonthLabel(int Number, bool IsLeap);

    private sealed record MoonEvent(AstroTime Time, int LocalDayNumber);

    private sealed record SolarEvent(
        SolarTerm Term,
        AstroTime Time,
        int LocalDayNumber,
        SolarTermOccurrence Occurrence);
}
