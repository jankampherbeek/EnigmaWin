// ObliquityViewModel.cs
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

public class ObliquityViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;

    // ── Labels ──────────────────────────────────────────────────────────────
    public string LabelTitle             => _rosetta.GetText(RbFile.Calculators, "view.obliquity.title");
    public string LabelDateTimeSection   => _rosetta.GetText(RbFile.Calculators, "view.obliquity.datetimesection");
    public string LabelCalcButton        => _rosetta.GetText(RbFile.Calculators, "view.obliquity.calcbutton");
    public string LabelResultMean        => _rosetta.GetText(RbFile.Calculators, "view.obliquity.result.mean");
    public string LabelResultTrue        => _rosetta.GetText(RbFile.Calculators, "view.obliquity.result.true");
    public string LabelDate              => _rosetta.GetText(RbFile.Calculators, "view.calculators.date");
    public string LabelCalendarYearCount => _rosetta.GetText(RbFile.Calculators, "view.calculators.calendaryearcount");
    public string LabelTime              => _rosetta.GetText(RbFile.Calculators, "view.calculators.time");
    public string LabelOffsetUt          => _rosetta.GetText(RbFile.Calculators, "view.calculators.offsetut");

    // ── Date/time input ──────────────────────────────────────────────────────
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

    // ── Results ──────────────────────────────────────────────────────────────
    private string _meanObliquity = string.Empty;
    public string MeanObliquity
    {
        get => _meanObliquity;
        set { _meanObliquity = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasResult)); }
    }

    private string _trueObliquity = string.Empty;
    public string TrueObliquity
    {
        get => _trueObliquity;
        set { _trueObliquity = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasResult)); }
    }

    public bool HasResult => !string.IsNullOrEmpty(MeanObliquity);

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObliquityViewModel(IRosetta rosetta)
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

    public void CalculateObliquity()
    {
        if (!TryGetAstronomicalYear(out var astroYear)) return;

        var date = new AstronomicalDate(astroYear, Month, Day, Calendar.Value == CalendarStyle.Gregorian);
        var time = new AstronomicalTime(Hour, Minute, Second);
        var localJd = SEWrapper.JulianDay(date, time);

        var offsetSeconds = OffsetHour * 3600.0 + OffsetMinute * 60.0;
        var sign = OffsetDirection.Value == UTOffsetDirection.Earlier ? -1.0 : 1.0;
        var utJd = localJd + sign * offsetSeconds / 86400.0;

        // SE returns xx[0]=true obliquity, xx[1]=mean obliquity for ipl=-1
        var pos = SEWrapper.CalculateFactorPosition(utJd, -1, 2);
        if (pos == null) return;

        TrueObliquity = pos.MainPos.ToString("F6", CultureInfo.InvariantCulture) + "°";
        MeanObliquity = pos.Deviation.ToString("F6", CultureInfo.InvariantCulture) + "°";
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
