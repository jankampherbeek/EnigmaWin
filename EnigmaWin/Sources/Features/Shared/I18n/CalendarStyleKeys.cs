using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Shared.I18n;

/// <summary>Localization keys for CalendarStyle enum values.</summary>
public static class CalendarStyleKeys
{
    private static readonly Dictionary<CalendarStyle, string> Keys = new()
    {
        [CalendarStyle.Gregorian] = "enum.calendarstyle.gregorian",
        [CalendarStyle.Julian]    = "enum.calendarstyle.julian",
    };

    public static string Key(CalendarStyle style) => Keys.GetValueOrDefault(style, "");
}
