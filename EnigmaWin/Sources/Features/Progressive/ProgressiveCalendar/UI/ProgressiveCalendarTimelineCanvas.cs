// ProgressiveCalendarTimelineCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar.UI;

/// <summary>
/// Horizontal-histogram-style timeline for one Progressive Calendar technique: time on the
/// x-axis. Aspect/parallel episodes each get their own row, shaped from zero at orb-entry to a
/// peak at the exact moment and back to zero at orb-exit. OOB events share that top section,
/// interleaved chronologically. Remaining instantaneous events (stations, cusp conjunctions,
/// declination extremes) get one row per factor, with tick marks. Clicking anywhere in the plot
/// shows the exact date/time at that x-position via <see cref="SelectedDateText"/>.
/// </summary>
public sealed class ProgressiveCalendarTimelineCanvas : FrameworkElement
{
    private const double RowHeight = 30;
    private const double PeakHalfHeight = 11;
    private const double LeftLabelWidth = 64;
    private const double TopInset = 12;
    private const double SectionGap = 14;
    private const double AxisHeight = 22;
    private const int TickCount = 5;

    public static readonly DependencyProperty EpisodesProperty =
        DependencyProperty.Register(nameof(Episodes), typeof(IReadOnlyList<ProgressiveOrbEpisode>), typeof(ProgressiveCalendarTimelineCanvas),
            new FrameworkPropertyMetadata(Array.Empty<ProgressiveOrbEpisode>(), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, OnDataChanged));

    public static readonly DependencyProperty EventsProperty =
        DependencyProperty.Register(nameof(Events), typeof(IReadOnlyList<ProgressiveCalendarEvent>), typeof(ProgressiveCalendarTimelineCanvas),
            new FrameworkPropertyMetadata(Array.Empty<ProgressiveCalendarEvent>(), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, OnDataChanged));

