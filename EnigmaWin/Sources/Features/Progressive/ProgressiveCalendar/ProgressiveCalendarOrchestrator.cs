// ProgressiveCalendarOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar;

/// <summary>One technique to include in a Progressive Calendar scan, with its own factor list.</summary>
/// <param name="SymbolicKey">Required, and only meaningful, for <see cref="ProgressiveCalendarTechnique.SymbolicDirection"/>.</param>
public sealed record ProgressiveCalendarSelection(
    ProgressiveCalendarTechnique Technique,
    IReadOnlyList<Factors> Factors,
    SymbolicKeys? SymbolicKey = null);

/// <summary>Which event and episode kinds to include in a Progressive Calendar scan.</summary>
public sealed class ProgressiveCalendarEventKindToggles
{
    public bool AspectsToRadix = true;
    public bool ParallelsToRadix = true;
    public bool AspectsProgToProg = true;
    public bool ParallelsProgToProg = true;
    public bool CuspConjunctions = true;
    public bool RetrogradeDirectStations = true;
    public bool OobEnterExit = true;
    public bool DeclinationExtremes = true;
}

public sealed record ProgressiveCalendarResult(
    IReadOnlyList<ProgressiveCalendarEvent> Events,
    IReadOnlyList<ProgressiveOrbEpisode> Episodes);

/// <summary>
/// Finds Progressive Calendar events and orb episodes over a date range, for the transit,
/// secondary-direction and symbolic-direction techniques. Primary directions are out of
/// scope for the Progressive Calendar.
/// </summary>
/// <remarks>
/// This orchestrator does not enforce the date-range performance limit — see
/// <see cref="ProgressiveCalendarRangeLimiter"/> — that is the caller's responsibility, applied
/// before <see cref="FindEvents"/> is invoked.
///
/// Symbolic directions are longitude-only in this app (mirroring <see cref="ProgressiveOrchestrator"/>,
/// which sets their declination to 0) and their arc always increases, so for
/// <see cref="ProgressiveCalendarTechnique.SymbolicDirection"/> this orchestrator only produces
/// longitude-based results: aspects to radix, aspects between symbolic factors, and cusp
/// conjunctions. Declination-based event kinds (zero/max declination, OOB enter/exit) and
/// parallels never fire for symbolic directions, and symbolic directions never produce
/// retrograde/direct stations.
/// </remarks>
public sealed class ProgressiveCalendarOrchestrator
{
    private readonly double _natalJD;
    private readonly FullChart _radixChart;

    private const double TropicalYearInDays = 365.242199074;
    private const double ZeroCrossingTolerance = 1e-6;
    /// <summary>Small backward probe, in days, used to read the sign of a quantity just before a
    /// detected zero crossing (e.g. to classify a station as retrograde vs. direct).</summary>
    private const double DirectionProbe = 1e-4;
    /// <summary>Threshold used to reject spurious sign changes caused by circular wraparound
    /// (e.g. an angular deviation jumping from +179° to −179°), mirroring PreNatalOrchestrator.</summary>
    private const double WraparoundGuardSum = 90.0;

    // SE flags
    private const int SeFlagsSwieph = 2;
    private const int SeFlagsSpeed = 256;
    private const int SeFlagsEquatorial = 2048;
    private const int SeEclNut = -1;

    public ProgressiveCalendarOrchestrator(double natalJD, FullChart radixChart)
    {
        _natalJD = natalJD;
        _radixChart = radixChart;
    }

    // MARK: - Public API

