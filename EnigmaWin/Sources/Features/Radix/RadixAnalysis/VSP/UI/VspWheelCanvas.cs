// VspWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.VSP.UI;

/// <summary>
/// Renders the radix wheel with the Venus Star Point pentagram overlay.
/// The wheel is rotated so that the Head point (sequence 3) sits at the top.
/// ASC/DSC/MC/IC lines are drawn from their real ecliptic longitudes (not from
/// the rotated frame), mirroring the Apple VspWheelCanvas approach.
/// Connects VSP points in pentagram order 1→3→5→2→4→1.
/// </summary>
public sealed class VspWheelCanvas : FrameworkElement
{
    // Pentagram star connection order
    private static readonly int[] PentagramOrder = { 1, 3, 5, 2, 4 };

    public static readonly DependencyProperty RadixDataProperty =
        DependencyProperty.Register(nameof(RadixData), typeof(WheelPlotData), typeof(VspWheelCanvas),
            new PropertyMetadata(WheelPlotData.Empty, OnVisualChanged));

    public static readonly DependencyProperty VspPositionsProperty =
        DependencyProperty.Register(nameof(VspPositions), typeof(IReadOnlyList<PresentableVspPosition>),
            typeof(VspWheelCanvas),
            new PropertyMetadata(null, OnVisualChanged));

    public static readonly DependencyProperty HeadLongitudeProperty =
        DependencyProperty.Register(nameof(HeadLongitude), typeof(double), typeof(VspWheelCanvas),
            new PropertyMetadata(0.0, OnVisualChanged));

