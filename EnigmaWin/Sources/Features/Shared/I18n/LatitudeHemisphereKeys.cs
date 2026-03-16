using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Shared.I18n;

/// <summary>Localization keys for LatitudeHemisphere enum values.</summary>
public static class LatitudeHemisphereKeys
{
    private static readonly Dictionary<LatitudeHemisphere, string> Keys = new()
    {
        [LatitudeHemisphere.North] = "enum.latitudehemisphere.north",
        [LatitudeHemisphere.South] = "enum.latitudehemisphere.south",
    };

    public static string Key(LatitudeHemisphere hemisphere) => Keys.GetValueOrDefault(hemisphere, "");
}
