using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Shared.I18n;

/// <summary>Localization keys for DSTOption enum values.</summary>
public static class DSTOptionKeys
{
    private static readonly Dictionary<DSTOption, string> Keys = new()
    {
        [DSTOption.NoDST] = "enum.dstoption.nodst",
        [DSTOption.DST]   = "enum.dstoption.dst",
    };

    public static string Key(DSTOption option) => Keys.GetValueOrDefault(option, "");
}
