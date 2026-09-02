// ProgressiveCalendarResultRows.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar.UI;

/// <summary>One row in the "Other events" table (instantaneous events — stations, cusp
/// conjunctions, OOB, declination extremes).</summary>
public sealed class ProgressiveCalendarEventRow
{
    public string Glyph { get; }
    public string TypeLabel { get; }
    public string DateText { get; }
    public string PositionDms { get; }
    public string PositionSignGlyph { get; }
    public bool IsOddRow { get; }

    public ProgressiveCalendarEventRow(ProgressiveCalendarEvent ev, int index, IRosetta rosetta)
    {
        var (glyph, label) = Describe(ev.Kind, rosetta);
        Glyph = glyph;
        TypeLabel = label;
        DateText = ev.DateText;
        IsOddRow = index % 2 != 0;

        var (dms, sign, valid) = PositionInDegreesConversion.DoubleToDmsSign(ev.Longitude);
        PositionDms = valid ? dms : string.Empty;
        PositionSignGlyph = valid && sign.HasValue ? GlyphSelector.GetGlyphForSign(sign.Value) : string.Empty;
    }

    private static (string Glyph, string Label) Describe(ProgressiveCalendarEventKind kind, IRosetta rosetta) => kind switch
    {
        CuspConjunctionKind k => (
            GlyphSelector.GetGlyphForFactor(k.Factor) + GlyphSelector.GetGlyphForAspect(Aspects.Conjunction),
            CuspLabel(k.Cusp)),
        RetrogradeStationKind k => (GlyphSelector.GetGlyphForFactor(k.Factor), "Rx"),
        DirectStationKind k => (GlyphSelector.GetGlyphForFactor(k.Factor), "D"),
        OobEnterKind k => (GlyphSelector.GetGlyphForFactor(k.Factor),
            rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.type.oob") + " ↑"),
        OobExitKind k => (GlyphSelector.GetGlyphForFactor(k.Factor),
            rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.type.oob") + " ↓"),
        ZeroDeclinationKind k => (GlyphSelector.GetGlyphForFactor(k.Factor), "0°"),
        MaxDeclinationKind k => (GlyphSelector.GetGlyphForFactor(k.Factor),
            rosetta.GetText(RbFile.ProgressiveCalendar,
                k.IsNorthern ? "view.progressivecalendar.results.type.declnorth" : "view.progressivecalendar.results.type.declsouth")),
        _ => (string.Empty, string.Empty)
    };

    internal static string CuspLabel(ProgressiveCuspTarget cusp) => cusp switch
    {
        HouseCuspTarget h => $"H{h.Number}",
        AscendantCuspTarget => "ASC",
        MidheavenCuspTarget => "MC",
        EastpointCuspTarget => "EP",
        VertexCuspTarget => "VX",
        _ => string.Empty
    };
}

/// <summary>One row in the "Aspects &amp; Parallels" table (orb episodes).</summary>
public sealed class ProgressiveCalendarEpisodeRow
{
    public string Factor1Glyph { get; }
    public string KindGlyph { get; }
    public string Factor2Glyph { get; }
    public string TargetLabel { get; }
    public string EnterText { get; }
    public string ExactText { get; }
    public string ExitText { get; }
    public string OrbText { get; }
    public bool IsOddRow { get; }

    public ProgressiveCalendarEpisodeRow(ProgressiveOrbEpisode episode, int index, IRosetta rosetta)
    {
        Factor1Glyph = GlyphSelector.GetGlyphForFactor(episode.Factor1);
        Factor2Glyph = GlyphSelector.GetGlyphForFactor(episode.Factor2);
        KindGlyph = KindGlyphFor(episode.Kind);
        TargetLabel = TargetLabelFor(episode.Kind, rosetta);
        EnterText = episode.EnterJD is { } enter ? DateTimeStr(enter) : "—";
        ExactText = episode.BecomesExact ? DateTimeStr(episode.ExactJD) : "—";
        ExitText = episode.ExitJD is { } exit ? DateTimeStr(exit) : "—";
        OrbText = OrbTextFor(episode.MinOrb);
        IsOddRow = index % 2 != 0;
    }

    internal static string KindGlyphFor(ProgressiveOrbEpisodeKind kind) => kind switch
    {
        AspectToRadixKind a => GlyphSelector.GetGlyphForAspect(a.Aspect),
        AspectProgToProgKind a => GlyphSelector.GetGlyphForAspect(a.Aspect),
        ParallelToRadixKind or ParallelProgToProgKind => "∥",
        ContraParallelToRadixKind or ContraParallelProgToProgKind => "∦",
        _ => string.Empty
    };

    private static string TargetLabelFor(ProgressiveOrbEpisodeKind kind, IRosetta rosetta) => kind switch
    {
        AspectToRadixKind or ParallelToRadixKind or ContraParallelToRadixKind =>
            rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.target.radix"),
        AspectProgToProgKind or ParallelProgToProgKind or ContraParallelProgToProgKind =>
            rosetta.GetText(RbFile.ProgressiveCalendar, "view.progressivecalendar.results.target.prog"),
        _ => string.Empty
    };

    private static string OrbTextFor(double orb)
    {
        var totalMin = (int)(Math.Abs(orb) * 60);
        return $"{totalMin / 60}°{totalMin % 60:D2}'";
    }

    private static string DateTimeStr(double jd)
    {
        var dt = AstronCalc.SEWrapper.DateFromJulianDay(jd, true);
        var sec = Math.Min(dt.Time.Second, 59);
        return $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2} {dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{sec:D2}";
    }
}

/// <summary>All results for one technique (Transit / Secondary / Symbolic): the timeline diagram
/// data plus the "Aspects &amp; Parallels" and "Other events" tables.</summary>
public sealed class ProgressiveCalendarTechniqueSection
{
    public string Title { get; }
    public double StartJD { get; }
    public double EndJD { get; }

    /// <summary>Raw episodes/events, unfiltered — bound directly to the timeline canvas.</summary>
    public IReadOnlyList<ProgressiveOrbEpisode> DiagramEpisodes { get; }
    public IReadOnlyList<ProgressiveCalendarEvent> DiagramEvents { get; }

    public ObservableCollection<ProgressiveCalendarEpisodeRow> EpisodeRows { get; }
    public ObservableCollection<ProgressiveCalendarEventRow> EventRows { get; }

    public bool HasEpisodes => EpisodeRows.Count > 0;
    public bool HasEvents => EventRows.Count > 0;

    public ProgressiveCalendarTechniqueSection(
        string title,
        IReadOnlyList<ProgressiveCalendarEvent> events,
        IReadOnlyList<ProgressiveOrbEpisode> episodes,
        double startJD, double endJD,
        IRosetta rosetta)
    {
        Title = title;
        StartJD = startJD;
        EndJD = endJD;
        DiagramEpisodes = episodes;
        DiagramEvents = events;

        EpisodeRows = new ObservableCollection<ProgressiveCalendarEpisodeRow>(
            episodes.Select((e, i) => new ProgressiveCalendarEpisodeRow(e, i, rosetta)));
        EventRows = new ObservableCollection<ProgressiveCalendarEventRow>(
            events.Select((e, i) => new ProgressiveCalendarEventRow(e, i, rosetta)));
    }
}
