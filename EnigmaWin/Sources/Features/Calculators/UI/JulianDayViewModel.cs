// JulianDayViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.I18n;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Calculators.UI;

public class JulianDayViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;

    // ── Labels ──────────────────────────────────────────────────────────────
    public string LabelTitle            => _rosetta.GetText(RbFile.Calculators, "view.julianday.title");
    public string LabelDateTimeToJd     => _rosetta.GetText(RbFile.Calculators, "view.julianday.datetimetojd");
    public string LabelCalcJd           => _rosetta.GetText(RbFile.Calculators, "view.julianday.calcbutton");
    public string LabelResultJd         => _rosetta.GetText(RbFile.Calculators, "view.julianday.result.jd");
    public string LabelResultDeltaT     => _rosetta.GetText(RbFile.Calculators, "view.julianday.result.deltat");
    public string LabelJdToDateTime     => _rosetta.GetText(RbFile.Calculators, "view.julianday.todatetime");
    public string LabelJdInput          => _rosetta.GetText(RbFile.Calculators, "view.julianday.input");
    public string LabelCalcDateTime     => _rosetta.GetText(RbFile.Calculators, "view.julianday.calcdatetimebutton");
    public string LabelResultDate       => _rosetta.GetText(RbFile.Calculators, "view.julianday.result.date");
    public string LabelResultTime       => _rosetta.GetText(RbFile.Calculators, "view.julianday.result.time");
    public string LabelDate             => _rosetta.GetText(RbFile.Calculators, "view.calculators.date");
    public string LabelCalendarYearCount => _rosetta.GetText(RbFile.Calculators, "view.calculators.calendaryearcount");
    public string LabelTime             => _rosetta.GetText(RbFile.Calculators, "view.calculators.time");
    public string LabelOffsetUt         => _rosetta.GetText(RbFile.Calculators, "view.calculators.offsetut");

    // ── Date/time input (date → JD) ─────────────────────────────────────────
    public IReadOnlyList<int> HourValues          { get; } = Enumerable.Range(0, 24).ToList();
    public IReadOnlyList<int> MinuteSecondValues  { get; } = Enumerable.Range(0, 60).ToList();
    public IReadOnlyList<int> MonthValues         { get; } = Enumerable.Range(1, 12).ToList();
    public IReadOnlyList<int> DayValues           { get; } = Enumerable.Range(1, 31).ToList();
    public IReadOnlyList<DisplayItem<CalendarStyle>>     CalendarValues        { get; private set; } = [];
    public IReadOnlyList<DisplayItem<YearCount>>         YearCountValues       { get; private set; } = [];
    public IReadOnlyList<DisplayItem<UTOffsetDirection>> OffsetDirectionValues { get; private set; } = [];

    private string _year = "2000";
    public string Year
    {
        get => _year;
        set { _year = value; OnPropertyChanged(); }
    }

    private int _month = 1;
    public int Month
    {
        get => _month;
        set { _month = value; OnPropertyChanged(); }
    }

    private int _day = 1;
    public int Day
    {
        get => _day;
        set { _day = value; OnPropertyChanged(); }
    }

    private DisplayItem<CalendarStyle> _calendar = null!;
    public DisplayItem<CalendarStyle> Calendar
    {
        get => _calendar;
        set { _calendar = value; OnPropertyChanged(); }
    }

    private DisplayItem<YearCount> _yearCount = null!;
    public DisplayItem<YearCount> YearCount
    {
        get => _yearCount;
        set { _yearCount = value; OnPropertyChanged(); }
    }

    private int _hour = 12;
    public int Hour
    {
        get => _hour;
        set { _hour = value; OnPropertyChanged(); }
    }

    private int _minute = 0;
    public int Minute
    {
        get => _minute;
        set { _minute = value; OnPropertyChanged(); }
    }

    private int _second = 0;
    public int Second
    {
        get => _second;
        set { _second = value; OnPropertyChanged(); }
    }

    private int _offsetHour = 0;
    public int OffsetHour
    {
        get => _offsetHour;
        set { _offsetHour = value; OnPropertyChanged(); }
    }

    private int _offsetMinute = 0;
    public int OffsetMinute
    {
        get => _offsetMinute;
        set { _offsetMinute = value; OnPropertyChanged(); }
    }

    private DisplayItem<UTOffsetDirection> _offsetDirection = null!;
    public DisplayItem<UTOffsetDirection> OffsetDirection
    {
        get => _offsetDirection;
        set { _offsetDirection = value; OnPropertyChanged(); }
    }

    // ── Results: date → JD ──────────────────────────────────────────────────
    private string _jdResult = string.Empty;
    public string JdResult
    {
        get => _jdResult;
        set { _jdResult = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasJdResult)); }
    }

    private string _deltaTResult = string.Empty;
    public string DeltaTResult
    {
        get => _deltaTResult;
        set { _deltaTResult = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasJdResult)); }
    }

    public bool HasJdResult => !string.IsNullOrEmpty(JdResult);

    // ── JD → date input ─────────────────────────────────────────────────────
    private string _jdInput = string.Empty;
    public string JdInput
    {
        get => _jdInput;
        set { _jdInput = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanCalculateDateTime)); }
    }

    public bool CanCalculateDateTime => double.TryParse(JdInput, out _);

    // ── Results: JD → date ──────────────────────────────────────────────────
    private string _dateResult = string.Empty;
    public string DateResult
    {
        get => _dateResult;
        set { _dateResult = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDateResult)); }
    }

    private string _timeResult = string.Empty;
    public string TimeResult
    {
        get => _timeResult;
        set { _timeResult = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDateResult)); }
    }

    public bool HasDateResult => !string.IsNullOrEmpty(DateResult);

    public event PropertyChangedEventHandler? PropertyChanged;

    public JulianDayViewModel(IRosetta rosetta)
    {
        _rosetta = rosetta;
        InitializeLists();
    }

    private void InitializeLists()
    {
        CalendarValues        = ToDisplayList<CalendarStyle>(v => EnumKeySelector.Key(v));
        YearCountValues       = ToDisplayList<Domain.YearCount>(v => EnumKeySelector.Key(v));
        OffsetDirectionValues = ToDisplayList<UTOffsetDirection>(v => EnumKeySelector.Key(v));

        Calendar        = CalendarValues.First(v => v.Value == CalendarStyle.Gregorian);
        YearCount       = YearCountValues.First(v => v.Value == Domain.YearCount.CE);
        OffsetDirection = OffsetDirectionValues.First(v => v.Value == UTOffsetDirection.Later);
    }

    public void CalculateJulianDay()
    {
        if (!TryGetAstronomicalYear(out var astroYear)) return;

        var date = new AstronomicalDate(astroYear, Month, Day, Calendar.Value == CalendarStyle.Gregorian);
        var time = new AstronomicalTime(Hour, Minute, Second);
        var localJd = SEWrapper.JulianDay(date, time);

        var offsetSeconds = OffsetHour * 3600.0 + OffsetMinute * 60.0;
        var sign = OffsetDirection.Value == UTOffsetDirection.Earlier ? -1.0 : 1.0;
        var utJd = localJd + sign * offsetSeconds / 86400.0;

        JdResult     = utJd.ToString("F6", CultureInfo.InvariantCulture);
        DeltaTResult = SEWrapper.CalculateDeltaT(utJd).ToString("F2", CultureInfo.InvariantCulture) + " s";
    }

    public void CalculateDateTime()
    {
        if (!double.TryParse(JdInput, NumberStyles.Any, CultureInfo.InvariantCulture, out var jd)) return;

        var dt = SEWrapper.DateFromJulianDay(jd, gregorian: true);
        DateResult = $"{dt.Date.Year:D4}-{dt.Date.Month:D2}-{dt.Date.Day:D2}";
        TimeResult = $"{dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{dt.Time.Second:D2}";
    }

    private bool TryGetAstronomicalYear(out int astroYear)
    {
        astroYear = 0;
        if (!int.TryParse(Year, out var y)) return false;
        astroYear = YearCount.Value switch
        {
            Domain.YearCount.Astronomical => y,
            Domain.YearCount.CE  => y > 0 ? y : -1,
            Domain.YearCount.BCE => y > 0 ? 1 - y : -1,
            _ => y
        };
        return astroYear != -1;
    }

    private IReadOnlyList<DisplayItem<T>> ToDisplayList<T>(Func<T, string> keySelector) where T : struct, Enum =>
        Enum.GetValues<T>()
            .Select(v => new DisplayItem<T>(v, _rosetta.GetText(RbFile.Localizable, keySelector(v))))
            .ToList();

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