    /// <summary>Scans [startJD, endJD] for Progressive Calendar events and orb episodes.</summary>
    /// <param name="selections">one entry per technique to include, each with its own factor list.</param>
    /// <param name="radixFactors">radix factors to compare progressive factors against.</param>
    /// <param name="aspects">aspect angles considered for both radix and prog-to-prog aspects.</param>
    /// <param name="aspectOrb">maximum orb, in degrees, for aspect episodes.</param>
    /// <param name="parallelOrb">maximum orb, in degrees, for parallel/contra-parallel episodes.</param>
    /// <param name="cuspOrb">currently unused — cusp conjunctions are found as exact crossings, not
    /// orb episodes. Reserved for a future orb-window variant.</param>
    public ProgressiveCalendarResult FindEvents(
        double startJD, double endJD,
        IReadOnlyList<ProgressiveCalendarSelection> selections,
        IReadOnlyList<Factors> radixFactors,
        IReadOnlyList<Aspects> aspects,
        double aspectOrb, double parallelOrb, double cuspOrb,
        ProgressiveCalendarEventKindToggles toggles)
    {
        var events = new List<ProgressiveCalendarEvent>();
        var episodes = new List<ProgressiveOrbEpisode>();

        foreach (var selection in selections)
        {
            var provider = TechniqueProviderFor(selection);
            var step = StepSizeInDays(selection.Technique, selection.Factors);

            if (toggles.RetrogradeDirectStations && provider.SupportsSpeed)
                events.AddRange(FindStations(selection.Technique, selection.Factors, startJD, endJD, step, provider));

            if (toggles.DeclinationExtremes && provider.SupportsDeclination)
            {
                events.AddRange(FindZeroDeclinations(selection.Technique, selection.Factors, startJD, endJD, step, provider));
                events.AddRange(FindDeclinationExtremes(selection.Technique, selection.Factors, startJD, endJD, step, provider));
            }
            if (toggles.OobEnterExit && provider.SupportsDeclination)
                events.AddRange(FindOobTransitions(selection.Technique, selection.Factors, startJD, endJD, step, provider));

            if (toggles.CuspConjunctions)
                events.AddRange(FindCuspConjunctions(selection.Technique, selection.Factors, startJD, endJD, step, provider));

            if (toggles.AspectsToRadix)
                episodes.AddRange(FindAspectEpisodesToRadix(
                    selection.Technique, selection.Factors, radixFactors, aspects, startJD, endJD, step, provider, aspectOrb));

            if (toggles.ParallelsToRadix && provider.SupportsDeclination)
                episodes.AddRange(FindParallelEpisodesToRadix(
                    selection.Technique, selection.Factors, radixFactors, startJD, endJD, step, provider, parallelOrb));

            if (toggles.AspectsProgToProg)
                episodes.AddRange(FindAspectEpisodesProgToProg(
                    selection.Technique, selection.Factors, aspects, startJD, endJD, step, provider, aspectOrb));

            if (toggles.ParallelsProgToProg && provider.SupportsDeclination)
                episodes.AddRange(FindParallelEpisodesProgToProg(
                    selection.Technique, selection.Factors, startJD, endJD, step, provider, parallelOrb));
        }

        events.Sort((a, b) => a.JulianDay.CompareTo(b.JulianDay));
        episodes.Sort((a, b) => (a.EnterJD ?? a.ExactJD).CompareTo(b.EnterJD ?? b.ExactJD));

        return new ProgressiveCalendarResult(events, episodes);
    }

    // MARK: - Position providers

    /// <summary>Supplies longitude/declination (and, where supported, their speeds) for a
    /// progressive factor at a given real-world Julian Day, for one technique.</summary>
    private sealed record TechniqueProvider(
        bool SupportsSpeed,
        bool SupportsDeclination,
        Func<Factors, double, double> Longitude,
        Func<Factors, double, double> LongitudeSpeed,
        Func<Factors, double, double> Declination,
        Func<Factors, double, double> DeclinationSpeed);

    private TechniqueProvider TechniqueProviderFor(ProgressiveCalendarSelection selection) => selection.Technique switch
    {
        ProgressiveCalendarTechnique.Transit =>
            EphemerisProvider(realJD => realJD),
        ProgressiveCalendarTechnique.SecondaryDirection =>
            EphemerisProvider(realJD => _natalJD + (realJD - _natalJD) / TropicalYearInDays),
        ProgressiveCalendarTechnique.SymbolicDirection =>
            SymbolicProvider(selection.SymbolicKey ?? SymbolicKeys.OneDegree),
        _ => EphemerisProvider(realJD => realJD)
    };

    /// <summary>Shared provider for transits and secondary directions: both reduce to evaluating
    /// the real ephemeris at some internal Julian Day derived from the real-world Julian Day.</summary>
    private TechniqueProvider EphemerisProvider(Func<double, double> internalJD) => new(
        SupportsSpeed: true,
        SupportsDeclination: true,
        Longitude: (factor, realJD) => CalcLongitude(factor, internalJD(realJD)),
        LongitudeSpeed: (factor, realJD) => CalcLongitudeSpeed(factor, internalJD(realJD)),
        Declination: (factor, realJD) => CalcDeclination(factor, internalJD(realJD)),
        DeclinationSpeed: (factor, realJD) => CalcDeclinationSpeed(factor, internalJD(realJD)));

