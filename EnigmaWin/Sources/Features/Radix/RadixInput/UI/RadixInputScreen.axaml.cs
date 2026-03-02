using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public partial class RadixInputScreen : UserControl, INotifyPropertyChanged
{
    private bool _isUpdatingExpanders;

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
    public IReadOnlyList<string> OffsetDirectionValues { get; } = new[] { "Earlier", "Later" };

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
    private string _offsetDirection = "Earlier";

    public event PropertyChangedEventHandler? PropertyChanged;

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
        return true;
    }

    private static string BlankToDash(string value) => string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();

    private void OnSectionExpanded(object? sender, RoutedEventArgs e)
    {
        if (_isUpdatingExpanders || sender is not Expander expanded)
        {
            return;
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

    private void OnPropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
