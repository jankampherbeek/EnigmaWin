// ProgressiveCalendarEvent.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar;

/// <summary>The progressive technique that produced a Progressive Calendar event or episode.
/// Primary directions are intentionally excluded from the Progressive Calendar.</summary>
public enum ProgressiveCalendarTechnique
{
    Transit,
    SecondaryDirection,
    SymbolicDirection
}

/// <summary>Identifies a house cusp or angle in the radix chart as the target of a
/// "conjunction with cusp" event.</summary>
public abstract record ProgressiveCuspTarget;
public sealed record HouseCuspTarget(int Number) : ProgressiveCuspTarget;
public sealed record AscendantCuspTarget : ProgressiveCuspTarget;
public sealed record MidheavenCuspTarget : ProgressiveCuspTarget;
public sealed record EastpointCuspTarget : ProgressiveCuspTarget;
public sealed record VertexCuspTarget : ProgressiveCuspTarget;

/// <summary>A single, instantaneous Progressive Calendar event kind. Aspects and parallels are
/// not modeled here — they span a range of time and are represented by
/// <see cref="ProgressiveOrbEpisode"/> instead.</summary>
public abstract record ProgressiveCalendarEventKind;
public sealed record CuspConjunctionKind(Factors Factor, ProgressiveCuspTarget Cusp) : ProgressiveCalendarEventKind;
public sealed record RetrogradeStationKind(Factors Factor) : ProgressiveCalendarEventKind;
public sealed record DirectStationKind(Factors Factor) : ProgressiveCalendarEventKind;
public sealed record OobEnterKind(Factors Factor) : ProgressiveCalendarEventKind;
public sealed record OobExitKind(Factors Factor) : ProgressiveCalendarEventKind;
public sealed record ZeroDeclinationKind(Factors Factor) : ProgressiveCalendarEventKind;
public sealed record MaxDeclinationKind(Factors Factor, bool IsNorthern) : ProgressiveCalendarEventKind;

/// <summary>An instantaneous Progressive Calendar event, for a given technique.</summary>
public sealed record ProgressiveCalendarEvent(
    ProgressiveCalendarTechnique Technique,
    double JulianDay,
    string DateText,
    ProgressiveCalendarEventKind Kind,
    double Longitude);

// MARK: - Orb-based episodes (aspects & parallels)

/// <summary>The relationship type for an orb-based episode. Prog-to-prog cases are scoped to a
/// single technique — cross-technique combinations (e.g. transit-to-secondary-direction) are
/// out of scope for the Progressive Calendar.</summary>
public abstract record ProgressiveOrbEpisodeKind;
public sealed record AspectToRadixKind(Aspects Aspect) : ProgressiveOrbEpisodeKind;
public sealed record AspectProgToProgKind(Aspects Aspect) : ProgressiveOrbEpisodeKind;
public sealed record ParallelToRadixKind : ProgressiveOrbEpisodeKind;
public sealed record ContraParallelToRadixKind : ProgressiveOrbEpisodeKind;
public sealed record ParallelProgToProgKind : ProgressiveOrbEpisodeKind;
public sealed record ContraParallelProgToProgKind : ProgressiveOrbEpisodeKind;

/// <summary>A progressive aspect or parallel that is in orb for a span of time: it enters orb,
/// (usually) reaches its exact/minimum-orb moment, and leaves orb again.</summary>
/// <remarks>
/// <paramref name="EnterJD"/> is null when the episode is already in orb at the start of the
/// scanned date range; <paramref name="ExitJD"/> is null when it is still in orb at the end of
/// the range.
///
/// <paramref name="BecomesExact"/> is false when the progressive factor never actually reaches
/// the exact angle within the episode — typically because a retrograde/direct station reverses
/// its approach first, or because the true exact moment lies outside the scanned range entirely.
/// <paramref name="ExactJD"/>/<paramref name="MinOrb"/> still describe the closest approach found
/// within the episode in that case, but callers showing an "Exact" date to the user should treat
/// it as not truly exact (e.g. by hiding the date) rather than implying the aspect completed.
/// </remarks>
public sealed record ProgressiveOrbEpisode(
    ProgressiveCalendarTechnique Technique,
    /// <summary>The progressive factor.</summary>
    Factors Factor1,
    /// <summary>The radix factor, or the second progressive factor for a prog-to-prog episode.</summary>
    Factors Factor2,
    ProgressiveOrbEpisodeKind Kind,
    double? EnterJD,
    double ExactJD,
    double? ExitJD,
    double MinOrb,
    double MaxOrb,
    bool BecomesExact);