    public static readonly DependencyProperty OriginalAscLongitudeProperty =
        DependencyProperty.Register(nameof(OriginalAscLongitude), typeof(double), typeof(VspWheelCanvas),
            new PropertyMetadata(0.0, OnVisualChanged));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(VspWheelCanvas),
            new PropertyMetadata(WheelTheme.Color, OnVisualChanged));

    public WheelPlotData                         RadixData            { get => (WheelPlotData)GetValue(RadixDataProperty);                         set => SetValue(RadixDataProperty, value); }
    public IReadOnlyList<PresentableVspPosition>? VspPositions        { get => (IReadOnlyList<PresentableVspPosition>?)GetValue(VspPositionsProperty); set => SetValue(VspPositionsProperty, value); }
    public double                                HeadLongitude         { get => (double)GetValue(HeadLongitudeProperty);                            set => SetValue(HeadLongitudeProperty, value); }
    public double                                OriginalAscLongitude  { get => (double)GetValue(OriginalAscLongitudeProperty);                     set => SetValue(OriginalAscLongitudeProperty, value); }
    public WheelTheme                            Theme                 { get => (WheelTheme)GetValue(ThemeProperty);                               set => SetValue(ThemeProperty, value); }

    private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((VspWheelCanvas)d).InvalidateVisual();

    protected override Size MeasureOverride(Size availableSize)
    {
        var side = Math.Min(
            double.IsInfinity(availableSize.Width)  ? 400 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 400 : availableSize.Height);
        return new Size(side, side);
    }

    protected override void OnRender(DrawingContext ctx)
    {
        base.OnRender(ctx);

        var w           = ActualWidth;
        var h           = ActualHeight;
        var outerRadius = Math.Min(w, h) / 2.0;
        if (outerRadius <= 0) return;

        var center  = new Point(w / 2.0, h / 2.0);
        var data    = RadixData;
        var theme   = Theme;

        // rotAsc makes Head appear at the top (visual 90°).
        var rotAsc       = HeadLongitude;
        var frameData    = data with { AscendantLongitude = rotAsc };

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

        DrawCircles.Draw(ctx, center, outerRadius, theme);
        DrawSigns.DrawElementSectors(ctx, center, outerRadius, rotAsc, theme);
        DrawSigns.DrawSignSeparators(ctx, center, outerRadius, rotAsc, theme);
        DrawSigns.DrawSignGlyphs(ctx, center, outerRadius, rotAsc, theme);
        DrawSigns.DrawDegreeLines(ctx, center, outerRadius, rotAsc, theme);

        if (data.HasTime)
        {
            // House cusp lines use the rotated frame (frameData) so they follow their zodiacal positions.
            DrawCusps.DrawCuspLines(ctx, center, outerRadius, frameData, theme);
            // Cardinal lines/labels must use the real ASC longitude so A/D/M/I track actual zodiacal positions.
            DrawVspCardinalLines(ctx, center, outerRadius, rotAsc, data.McLongitude, OriginalAscLongitude, outerRadius, theme);
            DrawVspCardinalLabels(ctx, center, outerRadius, rotAsc, data.McLongitude, OriginalAscLongitude, outerRadius, theme);
            DrawCusps.DrawCuspTexts(ctx, center, outerRadius, frameData, theme);
        }

        DrawPlanets.DrawPlanetConnectLines(ctx, center, outerRadius, frameData, theme);
        DrawPlanets.DrawPlanetGlyphs(ctx, center, outerRadius, frameData, theme);
        DrawPlanets.DrawPlanetTexts(ctx, center, outerRadius, frameData, theme);

        var positions = VspPositions;
        if (positions != null && positions.Count > 0)
        {
            DrawVspPentagram(ctx, center, outerRadius, positions, rotAsc);
            DrawVspPoints(ctx, center, outerRadius, positions, rotAsc, theme);
        }

        if (data.HasTime)
            DrawHouseNumbers(ctx, center, outerRadius, frameData, theme);
    }

    // ── VSP-aware cardinal drawing ──────────────────────────────────────────────
    // Mirrors VspWheelCanvas.swift: drawVspCardinalLines / drawVspCardinalLabels.
    // ASC angle = MundaneAngle(originalAscLon, rotAsc); DSC = ASC + 180.
    // MC  angle = MundaneAngle(mcLon, rotAsc);          IC  = MC  + 180.

    private static void DrawVspCardinalLines(DrawingContext ctx, Point center, double outerRadius,
        double rotAsc, double mcLon, double originalAscLon, double _unused, WheelTheme theme)
    {
        var innerR  = outerRadius * WheelMetrics.OuterSign;
        var outerR  = outerRadius * WheelMetrics.OuterCircle;
        var thick   = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 2.0, outerRadius);
        var cl      = theme.CuspLine;
        var color   = Color.FromArgb((byte)(WheelMetrics.CuspLineOpacity * 255), cl.R, cl.G, cl.B);
        var pen     = new Pen(new SolidColorBrush(color), thick);

        var ascAngle = WheelGeometry.MundaneAngle(originalAscLon, rotAsc);
        var dscAngle = WheelGeometry.Normalise(ascAngle + 180.0);
        var mcAngle  = WheelGeometry.MundaneAngle(mcLon, rotAsc);
        var icAngle  = WheelGeometry.Normalise(mcAngle + 180.0);

        foreach (var angle in new[] { ascAngle, dscAngle, mcAngle, icAngle })
        {
            var p1 = WheelGeometry.PointOnCircle(angle, innerR, center);
            var p2 = WheelGeometry.PointOnCircle(angle, outerR, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    private static void DrawVspCardinalLabels(DrawingContext ctx, Point center, double outerRadius,
        double rotAsc, double mcLon, double originalAscLon, double _unused, WheelTheme theme)
    {
        var r        = outerRadius * WheelMetrics.CardinalIndicator;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.CardinalFontFraction, outerRadius);
        var brush    = new SolidColorBrush(theme.CardinalIndicator);
        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

        var ascAngle = WheelGeometry.MundaneAngle(originalAscLon, rotAsc);
        var mcAngle  = WheelGeometry.MundaneAngle(mcLon, rotAsc);

        var labels = new (string Label, double Angle)[]
        {
            ("A", ascAngle),
            ("D", WheelGeometry.Normalise(ascAngle + 180.0)),
            ("M", mcAngle),
            ("I", WheelGeometry.Normalise(mcAngle + 180.0)),
        };

        foreach (var (label, angle) in labels)
        {
            var pt = WheelGeometry.PointOnCircle(angle, r, center);
            var ft = new FormattedText(label,
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                typeface, fontSize, brush, 1.0);
            ctx.DrawText(ft, new Point(pt.X - ft.Width / 2, pt.Y - ft.Height / 2));
        }
    }

    // ── House numbers ──────────────────────────────────────────────────────────
    // Drawn just inside the inner planet ring, using the rotated frameData so
    // each numeral sits at the midpoint of its house in the rotated wheel.

    private static readonly string[] RomanNumerals =
        ["I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII"];

    private static void DrawHouseNumbers(DrawingContext ctx, Point center, double outerRadius,
        WheelPlotData data, WheelTheme theme)
    {
        if (data.CuspLongitudes.Length < 12) return;

        var r        = outerRadius * 0.40;   // just inside OuterAspect (0.44), matching Apple
        var fontSize = WheelMetrics.FontSize(WheelMetrics.CardinalFontFraction * 0.85, outerRadius);
        var ascLong  = data.AscendantLongitude;
        var brush    = new SolidColorBrush(theme.CuspText);
        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Light, FontStretches.Normal);

        for (var i = 0; i < 12; i++)
        {
            var a1 = WheelGeometry.MundaneAngle(data.CuspLongitudes[i], ascLong);
            var a2 = WheelGeometry.MundaneAngle(data.CuspLongitudes[(i + 1) % 12], ascLong);
            if (a2 <= a1) a2 += 360.0;
            var midAngle = WheelGeometry.Normalise((a1 + a2) / 2.0);

            var pt = WheelGeometry.PointOnCircle(midAngle, r, center);
            var ft = new FormattedText(RomanNumerals[i],
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                typeface, fontSize, brush, 1.0);
            ctx.DrawText(ft, new Point(pt.X - ft.Width / 2, pt.Y - ft.Height / 2));
        }
    }

    // ── VSP pentagram ───────────────────────────────────────────────────────────

    private static void DrawVspPentagram(DrawingContext ctx, Point center, double outerRadius,
        IReadOnlyList<PresentableVspPosition> positions, double rotAsc)
    {
        var vspRadius  = WheelMetrics.Radius(WheelMetrics.Vsp, outerRadius);
        var lineRadius = vspRadius * 0.9;
        var lightBlue  = Color.FromArgb(160, 100, 180, 230);
        var pen        = new Pen(new SolidColorBrush(lightBlue), Math.Max(1.0, outerRadius * 0.010));

        var sorted = positions.OrderBy(p => p.SequenceId).ToList();
        var pts    = new Dictionary<int, Point>();
        foreach (var p in sorted)
        {
            var angle = WheelGeometry.MundaneAngle(p.Longitude, rotAsc);
            pts[p.SequenceId] = WheelGeometry.PointOnCircle(angle, lineRadius, center);
        }

        for (var i = 0; i < PentagramOrder.Length; i++)
        {
            var from = PentagramOrder[i];
            var to   = PentagramOrder[(i + 1) % PentagramOrder.Length];
            if (pts.TryGetValue(from, out var p1) && pts.TryGetValue(to, out var p2))
                ctx.DrawLine(pen, p1, p2);
        }
    }

    private static void DrawVspPoints(DrawingContext ctx, Point center, double outerRadius,
        IReadOnlyList<PresentableVspPosition> positions, double rotAsc, WheelTheme theme)
    {
        var vspRadius = WheelMetrics.Radius(WheelMetrics.Vsp, outerRadius);
        var fontSize  = WheelMetrics.FontSize(WheelMetrics.VspTextFraction, outerRadius);
        var circR     = fontSize * 1.1;

        var headBrush  = new SolidColorBrush(Color.FromRgb(210, 60,  60));
        var otherBrush = new SolidColorBrush(Color.FromRgb(100, 160, 220));
        var textBrush  = Brushes.White;

        var typeface = new Typeface("Segoe UI");
        var sorted   = positions.OrderBy(p => p.SequenceId).ToList();

        foreach (var vsp in sorted)
        {
            var angle  = WheelGeometry.MundaneAngle(vsp.Longitude, rotAsc);
            var pt     = WheelGeometry.PointOnCircle(angle, vspRadius, center);
            var isHead = vsp.SequenceId == 3;
            var fill   = isHead ? headBrush : otherBrush;

            ctx.DrawEllipse(fill, null, pt, circR, circR);

            var label = vsp.SequenceId.ToString();
            var ft = new FormattedText(label,
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                typeface, fontSize * 0.9, textBrush, 1.0);
            ctx.DrawText(ft, new Point(pt.X - ft.Width / 2, pt.Y - ft.Height / 2));
        }
    }
}
