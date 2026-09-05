# Ancient Chinese Calendar

## Feature Status

- [x] Pin and vendor the MIT-licensed Astronomy Engine C# implementation.
- [x] Calculate astronomical new moons and all 24 solar terms.
- [x] Assign month eleven, ordinary months, and leap months from principal terms.
- [x] Convert between Chinese dates and the configured historical civil dates.
- [x] Provide stable heavenly-stem, earthly-branch, and zodiac identities.
- [x] Fix the calendar day boundary at UTC+08:00 and bound the runtime year cache.
- [x] Add compile-time coverage for civil reform, month invariants, conversion,
  solar terms, bounds, and representative Song-era years.
- [ ] Validate story-critical dates against a selected primary historical
  Northern Song almanac before treating them as scholarly historical claims.
- [ ] Connect localized calendar display names to application UI.

## Scope

`WaterMargin.Calendar` is a deterministic game-owned chronology for Chinese
years 960 through 1644. It derives lunar months from astronomical new moons and
the 24 solar terms from apparent solar longitude. It exposes stable numeric and
enum identities; it does not own translated month, stem, branch, zodiac, or
solar-term display strings.

This is an astronomical projection designed for simulation and presentation.
It is not a reconstruction of every dynasty-specific calendar reform or a
claim that computed event times reproduce a particular surviving almanac.
Story-critical historical dates require separate source review.

## Rules

- A lunar month begins on the UTC+08:00 civil day containing its astronomical
  new moon.
- Month eleven is the month containing the winter solstice.
- A span between consecutive month elevens normally contains twelve months.
- When that span contains thirteen months, the first month after month eleven
  without a principal solar term is the leap month and repeats the preceding
  month number.
- Chinese New Year is the first ordinary month one after the preceding winter
  solstice.
- Solar terms occur at successive 15-degree apparent solar longitudes. Terms
  at multiples of 30 degrees are principal terms.
- The sexagenary year uses 4 CE as the `Jia`/`Zi` epoch. The enum values are
  identifiers, not player-visible transliterations.

## Civil-date convention

`HistoricalDate` follows the reference project's hybrid convention:

- Julian calendar through 1582-10-04;
- Gregorian calendar from 1582-10-15; and
- 1582-10-05 through 1582-10-14 are invalid skipped dates.

This convention describes the comparison date used by the API. It does not
suggest that Song China used the European civil calendar. Consecutive dates are
represented by an integer astronomical day number, so adding one day across
the reform advances directly from October 4 to October 15.

All lunar month and solar-term day classification uses a fixed UTC+08:00
offset. Host timezone, daylight saving, current culture, wall time, and OS
calendar settings cannot affect results.

## Architecture

`ThirdParty/AstronomyEngine` contains unmodified upstream `astronomy.cs` and
the MIT license pinned at commit
`865d3da7d8112bbc7911238052c6af4aaf877181`. Its WaterMargin-owned project file
adapts the source to the repository's offline .NET 10 build.

`AncientChineseCalendar` is the only Game-facing calculation service. Public
contracts contain no `CosineKitty` types. A calculation gathers three adjacent
civil years of solar terms, a bounded new-moon span, and labels the two
winter-solstice cycles needed to construct one Chinese year.

Instances retain at most eight immutable `ChineseCalendarYear` results. Cache
insertion and eviction are protected by the instance lock and use stable FIFO
order. Cache state changes only performance, never calculated values.

## Public API

- `GetYear(year)` returns the ordered twelve or thirteen lunar months.
- `FromCivilDate(date)` returns its Chinese year, month, leap flag, and day.
- `ToCivilDate(date)` validates and converts a Chinese date.
- `GetSolarTerms(year)` returns all 24 occurrences with local date/time and UTC
  Julian day for the conventional seasonal cycle. With the hybrid comparison
  calendar, an early term can fall in December of the preceding Julian year.
- `TryGetSolarTerm(date, out occurrence)` identifies a term falling on a civil
  day.
- `SexagenaryYear.FromChineseYear(year)` returns stable stem, branch, and zodiac
  identities.

Invalid years, invalid civil reform dates, nonexistent leap months, and day 30
in a 29-day month fail explicitly. Calculations do not silently clamp or consult
platform calendar data.

## Verification and limitations

The compile-only calendar test target covers representative years 960, 1082,
1120, 1279, 1582, and 1644; twelve/thirteen-month and 29/30-day invariants;
month-boundary round trips; all solar terms; supported bounds; and the hybrid
civil reform. Test execution remains user/CI-owned under repository policy.

Astronomical results depend on historical Delta T estimates and the truncated
model accuracy of Astronomy Engine. Near-midnight new moons or principal terms
can therefore place a calendar boundary on an adjacent day compared with a
different historical model. Pinning the dependency and fixed offset ensures
WaterMargin remains reproducible even if another model would choose a
different historical day.

## Revision History

- 2026-09-05 — Added the initial deterministic implementation using Astronomy
  Engine 2.1.19 at commit `865d3da`; Codex (GPT-5).
