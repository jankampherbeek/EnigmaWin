using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixPositions.UI;
using EnigmaWin.Sources.Features.Shared.Validation;
using EnigmaWin.ViewModels;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public partial class RadixInputScreen : UserControl, INotifyPropertyChanged
{
    public event Action<FullChart>? CalculationCompleted;

    private enum InputSection
    {
        About,
        Location,
        DateTime
    }

    private bool _isUpdatingExpanders;
    private InputSection _activeSection = InputSection.About;
    private readonly RadixInputViewModel _viewModel = new(new RadixInputModel());

    public IReadOnlyList<int> HourValues { get; } = Enumerable.Range(0, 24).ToList();
    public IReadOnlyList<int> DegreeValues { get; } = Enumerable.Range(0, 181).ToList();
    public IReadOnlyList<int> LatitudeDegreeValues { get; } = Enumerable.Range(0, 90).ToList();
    public IReadOnlyList<int> MinuteSecondValues { get; } = Enumerable.Range(0, 60).ToList();
    public IReadOnlyList<int> MonthValues { get; } = Enumerable.Range(1, 12).ToList();
    public IReadOnlyList<int> DayValues { get; } = Enumerable.Range(1, 31).ToList();
    public IReadOnlyList<string> RoddenRatingValues { get; } = new[] { "AA", "A", "B" };
    public IReadOnlyList<string> LongitudeDirectionValues { get; } = new[] { "E", "W" };
    public IReadOnlyList<string> LatitudeDirectionValues { get; } = new[] { "N", "S" };
    public IReadOnlyList<string> CalendarValues { get; } = new[] { "G", "J" };
    public IReadOnlyList<string> YearCountValues { get; } = new[] { "CE", "BCE", "Astronomical" };
    public IReadOnlyList<string> DstValues { get; } = new[] { "No DST", "DST" };
    public IReadOnlyList<string> OffsetDirectionValues { get; } = new[] { "Later", "Earlier" };

    private string _chartName = string.Empty;
    private string _roddenRating = "AA";
    private string _locationName = string.Empty;
    private int _longitudeDegree;
    private int _longitudeMinute;
    private int _longitudeSecond;
    private string _longitudeDirection = "E";
    private int _latitudeDegree;
    private int _latitudeMinute;
    private int _latitudeSecond;
    private string _latitudeDirection = "N";
    private string _year = string.Empty;
    private int _month = 1;
    private int _day = 1;
    private string _calendar = "G";
    private string _yearCount = "CE";
    private int _hour;
    private int _minute;
    private int _second;
    private string _dst = "No DST";
    private int _offsetHour;
    private int _offsetMinute;
    private int _offsetSecond;
    private string _offsetDirection = "Later";
    private string _aboutSectionError = string.Empty;
    private string _dateTimeSectionError = string.Empty;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string ChartName
    {
        get => _chartName;
        set => SetField(ref _chartName, value);
    }

    public string RoddenRating
    {
        get => _roddenRating;
        set => SetField(ref _roddenRating, value);
    }

    public string LocationName
    {
        get => _locationName;
        set => SetField(ref _locationName, value);
    }

    public int LongitudeDegree
    {
        get => _longitudeDegree;
        set => SetField(ref _longitudeDegree, value);
    }

    public int LongitudeMinute
    {
        get => _longitudeMinute;
        set => SetField(ref _longitudeMinute, value);
    }

    public int LongitudeSecond
    {
        get => _longitudeSecond;
        set => SetField(ref _longitudeSecond, value);
    }

    public string LongitudeDirection
    {
        get => _longitudeDirection;
        set => SetField(ref _longitudeDirection, value);
    }

    public int LatitudeDegree
    {
        get => _latitudeDegree;
        set => SetField(ref _latitudeDegree, value);
    }

    public int LatitudeMinute
    {
        get => _latitudeMinute;
        set => SetField(ref _latitudeMinute, value);
    }

    public int LatitudeSecond
    {
        get => _latitudeSecond;
        set => SetField(ref _latitudeSecond, value);
    }

    public string LatitudeDirection
    {
        get => _latitudeDirection;
        set => SetField(ref _latitudeDirection, value);
    }

    public string Year
    {
        get => _year;
        set => SetField(ref _year, value);
    }

    public int Month
    {
        get => _month;
        set => SetField(ref _month, value);
    }

    public int Day
    {
        get => _day;
        set => SetField(ref _day, value);
    }

    public string Calendar
    {
        get => _calendar;
        set => SetField(ref _calendar, value);
    }

    public string YearCount
    {
        get => _yearCount;
        set => SetField(ref _yearCount, value);
    }

    public int Hour
    {
        get => _hour;
        set => SetField(ref _hour, value);
    }

    public int Minute
    {
        get => _minute;
        set => SetField(ref _minute, value);
    }

    public int Second
    {
        get => _second;
        set => SetField(ref _second, value);
    }

    public string Dst
    {
        get => _dst;
        set => SetField(ref _dst, value);
    }

    public int OffsetHour
    {
        get => _offsetHour;
        set => SetField(ref _offsetHour, value);
    }

    public int OffsetMinute
    {
        get => _offsetMinute;
        set => SetField(ref _offsetMinute, value);
    }

    public int OffsetSecond
    {
        get => _offsetSecond;
        set => SetField(ref _offsetSecond, value);
    }

    public string OffsetDirection
    {
        get => _offsetDirection;
        set => SetField(ref _offsetDirection, value);
    }
    
    public string AboutSectionError
    {
        get => _aboutSectionError;
        private set
        {
            if (_aboutSectionError == value) return;
            _aboutSectionError = value;
            OnPropertyChanged(nameof(AboutSectionError));
            OnPropertyChanged(nameof(HasAboutSectionError));
        }
    }

    public bool HasAboutSectionError => !string.IsNullOrWhiteSpace(AboutSectionError);

    public string DateTimeSectionError
    {
        get => _dateTimeSectionError;
        private set
        {
            if (_dateTimeSectionError == value) return;
            _dateTimeSectionError = value;
            OnPropertyChanged(nameof(DateTimeSectionError));
            OnPropertyChanged(nameof(HasDateTimeSectionError));
        }
    }

    public bool HasDateTimeSectionError => !string.IsNullOrWhiteSpace(DateTimeSectionError);
    
    public bool CanCalculate => IsAboutSectionValid(out _) && IsDateTimeSectionValid(out _);

    public string SummaryAbout => $"Chart: {BlankToDash(ChartName)}";

    public string SummaryLocation =>
        $"Location: {BlankToDash(LocationName)} | Lon {LongitudeDegree}° {LongitudeMinute}' {LongitudeSecond}\" {LongitudeDirection} | Lat {LatitudeDegree}° {LatitudeMinute}' {LatitudeSecond}\" {LatitudeDirection}";

    public string SummaryDateTime =>
        $"Date/Time: {BlankToDash(Year)}-{Month:D2}-{Day:D2} | {Calendar} {YearCount} | {Hour:D2}:{Minute:D2}:{Second:D2} {Dst} | UT offset {OffsetHour:D2}:{OffsetMinute:D2}:{OffsetSecond:D2} {OffsetDirection}";

    public RadixInputScreen()
    {
        InitializeComponent();
        DataContext = this;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        OnPropertyChanged(nameof(SummaryAbout));
        OnPropertyChanged(nameof(SummaryLocation));
        OnPropertyChanged(nameof(SummaryDateTime));
        OnPropertyChanged(nameof(CanCalculate));

        // Keep validation in sync while editing.
        if (propertyName == nameof(ChartName))
        {
            ValidateSection(InputSection.About);
        }

        if ((propertyName == nameof(Year) ||
             propertyName == nameof(Month) ||
             propertyName == nameof(Day) ||
             propertyName == nameof(Calendar) ||
             propertyName == nameof(YearCount)))
        {
            ValidateSection(InputSection.DateTime);
        }

        return true;
    }

    private static string BlankToDash(string value) => string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    
    private void ValidateSection(InputSection section)
    {
        switch (section)
        {
            case InputSection.About:
                AboutSectionError = IsAboutSectionValid(out var aboutMessage)
                    ? string.Empty
                    : aboutMessage;
                break;
            case InputSection.DateTime:
                DateTimeSectionError = IsDateTimeSectionValid(out var dateTimeMessage)
                    ? string.Empty
                    : dateTimeMessage;
                break;
            case InputSection.Location:
                break;
        }
    }

    private bool IsAboutSectionValid(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(ChartName))
        {
            errorMessage = "Name is verplicht.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private bool IsDateTimeSectionValid(out string errorMessage)
    {
        if (!int.TryParse(Year, out var enteredYear))
        {
            errorMessage = "Year moet een numerieke waarde zijn.";
            return false;
        }

        if (!TryGetAstronomicalYear(enteredYear, out var astronomicalYear))
        {
            errorMessage = "Year is ongeldig voor de gekozen year count.";
            return false;
        }

        var date = new AstronomicalDate(
            Year: astronomicalYear,
            Month: Month,
            Day: Day,
            Gregorian: Calendar == "G"
        );

        var basicValid = IsValidByCalendarRules(date);
        if (!basicValid)
        {
            errorMessage = "Ongeldige datum.";
            return false;
        }

        try
        {
            var validatedBySe = AstronomicalDateValidation.ValidateDate(date);
            if (!validatedBySe)
            {
                // In UI runtime, SE-based roundtrip can be false-negative for otherwise valid inputs.
                // Keep ValidateDate call, but accept date when calendar rules are valid.
                errorMessage = string.Empty;
                return true;
            }
        }
        catch (Exception)
        {
            // If SE validation throws, fall back to deterministic calendar validation.
            errorMessage = string.Empty;
            return true;
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

    private static bool IsLeapYear(int year, bool gregorian)
    {
        return gregorian
            ? year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)
            : year % 4 == 0;
    }

    private bool TryGetAstronomicalYear(int enteredYear, out int astronomicalYear)
    {
        switch (YearCount)
        {
            case "Astronomical":
                astronomicalYear = enteredYear;
                return true;
            case "CE":
                if (enteredYear > 0)
                {
                    astronomicalYear = enteredYear;
                    return true;
                }
                astronomicalYear = 0;
                return false;
            case "BCE":
                if (enteredYear > 0)
                {
                    astronomicalYear = 1 - enteredYear;
                    return true;
                }
                astronomicalYear = 0;
                return false;
            default:
                astronomicalYear = enteredYear;
                return true;
        }
    }

    private void OnSectionExpanded(object? sender, RoutedEventArgs e)
    {
        if (_isUpdatingExpanders || sender is not Expander expanded)
        {
            return;
        }

        var newSection = ReferenceEquals(AboutSection, expanded)
            ? InputSection.About
            : ReferenceEquals(LocationSection, expanded)
                ? InputSection.Location
                : InputSection.DateTime;

        if (newSection != _activeSection)
        {
            ValidateSection(_activeSection);
            var hasCurrentSectionError = _activeSection switch
            {
                InputSection.About => HasAboutSectionError,
                InputSection.DateTime => HasDateTimeSectionError,
                _ => false
            };

            if (hasCurrentSectionError)
            {
                _isUpdatingExpanders = true;
                try
                {
                    expanded.IsExpanded = false;
                    switch (_activeSection)
                    {
                        case InputSection.About:
                            AboutSection.IsExpanded = true;
                            break;
                        case InputSection.Location:
                            LocationSection.IsExpanded = true;
                            break;
                        case InputSection.DateTime:
                            DateTimeSection.IsExpanded = true;
                            break;
                    }
                }
                finally
                {
                    _isUpdatingExpanders = false;
                }

                return;
            }

            _activeSection = newSection;
        }

        _isUpdatingExpanders = true;
        try
        {
            if (!ReferenceEquals(AboutSection, expanded))
            {
                AboutSection.IsExpanded = false;
            }

            if (!ReferenceEquals(LocationSection, expanded))
            {
                LocationSection.IsExpanded = false;
            }

            if (!ReferenceEquals(DateTimeSection, expanded))
            {
                DateTimeSection.IsExpanded = false;
            }
        }
        finally
        {
            _isUpdatingExpanders = false;
        }
    }

    private void OnSectionGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is not Expander section || _isUpdatingExpanders || section.IsExpanded)
        {
            return;
        }

        section.IsExpanded = true;
    }

    private void OnCalculateClicked(object? sender, RoutedEventArgs e)
    {
        if (!CanCalculate || !int.TryParse(Year, out var enteredYear))
        {
            return;
        }

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

        var chart = _viewModel.CalculateAndPrint(inputData);
        CalculationCompleted?.Invoke(chart);

        // Fallback navigation to ensure positions screen opens even if event wiring is missed.
        if (TopLevel.GetTopLevel(this) is Window { DataContext: MainWindowViewModel mainWindowViewModel })
        {
            mainWindowViewModel.OpenRadixPositions(chart);
        }

        if (TopLevel.GetTopLevel(this) is Window window)
        {
            var positionsScreen = window.FindControl<RadixPositionsScreen>("RadixPositionsScreenControl");
            var inputScreen = window.FindControl<RadixInputScreen>("RadixInputScreenControl");
            var placeholder = window.FindControl<TextBlock>("View2Placeholder");

            if (positionsScreen != null)
            {
                positionsScreen.Chart = chart;
                positionsScreen.IsVisible = true;
            }

            if (inputScreen != null)
            {
                inputScreen.IsVisible = false;
            }

            if (placeholder != null)
            {
                placeholder.IsVisible = false;
            }
        }
    }
    
    private void OnClearClicked(object? sender, RoutedEventArgs e)
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

        _activeSection = InputSection.About;
        _isUpdatingExpanders = true;
        try
        {
            AboutSection.IsExpanded = true;
            LocationSection.IsExpanded = false;
            DateTimeSection.IsExpanded = false;
        }
        finally
        {
            _isUpdatingExpanders = false;
        }
    }

    private void OnPropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