    /// <summary>Symbolic directions: longitude = natal longitude + arc(realJD). No speed or
    /// declination is modeled, matching <see cref="ProgressiveOrchestrator"/>.</summary>
    private TechniqueProvider SymbolicProvider(SymbolicKeys symbolicKey)
    {
        double NatalLongitude(Factors factor) => CalcLongitude(factor, _natalJD);
        var sunNatalLongitude = NatalLongitude(Factors.Sun);

        double Arc(double realJD)
        {
            var oneDegreeArc = (realJD - _natalJD) / TropicalYearInDays;
            switch (symbolicKey)
            {
                case SymbolicKeys.OneDegree:
                    return oneDegreeArc;
                case SymbolicKeys.MeanSun:
                    return (360.0 / TropicalYearInDays) * oneDegreeArc;
                case SymbolicKeys.TrueSun:
                default:
                {
                    var sunAtDirected = CalcLongitude(Factors.Sun, _natalJD + oneDegreeArc);
                    var diff = (sunAtDirected - sunNatalLongitude) % 360.0;
                    if (diff < 0) diff += 360.0;
                    return diff;
                }
            }
        }

        return new TechniqueProvider(
            SupportsSpeed: false,
            SupportsDeclination: false,
            Longitude: (factor, realJD) =>
            {
                var lon = (NatalLongitude(factor) + Arc(realJD)) % 360.0;
                if (lon < 0) lon += 360.0;
                return lon;
            },
            LongitudeSpeed: (_, _) => 0.0,
            Declination: (_, _) => 0.0,
            DeclinationSpeed: (_, _) => 0.0);
    }

    // MARK: - Instantaneous events: stations

    private List<ProgressiveCalendarEvent> FindStations(
        ProgressiveCalendarTechnique technique, IReadOnlyList<Factors> factors,
        double startJD, double endJD, double stepSize, TechniqueProvider provider)
    {
        var result = new List<ProgressiveCalendarEvent>();
        foreach (var factor in factors)
        {
            var crossings = ScanZeroCrossings(startJD, endJD, stepSize, ZeroCrossingTolerance,
                jd => provider.LongitudeSpeed(factor, jd));
            foreach (var jd in crossings)
            {
                var speedBefore = provider.LongitudeSpeed(factor, Math.Max(startJD, jd - DirectionProbe));
                ProgressiveCalendarEventKind kind = speedBefore > 0
                    ? new RetrogradeStationKind(factor)
                    : new DirectStationKind(factor);
                result.Add(MakeEvent(technique, jd, kind, provider.Longitude(factor, jd)));
            }
        }
        return result;
    }

    // MARK: - Instantaneous events: declination

    private List<ProgressiveCalendarEvent> FindZeroDeclinations(
        ProgressiveCalendarTechnique technique, IReadOnlyList<Factors> factors,
        double startJD, double endJD, double stepSize, TechniqueProvider provider)
    {
        var result = new List<ProgressiveCalendarEvent>();
        foreach (var factor in factors)
        {
            var crossings = ScanZeroCrossings(startJD, endJD, stepSize, ZeroCrossingTolerance,
                jd => provider.Declination(factor, jd));
            foreach (var jd in crossings)
                result.Add(MakeEvent(technique, jd, new ZeroDeclinationKind(factor), provider.Longitude(factor, jd)));
        }
        return result;
    }

    /// <summary>A declination extreme (maximum north or south) is a moment where the derivative
    /// of declination — declination speed — crosses zero.</summary>
    private List<ProgressiveCalendarEvent> FindDeclinationExtremes(
        ProgressiveCalendarTechnique technique, IReadOnlyList<Factors> factors,
        double startJD, double endJD, double stepSize, TechniqueProvider provider)
    {
        var result = new List<ProgressiveCalendarEvent>();
        foreach (var factor in factors)
        {
            var crossings = ScanZeroCrossings(startJD, endJD, stepSize, ZeroCrossingTolerance,
                jd => provider.DeclinationSpeed(factor, jd));
            foreach (var jd in crossings)
            {
                var isNorthern = provider.Declination(factor, jd) >= 0;
                result.Add(MakeEvent(technique, jd, new MaxDeclinationKind(factor, isNorthern), provider.Longitude(factor, jd)));
            }
        }
        return result;
    }

