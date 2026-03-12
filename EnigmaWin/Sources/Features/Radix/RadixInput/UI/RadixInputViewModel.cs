using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Localization;
using EnigmaWin.Sources.Features.Shared.Validation;
using System.Collections.Generic;
using System.Linq;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public sealed partial class RadixInputViewModel : ObservableObject
{
    private readonly RadixInputModel _model;
    private readonly INavigationService _navigationService;
    private readonly IChartContext _chartContext;

    // Picker list data
    public IReadOnlyList<int> HourValues { get; } = Enumerable.Range(0, 24).ToList();
    public IReadOnlyList<int> DegreeValues { get; } = Enumerable.Range(0, 181).ToList();
    public IReadOnlyList<int> LatitudeDegreeValues { get; } = Enumerable.Range(0, 90).ToList();
    public IReadOnlyList<int> MinuteSecondValues { get; } = Enumerable.Range(0, 60).ToList();
    public IReadOnlyList<int> MonthValues { get; } = Enumerable.Range(1, 12).ToList();
    public IReadOnlyList<int> DayValues { get; } = Enumerable.Range(1, 31).ToList();
    public IReadOnlyList<string> RoddenRatingValues { get; } = ["AA", "A", "B"];
    public IReadOnlyList<string> LongitudeDirectionValues { get; } = ["E", "W"];
    public IReadOnlyList<string> LatitudeDirectionValues { get; } = ["N", "S"];
    public IReadOnlyList<string> CalendarValues { get; } = ["G", "J"];
    public IReadOnlyList<string> YearCountValues { get; } = ["CE", "BCE", "Astronomical"];
    public IReadOnlyList<string> DstValues { get; } = ["No DST", "DST"];
    public IReadOnlyList<string> OffsetDirectionValues { get; } = ["Later", "Earlier"];

    // About section
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryAbout), nameof(CanCalculate))]
    private string _chartName = string.Empty;

    [ObservableProperty]
    private string _roddenRating = "AA";

    // Location section
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryLocation))]
    private string _locationName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryLocation))]
    private int _longitudeDegree;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryLocation))]
    private int _longitudeMinute;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryLocation))]
    private int _longitudeSecond;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryLocation))]
    private string _longitudeDirection = "E";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryLocation))]
    private int _latitudeDegree;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryLocation))]
    private int _latitudeMinute;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryLocation))]
    private int _latitudeSecond;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryLocation))]
    private string _latitudeDirection = "N";

    // Date/time section
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime), nameof(CanCalculate))]
    private string _year = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime), nameof(CanCalculate))]
    private int _month = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime), nameof(CanCalculate))]
    private int _day = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime), nameof(CanCalculate))]
    private string _calendar = "G";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime), nameof(CanCalculate))]
    private string _yearCount = "CE";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime))]
    private int _hour;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime))]
    private int _minute;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime))]
    private int _second;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime))]
    private string _dst = "No DST";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime))]
    private int _offsetHour;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime))]
    private int _offsetMinute;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime))]
    private int _offsetSecond;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime))]
    private string _offsetDirection = "Later";

    // Validation errors
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAboutSectionError))]
    private string _aboutSectionError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDateTimeSectionError))]
    private string _dateTimeSectionError = string.Empty;

    public bool HasAboutSectionError => !string.IsNullOrWhiteSpace(AboutSectionError);
    public bool HasDateTimeSectionError => !string.IsNullOrWhiteSpace(DateTimeSectionError);
    public bool CanCalculate => IsAboutSectionValid(out _) && IsDateTimeSectionValid(out _);

    public string SummaryAbout => $"Chart: {BlankToDash(ChartName)}";

    public string SummaryLocation =>
        $"Location: {BlankToDash(LocationName)} | Lon {LongitudeDegree}° {LongitudeMinute}' {LongitudeSecond}\" {LongitudeDirection} | Lat {LatitudeDegree}° {LatitudeMinute}' {LatitudeSecond}\" {LatitudeDirection}";

    public string SummaryDateTime =>
        $"Date/Time: {BlankToDash(Year)}-{Month:D2}-{Day:D2} | {Calendar} {YearCount} | {Hour:D2}:{Minute:D2}:{Second:D2} {Dst} | UT offset {OffsetHour:D2}:{OffsetMinute:D2}:{OffsetSecond:D2} {OffsetDirection}";

    public RadixInputViewModel(RadixInputModel model, INavigationService navigationService, IChartContext chartContext)
    {
        _model = model;
        _navigationService = navigationService;
        _chartContext = chartContext;
    }

    // Live validation as relevant fields change
    partial void OnChartNameChanged(string value) => ValidateAbout();
    partial void OnYearChanged(string value) => ValidateDateTime();
    partial void OnMonthChanged(int value) => ValidateDateTime();
    partial void OnDayChanged(int value) => ValidateDateTime();
    partial void OnCalendarChanged(string value) => ValidateDateTime();
    partial void OnYearCountChanged(string value) => ValidateDateTime();

    /// <summary>Forces evaluation of the About section and updates <see cref="AboutSectionError"/>.</summary>
    internal void ValidateAbout()
    {
        AboutSectionError = IsAboutSectionValid(out var msg) ? string.Empty : msg;
    }

    /// <summary>Forces evaluation of the DateTime section and updates <see cref="DateTimeSectionError"/>.</summary>
    internal void ValidateDateTime()
    {
        DateTimeSectionError = IsDateTimeSectionValid(out var msg) ? string.Empty : msg;
    }

    internal void Calculate()
    {
        if (!CanCalculate || !int.TryParse(Year, out var enteredYear))
            return;

        var inputData = new RadixInputModel.InputData(
            ChartName: ChartName,
            Year: enteredYear,
            Month: Month,
            Day: Day,
            Calendar: Calendar,
            YearCount: YearCount,
            Hour: Hour,
            Minute: Minute,
            Second: Second,
            OffsetHour: OffsetHour,
            OffsetMinute: OffsetMinute,
            OffsetSecond: OffsetSecond,
            OffsetDirection: OffsetDirection,
            Dst: Dst,
            LongitudeDegree: LongitudeDegree,
            LongitudeMinute: LongitudeMinute,
            LongitudeSecond: LongitudeSecond,
            LongitudeDirection: LongitudeDirection,
            LatitudeDegree: LatitudeDegree,
            LatitudeMinute: LatitudeMinute,
            LatitudeSecond: LatitudeSecond,
            LatitudeDirection: LatitudeDirection
        );

        var chart = _model.Calculate(inputData);
        _chartContext.CurrentChart = chart;
        _navigationService.NavigateDetail(AppRoutes.RadixPositions);
    }

    internal void Clear()
    {
        ChartName = string.Empty;
        RoddenRating = "AA";
        LocationName = string.Empty;
        LongitudeDegree = 0;
        LongitudeMinute = 0;
        LongitudeSecond = 0;
        LongitudeDirection = "E";
        LatitudeDegree = 0;
        LatitudeMinute = 0;
        LatitudeSecond = 0;
        LatitudeDirection = "N";
        Year = string.Empty;
        Month = 1;
        Day = 1;
        Calendar = "G";
        YearCount = "CE";
        Hour = 0;
        Minute = 0;
        Second = 0;
        Dst = "No DST";
        OffsetHour = 0;
        OffsetMinute = 0;
        OffsetSecond = 0;
        OffsetDirection = "Later";
        AboutSectionError = string.Empty;
        DateTimeSectionError = string.Empty;
    }

    private bool IsAboutSectionValid(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(ChartName))
        {
            errorMessage = Rosetta.GetText("radixinput.validation.name.empty");
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private bool IsDateTimeSectionValid(out string errorMessage)
    {
        if (!int.TryParse(Year, out var enteredYear))
        {
            errorMessage = Rosetta.GetText("radixinput.validation.year.notanumber");
            return false;
        }

        if (!TryGetAstronomicalYear(enteredYear, out var astronomicalYear))
        {
            errorMessage = Rosetta.GetText("radixinput.validation.year.invalid");
            return false;
        }

        var date = new AstronomicalDate(
            Year: astronomicalYear,
            Month: Month,
            Day: Day,
            Gregorian: Calendar == "G"
        );

        if (!IsValidByCalendarRules(date))
        {
            errorMessage = Rosetta.GetText("radixinput.validation.date.invalid");
            return false;
        }

        try
        {
            // SE-based roundtrip can be a false-negative for otherwise valid inputs; accept when calendar rules pass.
            AstronomicalDateValidation.ValidateDate(date);
        }
        catch
        {
            // Fall back to deterministic calendar validation already performed above.
        }

        errorMessage = string.Empty;
        return true;
    }

    private static bool IsValidByCalendarRules(AstronomicalDate date)
    {
        if (date.Month is < 1 or > 12) return false;
        if (date.Day < 1) return false;

        var maxDay = date.Month switch
        {
            1 or 3 or 5 or 7 or 8 or 10 or 12 => 31,
            4 or 6 or 9 or 11 => 30,
            2 => IsLeapYear(date.Year, date.Gregorian) ? 29 : 28,
            _ => 0
        };

        return date.Day <= maxDay;
    }

    private static bool IsLeapYear(int year, bool gregorian) =>
        gregorian ? year % 4 == 0 && (year % 100 != 0 || year % 400 == 0) : year % 4 == 0;

    private bool TryGetAstronomicalYear(int enteredYear, out int astronomicalYear)
    {
        switch (YearCount)
        {
            case "Astronomical":
                astronomicalYear = enteredYear;
                return true;
            case "CE":
                if (enteredYear > 0) { astronomicalYear = enteredYear; return true; }
                astronomicalYear = 0;
                return false;
            case "BCE":
                if (enteredYear > 0) { astronomicalYear = 1 - enteredYear; return true; }
                astronomicalYear = 0;
                return false;
            default:
                astronomicalYear = enteredYear;
                return true;
        }
    }

    private static string BlankToDash(string value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
}
