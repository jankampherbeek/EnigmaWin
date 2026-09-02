// ProgressiveCalendarRangeLimiter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar;

/// <summary>
/// Computes the maximum allowed date-range span for a Progressive Calendar scan, based on which
/// techniques and factors are currently selected. Scanning is far more expensive for fast-moving
/// factors (a transiting Moon) over a long range than for slow ones, so the limit tightens as
/// fast factors are added to the selection.
/// </summary>
/// <remarks>
/// When more than one rule matches the current selection, the shortest limit always wins. This
/// does not enforce the limit — callers (the input screen / view model) are responsible for
/// clamping the requested end date against <see cref="MaxRangeInDays"/>.
/// </remarks>
public static class ProgressiveCalendarRangeLimiter
{
    private const double TropicalYearInDays = 365.242199074;

    /// <summary>Applies when no rule below matches the current selection.</summary>
    public const double DefaultLimitInDays = 125.0 * TropicalYearInDays;

    private static readonly HashSet<Factors> MercuryVenusMars = [Factors.Mercury, Factors.Venus, Factors.Mars];
    private static readonly HashSet<Factors> JupiterSaturn = [Factors.Jupiter, Factors.Saturn];

    /// <param name="selections">the techniques and factors currently selected for the scan.</param>
    /// <returns>the maximum allowed span, in days, between the scan's start and end date.</returns>
    public static double MaxRangeInDays(IReadOnlyList<ProgressiveCalendarSelection> selections)
    {
        var limits = new List<double>();

        foreach (var selection in selections)
        {
            switch (selection.Technique)
            {
                case ProgressiveCalendarTechnique.Transit:
                    if (selection.Factors.Contains(Factors.Moon))
                        limits.Add(60.0);
                    if (selection.Factors.Any(MercuryVenusMars.Contains))
                        limits.Add(2.0 * TropicalYearInDays);
                    if (selection.Factors.Any(JupiterSaturn.Contains))
                        limits.Add(40.0 * TropicalYearInDays);
                    break;
                case ProgressiveCalendarTechnique.SecondaryDirection:
                    if (selection.Factors.Contains(Factors.Moon))
                        limits.Add(60.0 * TropicalYearInDays);
                    break;
                case ProgressiveCalendarTechnique.SymbolicDirection:
                    break;
            }
        }

        return limits.Count > 0 ? limits.Min() : DefaultLimitInDays;
    }
}
