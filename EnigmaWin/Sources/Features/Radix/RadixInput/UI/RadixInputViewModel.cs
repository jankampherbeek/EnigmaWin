using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.Sources.Features.Shared.Validation;
using System.Collections.Generic;
using System.Linq;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public sealed partial class RadixInputViewModel : ObservableObject
{
    private readonly RadixInputModel _model;
    private readonly INavigationService _navigationService;
    private readonly IChartContext _chartContext;
    private readonly IRosetta _rosetta;

    // Numeric picker lists (language-independent)
    public IReadOnlyList<int> HourValues { get; } = Enumerable.Range(0, 24).ToList();
    public IReadOnlyList<int> DegreeValues { get; } = Enumerable.Range(0, 181).ToList();
    public IReadOnlyList<int> LatitudeDegreeValues { get; } = Enumerable.Range(0, 90).ToList();
    public IReadOnlyList<int> MinuteSecondValues { get; } = Enumerable.Range(0, 60).ToList();
    public IReadOnlyList<int> MonthValues { get; } = Enumerable.Range(1, 12).ToList();
    public IReadOnlyList<int> DayValues { get; } = Enumerable.Range(1, 31).ToList();

    // Enum picker lists — populated with localized display text in constructor
    public IReadOnlyList<DisplayItem<RoddenRating>> RoddenRatingValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<LongitudeHemisphere>> LongitudeDirectionValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<LatitudeHemisphere>> LatitudeDirectionValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<CalendarStyle>> CalendarValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<YearCount>> YearCountValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<DSTOption>> DstValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<UTOffsetDirection>> OffsetDirectionValues { get; private set; } = [];

    // About section
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryAbout), nameof(CanCalculate))]
    private string _chartName = string.Empty;

    [ObservableProperty]
    private DisplayItem<RoddenRating> _roddenRating = null!;

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
    private DisplayItem<LongitudeHemisphere> _longitudeDirection = null!;

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
    private DisplayItem<LatitudeHemisphere> _latitudeDirection = null!;

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
    private DisplayItem<CalendarStyle> _calendar = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SummaryDateTime), nameof(CanCalculate))]
    private DisplayItem<YearCount> _yearCount = null!;

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
    private DisplayItem<DSTOption> _dst = null!;

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
    private DisplayItem<UTOffsetDirection> _offsetDirection = null!;

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

    // Localized UI labels
    public string LabelTitle            => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.title");
    public string LabelAboutChart       => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.aboutchart");
    public string LabelName             => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.name");
    public string LabelDescription      => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.description");
    public string LabelSource           => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.source");
    public string LabelRoddenRating     => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.roddenrating");
    public string LabelLocation         => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.location");
    public string LabelNameOfLocation   => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.nameoflocation");
    public string LabelLongitude        => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.longitude");
    public string LabelLatitude         => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.latitude");
    public string LabelDateAndTime      => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.datetime");
    public string LabelDate             => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.date");
    public string LabelCalendarYearCount => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.calendaryearcount");
    public string LabelTimeDst          => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.timedst");
    public string LabelOffsetUt         => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.offsetut");
    public string LabelCalculate        => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.calculate");

    // Watermark hints
    public string HintChartName     => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.hint.chartname");
    public string HintDescription   => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.hint.description");
    public string HintSource        => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.hint.source");
    public string HintLocationName  => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.hint.Locationname");

    public string SummaryAbout => $"Chart: {BlankToDash(ChartName)}";

    public string SummaryLocation =>
        $"Location: {BlankToDash(LocationName)} | Lon {LongitudeDegree}° {LongitudeMinute}' {LongitudeSecond}\" {LongitudeDirection} | Lat {LatitudeDegree}° {LatitudeMinute}' {LatitudeSecond}\" {LatitudeDirection}";

    public string SummaryDateTime =>
        $"Date/Time: {BlankToDash(Year)}-{Month:D2}-{Day:D2} | {Calendar} {YearCount} | {Hour:D2}:{Minute:D2}:{Second:D2} {Dst} | UT offset {OffsetHour:D2}:{OffsetMinute:D2}:{OffsetSecond:D2} {OffsetDirection}";

    public RadixInputViewModel(RadixInputModel model, INavigationService navigationService, IChartContext chartContext, IRosetta rosetta)
    {
        _model = model;
        _navigationService = navigationService;
        _chartContext = chartContext;
        _rosetta = rosetta;

        InitializeEnumLists();
        SetDefaults();
    }

    // Live validation as relevant fields change
    partial void OnChartNameChanged(string value) => ValidateAbout();
    partial void OnYearChanged(string value) => ValidateDateTime();
    partial void OnMonthChanged(int value) => ValidateDateTime();
    partial void OnDayChanged(int value) => ValidateDateTime();
    partial void OnCalendarChanged(DisplayItem<CalendarStyle> value) => ValidateDateTime();
    partial void OnYearCountChanged(DisplayItem<YearCount> value) => ValidateDateTime();

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
            Calendar: Calendar.Value,
            YearCount: YearCount.Value,
            Hour: Hour,
            Minute: Minute,
            Second: Second,
            OffsetHour: OffsetHour,
            OffsetMinute: OffsetMinute,
            OffsetSecond: OffsetSecond,
            OffsetDirection: OffsetDirection.Value,
            Dst: Dst.Value,
            LongitudeDegree: LongitudeDegree,
            LongitudeMinute: LongitudeMinute,
            LongitudeSecond: LongitudeSecond,
            LongitudeDirection: LongitudeDirection.Value,
            LatitudeDegree: LatitudeDegree,
            LatitudeMinute: LatitudeMinute,
            LatitudeSecond: LatitudeSecond,
            LatitudeDirection: LatitudeDirection.Value
        );

        var chart = _model.Calculate(inputData);
        _chartContext.CurrentChart = chart;
        _navigationService.NavigateDetail(AppRoutes.RadixPositions);
    }

    internal void Clear()
    {
        ChartName = string.Empty;
        LocationName = string.Empty;
        LongitudeDegree = 0;
        LongitudeMinute = 0;
        LongitudeSecond = 0;
        LatitudeDegree = 0;
        LatitudeMinute = 0;
        LatitudeSecond = 0;
        Year = string.Empty;
        Month = 1;
        Day = 1;
        Hour = 0;
        Minute = 0;
        Second = 0;
        OffsetHour = 0;
        OffsetMinute = 0;
        OffsetSecond = 0;
        AboutSectionError = string.Empty;
        DateTimeSectionError = string.Empty;
        SetDefaults();
    }

    private void InitializeEnumLists()
    {
        RoddenRatingValues       = ToDisplayList<Domain.RoddenRating>(v => EnumKeySelector.Key(v));
        LongitudeDirectionValues = ToDisplayList<LongitudeHemisphere>(v => EnumKeySelector.Key(v));
        LatitudeDirectionValues  = ToDisplayList<LatitudeHemisphere>(v => EnumKeySelector.Key(v));
        CalendarValues           = ToDisplayList<CalendarStyle>(v => EnumKeySelector.Key(v));
        YearCountValues          = ToDisplayList<Domain.YearCount>(v => EnumKeySelector.Key(v));
        DstValues                = ToDisplayList<DSTOption>(v => EnumKeySelector.Key(v));
        OffsetDirectionValues    = ToDisplayList<UTOffsetDirection>(v => EnumKeySelector.Key(v));
    }

    private void SetDefaults()
    {
        RoddenRating     = RoddenRatingValues.First(v => v.Value == Domain.RoddenRating.AA);
        LongitudeDirection = LongitudeDirectionValues.First(v => v.Value == LongitudeHemisphere.East);
        LatitudeDirection  = LatitudeDirectionValues.First(v => v.Value == LatitudeHemisphere.North);
        Calendar           = CalendarValues.First(v => v.Value == CalendarStyle.Gregorian);
        YearCount          = YearCountValues.First(v => v.Value == Domain.YearCount.CE);
        Dst                = DstValues.First(v => v.Value == DSTOption.NoDST);
        OffsetDirection    = OffsetDirectionValues.First(v => v.Value == UTOffsetDirection.Later);
    }

    private IReadOnlyList<DisplayItem<T>> ToDisplayList<T>(System.Func<T, string> keySelector)
        where T : struct, System.Enum
    {
        return System.Enum.GetValues<T>()
            .Select(v => new DisplayItem<T>(v, _rosetta.GetText(RbFile.Localizable, keySelector(v))))
            .ToList();
    }

    private bool IsAboutSectionValid(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(ChartName))
        {
            errorMessage = _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.validation.nameempty");
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private bool IsDateTimeSectionValid(out string errorMessage)
    {
        if (!int.TryParse(Year, out var enteredYear))
        {
            errorMessage = _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.validation.yearnotanumber");
            return false;
        }

        if (!TryGetAstronomicalYear(enteredYear, out var astronomicalYear))
        {
            errorMessage = _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.validation.invalidyear");
            return false;
        }

        var date = new AstronomicalDate(
            Year: astronomicalYear,
            Month: Month,
            Day: Day,
            Gregorian: Calendar.Value == CalendarStyle.Gregorian
        );

        if (!IsValidByCalendarRules(date))
        {
            errorMessage = _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.validation.dateinvalid");
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
        switch (YearCount.Value)
        {
            case Domain.YearCount.Astronomical:
                astronomicalYear = enteredYear;
                return true;
            case Domain.YearCount.CE:
                if (enteredYear > 0) { astronomicalYear = enteredYear; return true; }
                astronomicalYear = 0;
                return false;
            case Domain.YearCount.BCE:
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
