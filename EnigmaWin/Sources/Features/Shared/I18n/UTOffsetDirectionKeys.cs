using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Shared.I18n;

/// <summary>Localization keys for UTOffsetDirection enum values.</summary>
public static class UTOffsetDirectionKeys
{
    private static readonly Dictionary<UTOffsetDirection, string> Keys = new()
    {
        [UTOffsetDirection.Later]   = "enum.utoffsetdirection.later",
        [UTOffsetDirection.Earlier] = "enum.utoffsetdirection.earlier",
    };

    public static string Key(UTOffsetDirection direction) => Keys.GetValueOrDefault(direction, "");
}