    private List<ProgressiveCalendarEvent> FindOobTransitions(
        ProgressiveCalendarTechnique technique, IReadOnlyList<Factors> factors,
        double startJD, double endJD, double stepSize, TechniqueProvider provider)
    {
        double OobDeviation(Factors factor, double jd) => Math.Abs(provider.Declination(factor, jd)) - Obliquity(jd);

        var result = new List<ProgressiveCalendarEvent>();
        foreach (var factor in factors)
        {
            var crossings = ScanZeroCrossings(startJD, endJD, stepSize, ZeroCrossingTolerance,
                jd => OobDeviation(factor, jd));
            foreach (var jd in crossings)
            {
                var wasOobBefore = OobDeviation(factor, Math.Max(startJD, jd - DirectionProbe)) > 0;
                ProgressiveCalendarEventKind kind = wasOobBefore ? new OobExitKind(factor) : new OobEnterKind(factor);
                result.Add(MakeEvent(technique, jd, kind, provider.Longitude(factor, jd)));
            }
        }
        return result;
    }

    // MARK: - Instantaneous events: cusp conjunctions

    private List<ProgressiveCalendarEvent> FindCuspConjunctions(
        ProgressiveCalendarTechnique technique, IReadOnlyList<Factors> factors,
        double startJD, double endJD, double stepSize, TechniqueProvider provider)
    {
        var targets = CuspTargets();
        var result = new List<ProgressiveCalendarEvent>();
        foreach (var factor in factors)
        foreach (var (target, cuspLongitude) in targets)
        {
            var crossings = ScanZeroCrossings(startJD, endJD, stepSize, ZeroCrossingTolerance,
                jd => SignedDistance(provider.Longitude(factor, jd), cuspLongitude),
                (a, b) => Math.Abs(a) + Math.Abs(b) < WraparoundGuardSum);
            foreach (var jd in crossings)
                result.Add(MakeEvent(technique, jd, new CuspConjunctionKind(factor, target), provider.Longitude(factor, jd)));
        }
        return result;
    }

    private List<(ProgressiveCuspTarget Target, double Longitude)> CuspTargets()
    {
        var targets = new List<(ProgressiveCuspTarget, double)>();
        var cusps = _radixChart.HousePositions.Cusps;
        for (var i = 0; i < Math.Min(12, cusps.Length); i++)
            targets.Add((new HouseCuspTarget(i + 1), cusps[i].Longitude));
        targets.Add((new AscendantCuspTarget(), _radixChart.HousePositions.Ascendant.Longitude));
        targets.Add((new MidheavenCuspTarget(), _radixChart.HousePositions.Midheaven.Longitude));
        targets.Add((new EastpointCuspTarget(), _radixChart.HousePositions.Eastpoint.Longitude));
        targets.Add((new VertexCuspTarget(), _radixChart.HousePositions.Vertex.Longitude));
        return targets;
    }

    // MARK: - Orb episodes: aspects & parallels to radix

    private List<ProgressiveOrbEpisode> FindAspectEpisodesToRadix(
        ProgressiveCalendarTechnique technique, IReadOnlyList<Factors> factors, IReadOnlyList<Factors> radixFactors,
        IReadOnlyList<Aspects> aspects, double startJD, double endJD, double stepSize, TechniqueProvider provider, double maxOrb)
    {
        var radixLongitudes = RadixLongitudes(radixFactors);
        var episodes = new List<ProgressiveOrbEpisode>();
        foreach (var factor in factors)
        foreach (var (radixFactor, radixLon) in radixLongitudes)
        foreach (var aspect in aspects)
        {
            double Deviation(double jd) => SignedAspectDeviation(provider.Longitude(factor, jd), radixLon, aspect.Angle());
            episodes.AddRange(FindOrbEpisodes(
                technique, factor, radixFactor, new AspectToRadixKind(aspect), Deviation, startJD, endJD, stepSize, maxOrb));
        }
        return episodes;
    }

