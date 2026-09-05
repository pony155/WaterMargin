using WaterMargin.Calendar;

CalendarTests.Run();
return 0;

internal static class CalendarTests
{
    public static void Run()
    {
        CivilCalendarUsesTheDocumentedReform();
        SexagenaryYearUsesStableDomainValues();
        SongEraYearsHaveValidLunarMonths();
        ConversionRoundTripsAcrossMonthBoundaries();
        SolarTermsAreOrderedAndBounded();
        InvalidDatesAndYearsAreRejected();
        Console.WriteLine("Ancient Chinese calendar tests passed.");
    }

    private static void CivilCalendarUsesTheDocumentedReform()
    {
        HistoricalDate lastJulianDay = new(1582, 10, 4);
        HistoricalDate firstGregorianDay = new(1582, 10, 15);
        Equal(firstGregorianDay, lastJulianDay.AddDays(1), "Civil cutover did not skip the reform gap.");
        Equal(1, HistoricalDate.DaysBetween(lastJulianDay, firstGregorianDay), "Civil cutover is not contiguous.");
        Equal(DayOfWeek.Thursday, lastJulianDay.DayOfWeek, "Julian cutover weekday is wrong.");
        Equal(DayOfWeek.Friday, firstGregorianDay.DayOfWeek, "Gregorian cutover weekday is wrong.");
    }

    private static void SexagenaryYearUsesStableDomainValues()
    {
        SexagenaryYear cycle = SexagenaryYear.FromChineseYear(1120);
        Equal(HeavenlyStem.Geng, cycle.Stem, "The 1120 heavenly stem is wrong.");
        Equal(EarthlyBranch.Zi, cycle.Branch, "The 1120 earthly branch is wrong.");
        Equal(ChineseZodiac.Rat, cycle.Zodiac, "The 1120 zodiac is wrong.");
    }

    private static void SongEraYearsHaveValidLunarMonths()
    {
        AncientChineseCalendar calendar = new();
        foreach (int yearNumber in new[] { 960, 1082, 1120, 1279, 1582, 1644 })
        {
            ChineseCalendarYear year = calendar.GetYear(yearNumber);
            True(year.Months.Count is 12 or 13, $"Chinese year {yearNumber} has an invalid month count.");
            Equal(1, year.Months[0].Number, $"Chinese year {yearNumber} does not begin with month one.");
            False(year.Months[0].IsLeapMonth, $"Chinese year {yearNumber} begins with a leap month.");
            Equal(year.StartDate, year.Months[0].StartDate, "Year start does not match its first month.");

            for (int index = 0; index < year.Months.Count; ++index)
            {
                ChineseMonth month = year.Months[index];
                True(month.DayCount is 29 or 30, "Lunar month length is invalid.");
                if (index + 1 < year.Months.Count)
                {
                    Equal(month.EndDateExclusive, year.Months[index + 1].StartDate, "Lunar months are not contiguous.");
                }
            }
        }
    }

    private static void ConversionRoundTripsAcrossMonthBoundaries()
    {
        AncientChineseCalendar calendar = new();
        foreach (int yearNumber in new[] { 1082, 1120, 1121 })
        {
            ChineseCalendarYear year = calendar.GetYear(yearNumber);
            foreach (ChineseMonth month in year.Months)
            {
                foreach (int day in new[] { 1, month.DayCount })
                {
                    ChineseDate chinese = new(yearNumber, month.Number, day, month.IsLeapMonth);
                    HistoricalDate civil = calendar.ToCivilDate(chinese);
                    Equal(chinese, calendar.FromCivilDate(civil), "Chinese/civil conversion did not round-trip.");
                }
            }
        }
    }

    private static void SolarTermsAreOrderedAndBounded()
    {
        AncientChineseCalendar calendar = new();
        IReadOnlyList<SolarTermOccurrence> terms = calendar.GetSolarTerms(1120);
        Equal(24, terms.Count, "A civil year must contain 24 solar terms.");

        double previousJulianDay = double.NegativeInfinity;
        foreach (SolarTermOccurrence occurrence in terms)
        {
            True(occurrence.JulianDayUtc > previousJulianDay, "Solar terms are not chronological.");
            True(
                occurrence.Date.Year is 1119 or 1120,
                "Solar term escaped the requested seasonal cycle.");
            True(occurrence.LocalTimeOfDay >= TimeSpan.Zero, "Solar term local time is negative.");
            True(occurrence.LocalTimeOfDay < TimeSpan.FromDays(1), "Solar term local time exceeds one day.");
            True(calendar.TryGetSolarTerm(occurrence.Date, out SolarTermOccurrence found), "Solar-term lookup failed.");
            Equal(occurrence.Term, found.Term, "Solar-term lookup returned the wrong term.");
            previousJulianDay = occurrence.JulianDayUtc;
        }
    }

    private static void InvalidDatesAndYearsAreRejected()
    {
        Throws<ArgumentOutOfRangeException>(() => _ = new HistoricalDate(1582, 10, 10));
        AncientChineseCalendar calendar = new();
        Throws<ArgumentOutOfRangeException>(() => calendar.GetYear(959));
        Throws<ArgumentOutOfRangeException>(() => calendar.GetYear(1645));

        ChineseMonth shortMonth = calendar.GetYear(1120).Months.First(month => month.DayCount == 29);
        Throws<ArgumentOutOfRangeException>(() => calendar.ToCivilDate(
            new ChineseDate(1120, shortMonth.Number, 30, shortMonth.IsLeapMonth)));
    }

    private static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void False(bool condition, string message) => True(!condition, message);

    private static void Equal<T>(T expected, T actual, string message)
        where T : notnull
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message} Expected '{expected}', received '{actual}'.");
        }
    }

    private static void Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
    }
}
