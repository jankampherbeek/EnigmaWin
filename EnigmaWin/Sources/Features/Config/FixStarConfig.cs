// FixStarConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars;

namespace EnigmaWin.Sources.Features.Config;

public enum FixStarSelections { Ptolemy, Robson, Brady, Magnitude, SelfDefined }

public record FixStarSetting(StarDefinitions FixStar, bool IsUsed);

public record FixStarConfig(
    bool IncludeGalacticCenter,
    double FixStarOrb,
    double ParanTimeOrb,
    FixStarSelections ActiveSelection,
    int MagnitudeLimit,
    IReadOnlyList<FixStarSetting> FixStarSettings)
{
    public static FixStarConfig Default => new(
        IncludeGalacticCenter: false,
        FixStarOrb:            1.0,
        ParanTimeOrb:          10.0,
        ActiveSelection:       FixStarSelections.Magnitude,
        MagnitudeLimit:        4,
        FixStarSettings:       StarDefinitions.AllCases
            .Select(s => new FixStarSetting(s, false))
            .ToList());
}