    private List<ProgressiveOrbEpisode> FindParallelEpisodesToRadix(
        ProgressiveCalendarTechnique technique, IReadOnlyList<Factors> factors, IReadOnlyList<Factors> radixFactors,
        double startJD, double endJD, double stepSize, TechniqueProvider provider, double maxOrb)
    {
        var radixDeclinations = RadixDeclinations(radixFactors);
        var episodes = new List<ProgressiveOrbEpisode>();
        foreach (var factor in factors)
        foreach (var (radixFactor, radixDecl) in radixDeclinations)
        {
            double Deviation(double jd) => Math.Abs(provider.Declination(factor, jd)) - Math.Abs(radixDecl);
            var raw = FindOrbEpisodes(
                technique, factor, radixFactor, new ParallelToRadixKind(), Deviation, startJD, endJD, stepSize, maxOrb);
            foreach (var episode in raw)
            {
                var isContra = (provider.Declination(factor, episode.ExactJD) >= 0) != (radixDecl >= 0);
                episodes.Add(isContra ? episode with { Kind = new ContraParallelToRadixKind() } : episode);
            }
        }
        return episodes;
    }

    // MARK: - Orb episodes: aspects & parallels between progressive factors (within technique)

    private List<ProgressiveOrbEpisode> FindAspectEpisodesProgToProg(
        ProgressiveCalendarTechnique technique, IReadOnlyList<Factors> factors, IReadOnlyList<Aspects> aspects,
        double startJD, double endJD, double stepSize, TechniqueProvider provider, double maxOrb)
    {
        var episodes = new List<ProgressiveOrbEpisode>();
        if (factors.Count <= 1) return episodes;

        for (var i = 0; i < factors.Count; i++)
        for (var j = i + 1; j < factors.Count; j++)
        {
            var f1 = factors[i]; var f2 = factors[j];
            foreach (var aspect in aspects)
            {
                double Deviation(double jd) => SignedAspectDeviation(provider.Longitude(f1, jd), provider.Longitude(f2, jd), aspect.Angle());
                episodes.AddRange(FindOrbEpisodes(
                    technique, f1, f2, new AspectProgToProgKind(aspect), Deviation, startJD, endJD, stepSize, maxOrb));
            }
        }
        return episodes;
    }

    private List<ProgressiveOrbEpisode> FindParallelEpisodesProgToProg(
        ProgressiveCalendarTechnique technique, IReadOnlyList<Factors> factors,
        double startJD, double endJD, double stepSize, TechniqueProvider provider, double maxOrb)
    {
        var episodes = new List<ProgressiveOrbEpisode>();
        if (factors.Count <= 1) return episodes;

        for (var i = 0; i < factors.Count; i++)
        for (var j = i + 1; j < factors.Count; j++)
        {
            var f1 = factors[i]; var f2 = factors[j];
            double Deviation(double jd) => Math.Abs(provider.Declination(f1, jd)) - Math.Abs(provider.Declination(f2, jd));
            var raw = FindOrbEpisodes(
                technique, f1, f2, new ParallelProgToProgKind(), Deviation, startJD, endJD, stepSize, maxOrb);
            foreach (var episode in raw)
            {
                var isContra = (provider.Declination(f1, episode.ExactJD) >= 0) != (provider.Declination(f2, episode.ExactJD) >= 0);
                episodes.Add(isContra ? episode with { Kind = new ContraParallelProgToProgKind() } : episode);
            }
        }
        return episodes;
    }

    // MARK: - Generic orb episode builder

