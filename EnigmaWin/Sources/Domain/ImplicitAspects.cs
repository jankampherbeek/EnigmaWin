// ImplicitAspects.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Domain;

/// <summary>
/// Identifies factor pairs with a fixed mutual distance. Aspects between these factors are implicit
/// and should be omitted when calculating aspects within a single chart.
/// </summary>
public static class ImplicitAspects
{
    /// <summary>True if the distance between both factors is fixed, so any aspect between them is implicit.</summary>
    public static bool IsImplicit(Factors f1, Factors f2)
    {
        if (f1 == f2) return false;
        if (IsNodeRelated(f1) && IsNodeRelated(f2)) return true;
        return IsPair(f1, f2, Factors.ApogeeMean, Factors.Priapus)
               || IsPair(f1, f2, Factors.ApogeeKoch, Factors.PriapusKoch)
               || IsPair(f1, f2, Factors.ApogeeDuval, Factors.PriapusDuval)
               || IsPair(f1, f2, Factors.ApogeeInterpolated, Factors.PriapusInterpolated)
               || IsPair(f1, f2, Factors.BlackSun, Factors.Diamond);
    }

    private static bool IsNodeRelated(Factors factor) => factor is
        Factors.NorthNodeMean or Factors.NorthNodeTrue or
        Factors.SouthNodeMean or Factors.SouthNodeTrue or
        Factors.Dragon or Factors.Beast;

    private static bool IsPair(Factors f1, Factors f2, Factors a, Factors b) =>
        (f1 == a && f2 == b) || (f1 == b && f2 == a);
}
