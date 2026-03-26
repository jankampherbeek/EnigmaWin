using System;
using System.Linq;
using System.Threading.Tasks;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixInput.UI;
using EnigmaWin.Sources.Features.Shared.I18n;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixEdit.UI;

public sealed partial class RadixEditViewModel : RadixInputViewModel
{
    public new string LabelTitle   => _rosetta.GetText(RbFile.RadixEdit, "view.radixeditscreen.title");
    public     string LabelApply   => _rosetta.GetText(RbFile.RadixEdit, "view.radixeditscreen.apply");
    public new string TooltipHelp  => _rosetta.GetText(RbFile.RadixEdit, "view.radixeditscreen.help.tooltip");

    public RadixEditViewModel(
        RadixInputModel      model,
        INavigationService   navigationService,
        IChartSession        chartSession,
        IRosetta             rosetta,
        IHoroscopeRepository horoscopeRepository,
        IConfigContext       configContext)
        : base(model, navigationService, chartSession, rosetta, horoscopeRepository, configContext)
    {
        LoadFromSession();
    }

    internal async Task ApplyAsync()
    {
        if (!CanCalculate || !int.TryParse(Year, out var enteredYear))
            return;

        var inputData = new RadixInputModel.InputData(
            ChartName:          ChartName,
            Year:               enteredYear,
            Month:              Month,
            Day:                Day,
            Calendar:           Calendar.Value,
            YearCount:          YearCount.Value,
            Hour:               Hour,
            Minute:             Minute,
            Second:             Second,
            OffsetHour:         OffsetHour,
            OffsetMinute:       OffsetMinute,
            OffsetSecond:       OffsetSecond,
            OffsetDirection:    OffsetDirection.Value,
            Dst:                Dst.Value,
            LongitudeDegree:    LongitudeDegree,
            LongitudeMinute:    LongitudeMinute,
            LongitudeSecond:    LongitudeSecond,
            LongitudeDirection: LongitudeDirection.Value,
            LatitudeDegree:     LatitudeDegree,
            LatitudeMinute:     LatitudeMinute,
            LatitudeSecond:     LatitudeSecond,
            LatitudeDirection:  LatitudeDirection.Value
        );

        var (chart, request) = new RadixInputModel().CalculateWithRequest(inputData, _configContext.ActiveConfig.FactorConfig);

        var editingHoroscope = _chartSession.EditingHoroscope;
        if (editingHoroscope is not null)
        {
            var updatedHoroscope = editingHoroscope with
            {
                Name         = ChartName,
                Notes        = string.IsNullOrWhiteSpace(Description) ? null : Description,
                Source       = string.IsNullOrWhiteSpace(Source) ? null : Source,
                RoddenRating = RoddenRating.Value,
                PlaceName    = string.IsNullOrWhiteSpace(LocationName) ? null : LocationName,
                Latitude     = request.Latitude,
                Longitude    = request.Longitude
            };

            await _horoscopeRepository.UpdateAsync(updatedHoroscope);

            var preferredDateTime = editingHoroscope.DateTimes.FirstOrDefault(dt => dt.IsPreferred)
                                    ?? editingHoroscope.DateTimes.FirstOrDefault();

            if (preferredDateTime is not null)
            {
                var updatedDateTime = preferredDateTime with
                {
                    JulianDate         = request.JulianDay,
                    TimeZoneIdentifier = BuildOffsetString(OffsetDirection.Value, OffsetHour, OffsetMinute, OffsetSecond),
                    OriginalInput      = BuildOriginalInput(enteredYear, Month, Day, Hour, Minute, Second,
                                            OffsetDirection.Value, OffsetHour, OffsetMinute, OffsetSecond)
                };
                await _horoscopeRepository.UpdateDateTimeAsync(updatedDateTime);
            }

            _chartSession.EditingHoroscope = null;
        }

        var editingNamedChart = _chartSession.EditingNamedChart;
        if (editingNamedChart is not null)
        {
            _chartSession.Replace(editingNamedChart, ChartName, chart);
            _chartSession.EditingNamedChart = null;
        }

        _navigationService.NavigateDetail(AppRoutes.RadixOverview);
    }

