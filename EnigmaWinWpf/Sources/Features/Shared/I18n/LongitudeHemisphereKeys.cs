// LongitudeHemisphereKeys.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Shared.I18n;

/// <summary>Localization keys for LongitudeHemisphere enum values.</summary>
public static class LongitudeHemisphereKeys
{
    private static readonly Dictionary<LongitudeHemisphere, string> Keys = new()
    {
        [LongitudeHemisphere.East] = "enum.longitudehemisphere.east",
        [LongitudeHemisphere.West] = "enum.longitudehemisphere.west",
    };

    public static string Key(LongitudeHemisphere hemisphere) => Keys.GetValueOrDefault(hemisphere, "");
}