    /// <summary>Builds orb episodes from a signed <paramref name="deviation"/> function of Julian
    /// Day: the episode is in orb while |deviation| &lt;= maxOrb. Reused for aspects and parallels
    /// alike — only <paramref name="deviation"/> differs between the two.</summary>
    /// <remarks>
    /// If the episode's interior never reaches an exact/exact-magnitude crossing (deviation never
    /// hits exactly 0 — e.g. a station reverses the approach before it gets there), the closer of
    /// the episode's two boundary moments is used as an approximate exactJD instead of solving for
    /// the true closest approach. This is a deliberate simplification.
    /// </remarks>
    private List<ProgressiveOrbEpisode> FindOrbEpisodes(
        ProgressiveCalendarTechnique technique, Factors factor1, Factors factor2,
        ProgressiveOrbEpisodeKind kind, Func<double, double> deviation,
        double startJD, double endJD, double stepSize, double maxOrb)
    {
        bool GuardFn(double a, double b) => Math.Abs(a) + Math.Abs(b) < WraparoundGuardSum;

        var exactCrossings = ScanZeroCrossings(startJD, endJD, stepSize, ZeroCrossingTolerance, deviation, GuardFn);
        var boundaryCrossings = ScanZeroCrossings(startJD, endJD, stepSize, ZeroCrossingTolerance,
            jd => Math.Abs(deviation(jd)) - maxOrb, GuardFn);

        var startsInOrb = Math.Abs(deviation(startJD)) <= maxOrb;
        if (boundaryCrossings.Count == 0 && !startsInOrb) return [];

        var episodes = new List<ProgressiveOrbEpisode>();
        var hasOpenEpisode = startsInOrb;
        double? pendingEnter = null;

        void CloseEpisode(double? enterJD, double? exitJD)
        {
            var lo = enterJD ?? startJD;
            var hi = exitJD ?? endJD;
            var genuineExact = exactCrossings.Where(jd => jd >= lo && jd <= hi).Cast<double?>().FirstOrDefault();
            var exact = genuineExact ?? ClosestApproachJD(lo, hi, stepSize, deviation);
            episodes.Add(new ProgressiveOrbEpisode(
                technique, factor1, factor2, kind, enterJD, exact, exitJD,
                Math.Abs(deviation(exact)), maxOrb, genuineExact is not null));
        }

        foreach (var crossingJD in boundaryCrossings)
        {
            if (hasOpenEpisode)
            {
                CloseEpisode(pendingEnter, crossingJD);
                hasOpenEpisode = false;
                pendingEnter = null;
            }
            else
            {
                pendingEnter = crossingJD;
                hasOpenEpisode = true;
            }
        }
        if (hasOpenEpisode)
            CloseEpisode(pendingEnter, null);

        return episodes;
    }

    /// <summary>Approximates the moment of closest approach within [lo, hi] when
    /// <paramref name="deviation"/> never actually reaches zero there (e.g. a station reverses the
    /// approach before the aspect becomes exact) — a local extremum of |deviation| is where its
    /// derivative crosses zero, found here via numerical differentiation and the same zero-crossing
    /// scanner used everywhere else, since deviation has no closed-form derivative available.</summary>
    private double ClosestApproachJD(double lo, double hi, double stepSize, Func<double, double> deviation)
    {
        if (hi <= lo) return lo;
        var h = Math.Min(ZeroCrossingTolerance * 1000, (hi - lo) / 4.0);

        double Derivative(double jd)
        {
            var a = Math.Max(lo, jd - h);
            var b = Math.Min(hi, jd + h);
            if (b <= a) return 0;
            return (deviation(b) - deviation(a)) / (b - a);
        }

        var extrema = ScanZeroCrossings(lo, hi, stepSize, ZeroCrossingTolerance, Derivative);
        if (extrema.Count > 0)
            return extrema.MinBy(jd => Math.Abs(deviation(jd)));

        return Math.Abs(deviation(lo)) <= Math.Abs(deviation(hi)) ? lo : hi;
    }

    // MARK: - Radix lookups

    private List<(Factors Factor, double Longitude)> RadixLongitudes(IReadOnlyList<Factors> factors) =>
        factors
            .Where(f => _radixChart.Coordinates.ContainsKey(f) && _radixChart.Coordinates[f].Ecliptical.Length > 0)
            .Select(f => (f, _radixChart.Coordinates[f].Ecliptical[0].MainPos))
            .ToList();

    private List<(Factors Factor, double Declination)> RadixDeclinations(IReadOnlyList<Factors> factors) =>
        factors
            .Where(f => _radixChart.Coordinates.ContainsKey(f) && _radixChart.Coordinates[f].Equatorial.Length > 0)
            .Select(f => (f, _radixChart.Coordinates[f].Equatorial[0].Deviation))
            .ToList();

    // MARK: - Step sizing

    /// <summary>Coarse scan step, in days of real time, for a technique and the factors involved
    /// in a particular scan. These are initial heuristics — the Moon is fast even in transits, and
    /// secondary/symbolic directions compress a lifetime into roughly a hundred days, so their
    /// internal positions barely move per real-world day.</summary>
    private static double StepSizeInDays(ProgressiveCalendarTechnique technique, IReadOnlyList<Factors> factors)
    {
        switch (technique)
        {
            case ProgressiveCalendarTechnique.Transit:
                if (factors.Contains(Factors.Moon)) return 0.1;
                var inner = new HashSet<Factors> { Factors.Sun, Factors.Mercury, Factors.Venus, Factors.Mars };
                if (factors.Any(inner.Contains)) return 0.5;
                return 2.0;
            case ProgressiveCalendarTechnique.SecondaryDirection:
                return factors.Contains(Factors.Moon) ? 5.0 : 30.0;
            case ProgressiveCalendarTechnique.SymbolicDirection:
                return 60.0;
            default:
                return 2.0;
        }
    }

