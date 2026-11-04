// AnalysisGroup.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.AppShell.Navigation;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis;

// Single definition of the analysis items and their grouping.
// Used by RadixAnalysisScreen (center pane) and by the Analysis menu in the menu bar, so both stay identical.
// Mirrors AnalysisGroup in the Apple version.

public enum AnalysisGroup
{
    General,
    Harmonics,
    Zodiac,
    Schemas,
    Stars
}

public enum AnalysisItem
{
    Aspects,
    Midpoints,
    Declinations,
    Lots,
    Harmonics,
    HarmonicExactAspects,
    HarmonicOrbs,
    ZodiacDivisions,
    AltZodiacStart,
    Enneagram,
    Vsp,
    BlaSchema,
    Countings,
    FixStars,
    Parans
}

public static class AnalysisCatalog
{
    public static IReadOnlyList<AnalysisGroup> Groups { get; } =
    [
        AnalysisGroup.General, AnalysisGroup.Harmonics, AnalysisGroup.Zodiac, AnalysisGroup.Schemas, AnalysisGroup.Stars
    ];

    public static IReadOnlyList<AnalysisItem> ItemsOf(AnalysisGroup group) => group switch
    {
        AnalysisGroup.General   => [AnalysisItem.Aspects, AnalysisItem.Midpoints, AnalysisItem.Declinations, AnalysisItem.Lots],
        AnalysisGroup.Harmonics => [AnalysisItem.Harmonics, AnalysisItem.HarmonicExactAspects, AnalysisItem.HarmonicOrbs],
        AnalysisGroup.Zodiac    => [AnalysisItem.ZodiacDivisions, AnalysisItem.AltZodiacStart],
        AnalysisGroup.Schemas   => [AnalysisItem.Enneagram, AnalysisItem.Vsp, AnalysisItem.BlaSchema, AnalysisItem.Countings],
        AnalysisGroup.Stars     => [AnalysisItem.FixStars, AnalysisItem.Parans],
        _                       => []
    };

    /// <summary>Key in RadixAnalysis.strings for the title of the group.</summary>
    public static string RbKey(AnalysisGroup group) => group switch
    {
        AnalysisGroup.General   => "analysis.group.general",
        AnalysisGroup.Harmonics => "analysis.group.harmonics",
        AnalysisGroup.Zodiac    => "analysis.group.zodiac",
        AnalysisGroup.Schemas   => "analysis.group.schemas",
        AnalysisGroup.Stars     => "analysis.group.stars",
        _                       => ""
    };

    /// <summary>Key in RadixAnalysis.strings for the name of the analysis.</summary>
    public static string RbKey(AnalysisItem item) => item switch
    {
        AnalysisItem.Aspects              => "analysis.btn.aspects",
        AnalysisItem.Midpoints            => "analysis.btn.midpoints",
        AnalysisItem.Declinations         => "analysis.btn.declinations",
        AnalysisItem.Lots                 => "analysis.btn.lots",
        AnalysisItem.Harmonics            => "analysis.btn.harmonics",
        AnalysisItem.HarmonicExactAspects => "analysis.btn.harmonicexactaspects",
        AnalysisItem.HarmonicOrbs         => "analysis.btn.harmonicorbs",
        AnalysisItem.ZodiacDivisions      => "analysis.btn.zodiacdivisions",
        AnalysisItem.AltZodiacStart       => "analysis.btn.altzodiacstart",
        AnalysisItem.Enneagram            => "analysis.btn.enneagram",
        AnalysisItem.Vsp                  => "analysis.btn.vsp",
        AnalysisItem.BlaSchema            => "analysis.btn.blaschema",
        AnalysisItem.Countings            => "analysis.btn.countings",
        AnalysisItem.FixStars             => "analysis.btn.fixstars",
        AnalysisItem.Parans               => "analysis.btn.parans",
        _                                 => ""
    };

    /// <summary>Routes for the center (main) and right (detail) pane of an analysis.
    /// Analyses without input show the chart wheel in the center pane.</summary>
    public static (string Main, string Detail) RoutesOf(AnalysisItem item) => item switch
    {
        AnalysisItem.Aspects              => (AppRoutes.RadixChart, AppRoutes.RadixAspects),
        AnalysisItem.Midpoints            => (AppRoutes.RadixChart, AppRoutes.RadixMidpoints),
        AnalysisItem.Declinations         => (AppRoutes.RadixChart, AppRoutes.RadixDeclinations),
        AnalysisItem.Lots                 => (AppRoutes.RadixLotsInput, AppRoutes.RadixLots),
        AnalysisItem.Harmonics            => (AppRoutes.RadixChart, AppRoutes.RadixHarmonics),
        AnalysisItem.HarmonicExactAspects => (AppRoutes.RadixChart, AppRoutes.RadixHarmonicExactAspects),
        AnalysisItem.HarmonicOrbs         => (AppRoutes.RadixHarmonicOrbsInput, AppRoutes.RadixHarmonicOrbs),
        AnalysisItem.ZodiacDivisions      => (AppRoutes.RadixZodiacDivisionsInput, AppRoutes.RadixZodiacDivisions),
        AnalysisItem.AltZodiacStart       => (AppRoutes.RadixAltZodiacStartInput, AppRoutes.RadixAltZodiacStart),
        AnalysisItem.Enneagram            => (AppRoutes.RadixEnneagramOptions, AppRoutes.RadixEnneagram),
        AnalysisItem.Vsp                  => (AppRoutes.RadixVsp, AppRoutes.RadixVspDetail),
        AnalysisItem.BlaSchema            => (AppRoutes.RadixChart, AppRoutes.RadixBlaSchema),
        AnalysisItem.Countings            => (AppRoutes.RadixChart, AppRoutes.RadixCountings),
        AnalysisItem.FixStars             => (AppRoutes.RadixFixStarsInput, AppRoutes.RadixFixStars),
        AnalysisItem.Parans               => (AppRoutes.RadixParansInput, AppRoutes.RadixParans),
        _                                 => (AppRoutes.RadixChart, AppRoutes.None)
    };

    /// <summary>Opens an analysis: its input (or the chart) in the center pane, the result in the right pane.</summary>
    public static void Open(INavigationService navigationService, AnalysisItem item)
    {
        var (main, detail) = RoutesOf(item);
        navigationService.NavigateMain(main);
        navigationService.NavigateDetail(detail);
    }

    private static readonly HashSet<string> DetailRoutes =
        System.Enum.GetValues<AnalysisItem>().Select(i => RoutesOf(i).Detail).ToHashSet();

    /// <summary>True when the route is the result pane of an analysis, i.e. an analysis was opened from the overview.</summary>
    public static bool IsAnalysisDetailRoute(string route) => DetailRoutes.Contains(route);
}
