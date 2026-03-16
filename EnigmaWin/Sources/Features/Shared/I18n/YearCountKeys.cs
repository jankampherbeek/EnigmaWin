using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Shared.I18n;

/// <summary>Localization keys for YearCount enum values.</summary>
public static class YearCountKeys
{
    private static readonly Dictionary<YearCount, string> Keys = new()
    {
        [YearCount.CE]           = "enum.yearcount.ce",
        [YearCount.BCE]          = "enum.yearcount.bce",
        [YearCount.Astronomical] = "enum.yearcount.astronomical",
    };

    public static string Key(YearCount count) => Keys.GetValueOrDefault(count, "");
}