    // MARK: - Low-level ephemeris helpers

    private static double CalcLongitude(Factors factor, double jd) =>
        SEWrapper.CalculateFactorPosition(jd, factor.SeId(), SeFlagsSwieph)?.MainPos ?? 0.0;

    private static double CalcLongitudeSpeed(Factors factor, double jd) =>
        SEWrapper.CalculateFactorPosition(jd, factor.SeId(), SeFlagsSwieph + SeFlagsSpeed)?.MainPosSpeed ?? 0.0;

    private static double CalcDeclination(Factors factor, double jd) =>
        SEWrapper.CalculateFactorPosition(jd, factor.SeId(), SeFlagsSwieph + SeFlagsEquatorial)?.Deviation ?? 0.0;

    private static double CalcDeclinationSpeed(Factors factor, double jd) =>
        SEWrapper.CalculateFactorPosition(jd, factor.SeId(), SeFlagsSwieph + SeFlagsSpeed + SeFlagsEquatorial)?.DeviationSpeed ?? 0.0;

    private double Obliquity(double jd) =>
        SEWrapper.CalculateFactorPosition(jd, SeEclNut, SeFlagsSwieph)?.MainPos ?? _radixChart.Obliquity;

    // MARK: - Angular helpers

    /// <summary>Signed deviation from an exact aspect, in the range roughly (−180, 180], mirroring
    /// PreNatalOrchestrator's Dist(): changes sign exactly when the aspect is exact.</summary>
    private static double SignedAspectDeviation(double lon1, double lon2, double aspectAngle)
    {
        var diff = (lon1 - lon2) % 360.0;
        if (diff < 0) diff += 360.0;
        var d1 = diff - aspectAngle;
        var d2 = diff - (360.0 - aspectAngle);
        return Math.Abs(d1) <= Math.Abs(d2) ? d1 : d2;
    }

    /// <summary>Signed shortest angular distance from lon1 to lon2, in (−180, 180].</summary>
    private static double SignedDistance(double lon1, double lon2) => SignedAspectDeviation(lon1, lon2, 0.0);

    // MARK: - Event construction

    private ProgressiveCalendarEvent MakeEvent(
        ProgressiveCalendarTechnique technique, double jd, ProgressiveCalendarEventKind kind, double longitude) =>
        new(technique, jd, DateTimeStr(jd), kind, longitude);

    private static string DateTimeStr(double jd)
    {
        var dt = SEWrapper.DateFromJulianDay(jd, true);
        var sec = Math.Min(dt.Time.Second, 59);
        return $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2} {dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{sec:D2}";
    }

    // MARK: - Zero-crossing scanner

    /// <summary>Scans [startJD, endJD] in coarse steps of <paramref name="stepSize"/> for sign
    /// changes of <paramref name="f"/>, then refines each one to <paramref name="tolerance"/> via
    /// bisection. Mirrors the bisection pattern already used by PreNatalOrchestrator, generalized
    /// into a single reusable helper since the Progressive Calendar needs it for many different
    /// quantities (speed, declination, aspect deviation, orb-boundary deviation, ...).</summary>
    private static List<double> ScanZeroCrossings(
        double startJD, double endJD, double stepSize, double tolerance,
        Func<double, double> f, Func<double, double, bool>? isPlausibleCrossing = null)
    {
        var result = new List<double>();
        if (endJD <= startJD || stepSize <= 0) return result;

        var curJD = startJD;
        var curVal = f(curJD);
        while (curJD < endJD)
        {
            var nxtJD = Math.Min(curJD + stepSize, endJD);
            var nxtVal = f(nxtJD);

            if (curVal * nxtVal < 0 && (isPlausibleCrossing?.Invoke(curVal, nxtVal) ?? true))
            {
                var lo = curJD; var hi = nxtJD;
                var loVal = curVal;
                while (hi - lo > tolerance)
                {
                    var mid = (lo + hi) / 2.0;
                    var midVal = f(mid);
                    if (loVal * midVal <= 0) { hi = mid; }
                    else { lo = mid; loVal = midVal; }
                }
                result.Add((lo + hi) / 2.0);
            }

            curJD = nxtJD;
            curVal = nxtVal;
        }
        return result;
    }
}