    public static readonly DependencyProperty StartJDProperty =
        DependencyProperty.Register(nameof(StartJD), typeof(double), typeof(ProgressiveCalendarTimelineCanvas),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty EndJDProperty =
        DependencyProperty.Register(nameof(EndJD), typeof(double), typeof(ProgressiveCalendarTimelineCanvas),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly DependencyPropertyKey SelectedDateTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedDateText), typeof(string), typeof(ProgressiveCalendarTimelineCanvas),
            new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty SelectedDateTextProperty = SelectedDateTextPropertyKey.DependencyProperty;

    public IReadOnlyList<ProgressiveOrbEpisode> Episodes
    {
        get => (IReadOnlyList<ProgressiveOrbEpisode>)GetValue(EpisodesProperty);
        set => SetValue(EpisodesProperty, value);
    }
    public IReadOnlyList<ProgressiveCalendarEvent> Events
    {
        get => (IReadOnlyList<ProgressiveCalendarEvent>)GetValue(EventsProperty);
        set => SetValue(EventsProperty, value);
    }
    public double StartJD { get => (double)GetValue(StartJDProperty); set => SetValue(StartJDProperty, value); }
    public double EndJD { get => (double)GetValue(EndJDProperty); set => SetValue(EndJDProperty, value); }
    public string SelectedDateText => (string)GetValue(SelectedDateTextProperty);

    private double? _selectedJD;

    public ProgressiveCalendarTimelineCanvas()
    {
        // Belt-and-braces: keep rendering confined to the measured bounds even if some future
        // change makes OnRender draw more than MeasureOverride accounted for.
        ClipToBounds = true;
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((ProgressiveCalendarTimelineCanvas)d).InvalidateMeasure();

    protected override Size MeasureOverride(Size availableSize)
    {
        // The element must be measured at its full content height — every row actually drawn in
        // OnRender must fit within the layout space WPF reserves for it, otherwise rows spill
        // into whatever is arranged below (the tables, or the next technique's diagram). The page
        // this canvas lives on already scrolls as a whole, so there's no need to cap the height
        // here and scroll internally.
        var height = AxisHeight + 4 + ContentHeight();
        var width = double.IsInfinity(availableSize.Width) ? 400 : availableSize.Width;
        return new Size(width, height);
    }

    private double ContentHeight()
    {
        var topRows = TopRows().Count;
        var bottomRows = EventRowFactors().Count;
        var height = TopInset * 2;
        if (topRows > 0) height += topRows * RowHeight;
        if (topRows > 0 && bottomRows > 0) height += SectionGap;
        if (bottomRows > 0) height += bottomRows * RowHeight;
        return Math.Max(height, RowHeight + TopInset * 2);
    }

    // MARK: - Row composition

    private abstract record TopRow(double SortJD);
    private sealed record EpisodeRow(ProgressiveOrbEpisode Episode) : TopRow(Episode.ExactJD);
    private sealed record OobRow(ProgressiveCalendarEvent Event) : TopRow(Event.JulianDay);

    private List<TopRow> TopRows()
    {
        var episodes = Episodes ?? [];
        var oobEvents = (Events ?? []).Where(e => e.Kind is OobEnterKind or OobExitKind);
        var rows = new List<TopRow>();
        rows.AddRange(episodes.Select(e => (TopRow)new EpisodeRow(e)));
        rows.AddRange(oobEvents.Select(e => (TopRow)new OobRow(e)));
        rows.Sort((a, b) => a.SortJD.CompareTo(b.SortJD));
        return rows;
    }

    private List<Factors> EventRowFactors()
    {
        var set = new HashSet<Factors>();
        foreach (var ev in Events ?? [])
        {
            if (ev.Kind is OobEnterKind or OobExitKind) continue;
            set.Add(FactorFor(ev.Kind));
        }
        return set.OrderBy(f => (int)f).ToList();
    }

    private List<ProgressiveCalendarEvent> OtherEvents() =>
        (Events ?? []).Where(e => e.Kind is not (OobEnterKind or OobExitKind)).ToList();

    private static Factors FactorFor(ProgressiveCalendarEventKind kind) => kind switch
    {
        CuspConjunctionKind k => k.Factor,
        RetrogradeStationKind k => k.Factor,
        DirectStationKind k => k.Factor,
        OobEnterKind k => k.Factor,
        OobExitKind k => k.Factor,
        ZeroDeclinationKind k => k.Factor,
        MaxDeclinationKind k => k.Factor,
        _ => Factors.Sun
    };

    // MARK: - X-axis mapping

    private double X(double jd, double width)
    {
        var plotWidth = width - LeftLabelWidth;
        if (plotWidth <= 0 || EndJD <= StartJD) return LeftLabelWidth;
        var clamped = Math.Min(Math.Max(jd, StartJD), EndJD);
        return LeftLabelWidth + (clamped - StartJD) / (EndJD - StartJD) * plotWidth;
    }

    private double? JdForX(double xPos, double width)
    {
        var plotWidth = width - LeftLabelWidth;
        if (plotWidth <= 0 || EndJD <= StartJD || xPos < LeftLabelWidth) return null;
        var fraction = Math.Min(Math.Max((xPos - LeftLabelWidth) / plotWidth, 0.0), 1.0);
        return StartJD + fraction * (EndJD - StartJD);
    }

    // MARK: - Mouse

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        var pos = e.GetPosition(this);
        var jd = JdForX(pos.X, ActualWidth);
        if (jd is null) return;
        _selectedJD = jd;
        SetValue(SelectedDateTextPropertyKey, PreciseDateTimeStr(jd.Value));
        InvalidateVisual();
    }

    // MARK: - Rendering

    protected override void OnRender(DrawingContext ctx)
    {
        base.OnRender(ctx);

        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0) return;

        // Transparent full-bounds rect so hit-testing (clicks) works across the whole area,
        // not just where something was actually drawn.
        ctx.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, w, h));

        DrawAxis(ctx, w);
        ctx.PushTransform(new TranslateTransform(0, AxisHeight + 4));
        DrawRows(ctx, w);
        ctx.Pop();
    }

    private void DrawAxis(DrawingContext ctx, double width)
    {
        if (width - LeftLabelWidth <= 0 || EndJD <= StartJD) return;

        var linePen = new Pen(new SolidColorBrush(Color.FromArgb(64, 0, 0, 0)), 1);
        ctx.DrawLine(linePen, new Point(LeftLabelWidth, AxisHeight - 1), new Point(width, AxisHeight - 1));

        DrawCrosshair(ctx, width, AxisHeight);

        var typeface = new Typeface("Segoe UI");
        var tickBrush = new SolidColorBrush(Color.FromArgb(160, 0, 0, 0));
        for (var i = 0; i < TickCount; i++)
        {
            var jd = StartJD + (EndJD - StartJD) * i / (TickCount - 1.0);
            var tickX = X(jd, width);
            ctx.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), 1),
                new Point(tickX, AxisHeight - 5), new Point(tickX, AxisHeight - 1));

            var ft = new FormattedText(DateLabel(jd), CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                typeface, 10.5, tickBrush, 1.0);
            double drawX = i == 0 ? tickX : i == TickCount - 1 ? tickX - ft.Width : tickX - ft.Width / 2;
            ctx.DrawText(ft, new Point(drawX, 1));
        }
    }

    private void DrawRows(DrawingContext ctx, double width)
    {
        if (width - LeftLabelWidth <= 0 || EndJD <= StartJD) return;

        var glyphTypeface = WheelMetrics.GlyphTypeface;
        var textTypeface = new Typeface("Segoe UI");
        var rowTop = TopInset;

        foreach (var row in TopRows())
        {
            var midY = rowTop + RowHeight / 2;
            DrawBaseline(ctx, midY, width);

            switch (row)
            {
                case EpisodeRow er:
                {
                    var label = GlyphSelector.GetGlyphForFactor(er.Episode.Factor1)
                                + ProgressiveCalendarEpisodeRow.KindGlyphFor(er.Episode.Kind)
                                + GlyphSelector.GetGlyphForFactor(er.Episode.Factor2);
                    DrawCenteredText(ctx, label, new Point(LeftLabelWidth / 2, midY), 14, glyphTypeface, Brushes.Black);
                    DrawEpisode(ctx, er.Episode, midY, width);
                    break;
                }
                case OobRow orow:
                {
                    DrawOobRowLabel(ctx, orow.Event, midY, glyphTypeface, textTypeface);
                    DrawTick(ctx, EventColor(orow.Event.Kind), X(orow.Event.JulianDay, width), midY);
                    break;
                }
            }
            rowTop += RowHeight;
        }

        var eventRowFactors = EventRowFactors();
        if (TopRows().Count > 0 && eventRowFactors.Count > 0) rowTop += SectionGap;

        var otherEvents = OtherEvents();
        foreach (var factor in eventRowFactors)
        {
            var midY = rowTop + RowHeight / 2;
            DrawBaseline(ctx, midY, width);
            DrawCenteredText(ctx, GlyphSelector.GetGlyphForFactor(factor), new Point(LeftLabelWidth / 2, midY), 15, glyphTypeface, Brushes.Black);

            foreach (var ev in otherEvents.Where(e => FactorFor(e.Kind) == factor))
            {
                var tickX = X(ev.JulianDay, width);
                var color = EventColor(ev.Kind);
                DrawTick(ctx, color, tickX, midY);
                var label = TickLabel(ev.Kind);
                if (label is not null)
                {
                    var ft = new FormattedText(label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        textTypeface, 10, new SolidColorBrush(color), 1.0);
                    ctx.DrawText(ft, new Point(tickX - ft.Width / 2, midY - PeakHalfHeight - 14));
                }
            }
            rowTop += RowHeight;
        }

        DrawCrosshair(ctx, width, rowTop);
    }

    private void DrawCrosshair(DrawingContext ctx, double width, double height)
    {
        if (_selectedJD is not { } jd) return;
        var x = X(jd, width);
        ctx.DrawLine(new Pen(Brushes.SteelBlue, 1) { DashStyle = new DashStyle([4, 3], 0) },
            new Point(x, 0), new Point(x, height));
    }

    private void DrawBaseline(DrawingContext ctx, double midY, double width)
    {
        ctx.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(30, 0, 0, 0)), 1),
            new Point(LeftLabelWidth, midY), new Point(width, midY));
    }

    private void DrawOobRowLabel(DrawingContext ctx, ProgressiveCalendarEvent ev, double midY, Typeface glyphTypeface, Typeface textTypeface)
    {
        var factor = FactorFor(ev.Kind);
        var arrow = ev.Kind is OobEnterKind ? "↑" : "↓";
        var glyphFt = new FormattedText(GlyphSelector.GetGlyphForFactor(factor), CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, glyphTypeface, 15, Brushes.Black, 1.0);
        ctx.DrawText(glyphFt, new Point(4, midY - glyphFt.Height / 2));

        var textFt = new FormattedText(" OOB" + arrow, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            textTypeface, 10, Brushes.Black, 1.0);
        ctx.DrawText(textFt, new Point(4 + glyphFt.Width, midY - textFt.Height / 2));
    }

    private void DrawEpisode(DrawingContext ctx, ProgressiveOrbEpisode episode, double midY, double width)
    {
        var leftX = X(episode.EnterJD ?? StartJD, width);
        var exactX = X(episode.ExactJD, width);
        var rightX = X(episode.ExitJD ?? EndJD, width);

        var strength = episode.MaxOrb > 0 ? Math.Max(0.0, Math.Min(1.0, 1.0 - episode.MinOrb / episode.MaxOrb)) : 1.0;
        var peak = PeakHalfHeight * strength;
        var leftHeight = episode.EnterJD is null ? peak : 0;
        var rightHeight = episode.ExitJD is null ? peak : 0;

        var geo = new StreamGeometry();
        using (var sgc = geo.Open())
        {
            sgc.BeginFigure(new Point(leftX, midY - leftHeight), isFilled: true, isClosed: true);
            sgc.LineTo(new Point(exactX, midY - peak), true, false);
            sgc.LineTo(new Point(rightX, midY - rightHeight), true, false);
            sgc.LineTo(new Point(rightX, midY + rightHeight), true, false);
            sgc.LineTo(new Point(exactX, midY + peak), true, false);
            sgc.LineTo(new Point(leftX, midY + leftHeight), true, false);
        }
        geo.Freeze();

        var color = EpisodeColor(episode.Kind);
        ctx.DrawGeometry(new SolidColorBrush(Color.FromArgb(140, color.R, color.G, color.B)),
            new Pen(new SolidColorBrush(color), 1), geo);
    }

    private static void DrawTick(DrawingContext ctx, Color color, double x, double midY)
    {
        ctx.DrawLine(new Pen(new SolidColorBrush(color), 2), new Point(x, midY - PeakHalfHeight), new Point(x, midY + PeakHalfHeight));
        ctx.DrawEllipse(new SolidColorBrush(color), null, new Point(x, midY), 2.5, 2.5);
    }

    private static void DrawCenteredText(DrawingContext ctx, string text, Point at, double fontSize, Typeface typeface, Brush brush)
    {
        var ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, fontSize, brush, 1.0);
        ctx.DrawText(ft, new Point(at.X - ft.Width / 2, at.Y - ft.Height / 2));
    }

    // MARK: - Labels / colors

    private static string? TickLabel(ProgressiveCalendarEventKind kind) => kind switch
    {
        RetrogradeStationKind => "R",
        DirectStationKind => "D",
        ZeroDeclinationKind => "o",
        MaxDeclinationKind k => k.IsNorthern ? "+" : "-",
        CuspConjunctionKind k => ProgressiveCalendarEventRow.CuspLabel(k.Cusp),
        _ => null
    };

    private static Color EventColor(ProgressiveCalendarEventKind kind) => kind switch
    {
        CuspConjunctionKind => Colors.Gray,
        RetrogradeStationKind or DirectStationKind => Colors.Orange,
        OobEnterKind or OobExitKind => Colors.Red,
        ZeroDeclinationKind or MaxDeclinationKind => Colors.Green,
        _ => Colors.Gray
    };

    private static Color EpisodeColor(ProgressiveOrbEpisodeKind kind) => kind switch
    {
        AspectToRadixKind a => ToColor(AspectSettings.DefaultColor(a.Aspect)),
        AspectProgToProgKind a => ToColor(AspectSettings.DefaultColor(a.Aspect)),
        ParallelToRadixKind or ParallelProgToProgKind => Colors.Cyan,
        ContraParallelToRadixKind or ContraParallelProgToProgKind => Colors.Indigo,
        _ => Colors.Gray
    };

    private static Color ToColor(ColorConfig c) => Color.FromArgb(
        (byte)(c.Opacity * 255), (byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255));

    private static string DateLabel(double jd)
    {
        var dt = SEWrapper.DateFromJulianDay(jd, true);
        return $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2}";
    }

    private static string PreciseDateTimeStr(double jd)
    {
        var dt = SEWrapper.DateFromJulianDay(jd, true);
        var sec = Math.Min(dt.Time.Second, 59);
        return $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2} {dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{sec:D2}";
    }
}