    private void LoadFromSession()
    {
        var horoscope = _chartSession.EditingHoroscope;
        if (horoscope is null) return;

        ChartName    = horoscope.Name;
        Description  = horoscope.Notes  ?? string.Empty;
        Source       = horoscope.Source ?? string.Empty;
        RoddenRating = RoddenRatingValues.FirstOrDefault(v => v.Value == horoscope.RoddenRating)
                       ?? RoddenRatingValues.First();
        LocationName = horoscope.PlaceName ?? string.Empty;

        if (horoscope.Longitude.HasValue)
        {
            var (deg, min, sec, west) = DecomposeDegrees(horoscope.Longitude.Value);
            LongitudeDegree    = deg;
            LongitudeMinute    = min;
            LongitudeSecond    = sec;
            LongitudeDirection = LongitudeDirectionValues.First(v =>
                v.Value == (west ? LongitudeHemisphere.West : LongitudeHemisphere.East));
        }

        if (horoscope.Latitude.HasValue)
        {
            var (deg, min, sec, south) = DecomposeDegrees(horoscope.Latitude.Value);
            LatitudeDegree    = deg;
            LatitudeMinute    = min;
            LatitudeSecond    = sec;
            LatitudeDirection = LatitudeDirectionValues.First(v =>
                v.Value == (south ? LatitudeHemisphere.South : LatitudeHemisphere.North));
        }

        var preferredDateTime = horoscope.DateTimes.FirstOrDefault(dt => dt.IsPreferred)
                                ?? horoscope.DateTimes.FirstOrDefault();

        if (preferredDateTime?.OriginalInput is { } input)
            ParseOriginalInput(input);
    }

    private void ParseOriginalInput(string originalInput)
    {
        // Format: "YYYY-MM-DD HH:MM:SS ±HH:MM:SS"
        var parts = originalInput.Split(' ');
        if (parts.Length < 3) return;

        var dateParts = parts[0].Split('-');
        var timeParts = parts[1].Split(':');
        var offsetStr = parts[2];

        if (dateParts.Length == 3 &&
            int.TryParse(dateParts[0], out var year) &&
            int.TryParse(dateParts[1], out var month) &&
            int.TryParse(dateParts[2], out var day))
        {
            Year  = year.ToString();
            Month = month;
            Day   = day;
        }

        if (timeParts.Length == 3 &&
            int.TryParse(timeParts[0], out var hour) &&
            int.TryParse(timeParts[1], out var minute) &&
            int.TryParse(timeParts[2], out var second))
        {
            Hour   = hour;
            Minute = minute;
            Second = second;
        }

        if (offsetStr.Length >= 1)
        {
            var sign       = offsetStr[0];
            var offsetDir  = sign == '-' ? UTOffsetDirection.Earlier : UTOffsetDirection.Later;
            OffsetDirection = OffsetDirectionValues.First(v => v.Value == offsetDir);

            var offsetParts = offsetStr.Substring(1).Split(':');
            if (offsetParts.Length == 3 &&
                int.TryParse(offsetParts[0], out var oh) &&
                int.TryParse(offsetParts[1], out var om) &&
                int.TryParse(offsetParts[2], out var os))
            {
                OffsetHour   = oh;
                OffsetMinute = om;
                OffsetSecond = os;
            }
        }
    }

    private static (int deg, int min, int sec, bool negative) DecomposeDegrees(double value)
    {
        var negative = value < 0;
        var abs      = Math.Abs(value);
        var deg      = (int)abs;
        var minRaw   = (abs - deg) * 60.0;
        var min      = (int)minRaw;
        var sec      = (int)Math.Round((minRaw - min) * 60.0);
        if (sec == 60) { sec = 0; min++; }
        if (min == 60) { min = 0; deg++; }
        return (deg, min, sec, negative);
    }

    private static string BuildOffsetString(UTOffsetDirection direction, int hours, int minutes, int seconds)
    {
        var sign = direction == UTOffsetDirection.Later ? "+" : "-";
        return $"{sign}{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    private static string BuildOriginalInput(int year, int month, int day, int hour, int minute, int second,
        UTOffsetDirection direction, int offsetH, int offsetM, int offsetS)
    {
        var offset = BuildOffsetString(direction, offsetH, offsetM, offsetS);
        return $"{year:D4}-{month:D2}-{day:D2} {hour:D2}:{minute:D2}:{second:D2} {offset}";
    }
}
