// DrawDeclArc.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

/// <summary>
/// A single factor plotted on the declination arc.
/// visualAngle = Normalise(-declination * 4):
///   decl =   0  →  0° (top, 12 o'clock)
///   decl = +30  → 240° (bottom-left, north end)
///   decl = -30  → 120° (bottom-right, south end)
/// </summary>
public record DeclArcItem(
    EnigmaWin.Sources.Domain.Factors Factor,
    string  Glyph,
    double  Declination,
    double  VisualAngle);    // after anti-overlap nudge

/// <summary>All static rendering methods for the 240° declination arc diagram.</summary>
public static class DrawDeclArc
{
    // ── Layout constants (fraction of outer radius R) ────────────────────────

    private const double FLabelOuter   = 0.99;
    private const double FLabelInner   = 0.91;
    private const double FDeg1Inner    = 0.87;
    private const double FTickInner    = 0.82;   // planet area starts here
    private const double FPlanetGlyph  = 0.74;
    private const double FCrossArm     = 0.035;

    // Tick lengths (fraction of R, pointing inward from boundary)
    private const double FTick5Deg      = 0.080;
    private const double FTickWholeDeg  = 0.030;
    private const double FTickHalfDeg   = 0.012;

    // Label ring mid-radius fraction
    private const double FDegLabel  = 0.949;

    // Arc: the hidden bottom segment spans ±120° from the bottom (= 180°).
    // Visible arc: from 120° through 0° to 240° (clockwise) = 240°.
    private const double ArcHalfAngle = 120.0;   // degrees from top to each endpoint

    // ── Geometry ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Computes the circle center and outer radius so the arc fits the available size.
    /// The circle center is placed so the arc top (0°) is near the top of the canvas
    /// and the two endpoints (±120°) sit just above the canvas bottom.
    /// </summary>
    public static (Point center, double R) ArcGeometry(Size size)
    {
        var w      = size.Width;
        // arc width = 2 * R * sin(120°); leave 8% margin each side
        var sin120 = Math.Sin(ArcHalfAngle * Math.PI / 180.0);  // ≈ 0.8660
        var mSide  = w * 0.08;
        var r      = (w - 2.0 * mSide) / (2.0 * sin120);
        var mTop   = w * 0.04;
        var center = new Point(w / 2.0, mTop + r);
        return (center, r);
    }

    /// <summary>Maps declination (−30…+30) to visual angle on the arc.</summary>
    public static double VisualAngle(double decl) => WheelGeometry.Normalise(-decl * 4.0);

    // ── Background ───────────────────────────────────────────────────────────

    public static void DrawBackground(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        // Label ring (outer annulus)
        FillArcAnnulus(ctx, center, R * FLabelOuter, R * FLabelInner, theme.OuterCircleBackground);
        // Inner disc
        FillArcDisc(ctx, center, R * FLabelInner, Colors.White);
    }

    // ── Ring boundary strokes ────────────────────────────────────────────────

    public static void DrawRingStrokes(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var sw  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, R);
        var pen = new Pen(new SolidColorBrush(theme.CircleStroke), sw);
        foreach (var f in new[] { FLabelOuter, FLabelInner, FDeg1Inner, FTickInner })
            StrokeArcBoundary(ctx, center, R * f, pen);
    }

    // ── 1° ring: 61 ticks (decl = -30…+30) ──────────────────────────────────

    public static void Draw1DegTicks(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var rOuter = R * FLabelInner;
        var rInner = R * FDeg1Inner;
        var sw     = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 0.8, R);
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), sw);

        for (var i = -30; i <= 30; i++)
        {
            var angle = VisualAngle(i);
            var p1    = WheelGeometry.PointOnCircle(angle, rOuter, center);
            var p2    = WheelGeometry.PointOnCircle(angle, rInner, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // ── 0.5° ring: 121 ticks with variable lengths ───────────────────────────

    public static void DrawHalfDegTicks(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var rOuter = R * FDeg1Inner;
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), 1.0);

        // i = -60…+60 in half-degree steps
        for (var i = -60; i <= 60; i++)
        {
            var decl  = i * 0.5;
            var angle = VisualAngle(decl);

            double tickFrac;
            if (i % 10 == 0)      tickFrac = FTick5Deg;      // 5° boundary
            else if (i % 2 == 0)  tickFrac = FTickWholeDeg;  // 1° boundary
            else                   tickFrac = FTickHalfDeg;   // 0.5° midpoint

            var rInner = rOuter - R * tickFrac;
            var p1     = WheelGeometry.PointOnCircle(angle, rOuter, center);
            var p2     = WheelGeometry.PointOnCircle(angle, rInner, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // ── Degree labels: 0, ±5, ±10, … ±30 ────────────────────────────────────

    public static void DrawDegreeLabels(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var labelR   = R * FDegLabel;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction * 1.3, R);
        var brush    = new SolidColorBrush(theme.PlanetText);

        for (var decl = -30; decl <= 30; decl += 5)
        {
            var angle = VisualAngle(decl);
            var pt    = WheelGeometry.PointOnCircle(angle, labelR, center);
            DrawTextCentered(ctx, $"{Math.Abs(decl)}", pt, fontSize, null, brush);
        }
    }

    // ── Centre cross ─────────────────────────────────────────────────────────

    public static void DrawCenterCross(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var arm = R * FCrossArm;
        var sw  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, R);
        var pen = new Pen(new SolidColorBrush(theme.CircleStroke), sw);
        ctx.DrawLine(pen, new Point(center.X - arm, center.Y), new Point(center.X + arm, center.Y));
        ctx.DrawLine(pen, new Point(center.X, center.Y - arm), new Point(center.X, center.Y + arm));
    }

    // ── Arrow from just above cross to 0° (top) ──────────────────────────────

    public static void DrawArrow(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var tailR    = R * FCrossArm * 2.5;          // just above the cross
        var tipR     = R * FTickInner - 2;            // just inside the tick ring
        var strokeW  = Math.Max(2.0, R * 0.012);

        // Angle 0° = top of circle → PointOnCircle(0, r, center) gives (cx, cy - r)
        var tail = WheelGeometry.PointOnCircle(0.0, tailR, center);
        var tip  = WheelGeometry.PointOnCircle(0.0, tipR,  center);

        var pen = new Pen(new SolidColorBrush(theme.CircleStroke), strokeW);
        ctx.DrawLine(pen, tail, tip);

        // Arrowhead wings (perpendicular to upward direction)
        var wing = R * 0.04;
        var wL   = new Point(tip.X - wing, tip.Y + wing);
        var wR   = new Point(tip.X + wing, tip.Y + wing);
        ctx.DrawLine(pen, tip, wL);
        ctx.DrawLine(pen, tip, wR);
    }

    // ── Connect lines ─────────────────────────────────────────────────────────

    public static void DrawConnectLines(DrawingContext ctx, Point center, double R,
                                         IEnumerable<DeclArcItem> items, WheelTheme theme)
    {
        var glyphR = R * FPlanetGlyph;
        var ringR  = R * FTickInner;
        var sw     = WheelMetrics.StrokeWidth(WheelMetrics.ConnectLineFraction, R);
        var color  = theme.PlanetConnectLine;
        var pen    = new Pen(new SolidColorBrush(
            Color.FromArgb((byte)(WheelMetrics.ConnectLineOpacity * 255),
                           color.R, color.G, color.B)), sw);

        foreach (var item in items)
        {
            var exactAngle = VisualAngle(item.Declination);
            var p1 = WheelGeometry.PointOnCircle(item.VisualAngle, glyphR, center);
            var p2 = WheelGeometry.PointOnCircle(exactAngle,       ringR,  center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // ── Planet glyphs ─────────────────────────────────────────────────────────

    public static void DrawGlyphs(DrawingContext ctx, Point center, double R,
                                   IEnumerable<DeclArcItem> items, WheelTheme theme)
    {
        var r        = R * FPlanetGlyph;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PlanetGlyphFontFraction, R);
        var brush    = new SolidColorBrush(theme.PlanetGlyph);

        foreach (var item in items)
        {
            var pt = WheelGeometry.PointOnCircle(item.VisualAngle, r, center);
            DrawTextCentered(ctx, item.Glyph, pt, fontSize, "EnigmaAstrology2", brush);
        }
    }

    // ── N / S labels ─────────────────────────────────────────────────────────

    public static void DrawNSLabels(DrawingContext ctx, Point center, double R,
                                     string northLabel, string southLabel, WheelTheme theme)
    {
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction * 6.0, R);
        var brush    = new SolidColorBrush(
            Color.FromArgb((byte)(0.25 * 255),
                           theme.PlanetText.R, theme.PlanetText.G, theme.PlanetText.B));

        // North label: arc endpoint at 240° (bottom-left in screen coords, because +30 decl → 240°)
        var northPt = WheelGeometry.PointOnCircle(240.0, R * FPlanetGlyph * 0.65, center);
        DrawTextCentered(ctx, northLabel, northPt, fontSize, null, brush);

        // South label: arc endpoint at 120° (bottom-right in screen coords, because -30 decl → 120°)
        var southPt = WheelGeometry.PointOnCircle(120.0, R * FPlanetGlyph * 0.65, center);
        DrawTextCentered(ctx, southLabel, southPt, fontSize, null, brush);
    }

    // ── Overlay: opposition + midpoint pair lines ─────────────────────────────

    /// <summary>Draws a red opposition line and midpoint pair lines for the selected factor.</summary>
    public static void DrawOverlay(DrawingContext ctx, Point center, double R,
                                    IReadOnlyList<DeclArcItem> items,
                                    EnigmaWin.Sources.Domain.Factors selectedFactor,
                                    double orbDeg)
    {
        DeclArcItem? selected = null;
        foreach (var item in items)
            if (item.Factor == selectedFactor) { selected = item; break; }
        if (selected is null) return;

        var glyphR  = R * FPlanetGlyph;
        var strokeW = Math.Max(1.0, R * 0.006);
        var redPen  = new Pen(Brushes.Red, strokeW);
        var fadePen = new Pen(new SolidColorBrush(Color.FromArgb(191, 255, 0, 0)), strokeW);

        // Opposition line through center
        var selAngle = selected.VisualAngle;
        var oppAngle = WheelGeometry.Normalise(selAngle + 180.0);
        var p1 = WheelGeometry.PointOnCircle(selAngle, glyphR, center);
        var p2 = WheelGeometry.PointOnCircle(oppAngle, glyphR, center);
        ctx.DrawLine(redPen, p1, p2);

        // Midpoint pair lines: (B, C) whose declination midpoint ≈ selected.Declination
        foreach (var (b, c) in MidpointPairs(selected, items, orbDeg))
        {
            var bp = WheelGeometry.PointOnCircle(b.VisualAngle, glyphR, center);
            var cp = WheelGeometry.PointOnCircle(c.VisualAngle, glyphR, center);
            ctx.DrawLine(fadePen, bp, cp);
        }
    }

    // ── Hit testing ───────────────────────────────────────────────────────────

    private const double HitRadius = 20.0;

    public static EnigmaWin.Sources.Domain.Factors? NearestFactor(
        Point location, Point center, double R, IReadOnlyList<DeclArcItem> items)
    {
        var glyphR   = R * FPlanetGlyph;
        EnigmaWin.Sources.Domain.Factors? best     = null;
        var                               bestDist = double.MaxValue;

        foreach (var item in items)
        {
            var pt   = WheelGeometry.PointOnCircle(item.VisualAngle, glyphR, center);
            var dx   = location.X - pt.X;
            var dy   = location.Y - pt.Y;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < HitRadius && dist < bestDist)
            {
                best     = item.Factor;
                bestDist = dist;
            }
        }
        return best;
    }

    // ── Anti-overlap ──────────────────────────────────────────────────────────

    /// <summary>
    /// Nudges items that are very close in visual angle so glyphs don't overlap.
    /// Mirrors the Apple resolveArcOverlaps helper.
    /// </summary>
    public static List<DeclArcItem> ResolveOverlaps(IEnumerable<DeclArcItem> items)
    {
        var result = new List<DeclArcItem>(items);
        result.Sort((a, b) => a.VisualAngle.CompareTo(b.VisualAngle));
        const double minSep = 6.0;
        for (var i = 1; i < result.Count; i++)
        {
            var prev = result[i - 1].VisualAngle;
            var curr = result[i].VisualAngle;
            if (curr - prev < minSep)
                result[i] = result[i] with { VisualAngle = prev + minSep };
        }
        return result;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static List<(DeclArcItem, DeclArcItem)> MidpointPairs(
        DeclArcItem selected, IReadOnlyList<DeclArcItem> all, double orbDeg)
    {
        var others = new List<DeclArcItem>();
        foreach (var item in all)
            if (item.Factor != selected.Factor) others.Add(item);

        var result = new List<(DeclArcItem, DeclArcItem)>();
        for (var i = 0; i < others.Count; i++)
        for (var j = i + 1; j < others.Count; j++)
        {
            var b       = others[i];
            var c       = others[j];
            var midDecl = (b.Declination + c.Declination) / 2.0;
            if (Math.Abs(selected.Declination - midDecl) <= orbDeg)
                result.Add((b, c));
        }
        return result;
    }

    // Arc step resolution: 1° per segment gives smooth curves
    private const int ArcSteps = 240;

    /// <summary>Returns the 241 sample angles for the visible arc, from 120° through 0° to 240°.</summary>
    private static double[] ArcAngles()
    {
        // The arc runs from 120° counter-clockwise to 240° (passing through 0°).
        // In our WheelGeometry convention (0° = top, CW positive), going counter-clockwise
        // from 120° means subtracting: 120, 119, ..., 1, 0, 359, ..., 241, 240.
        var angles = new double[ArcSteps + 1];
        for (var i = 0; i <= ArcSteps; i++)
            angles[i] = WheelGeometry.Normalise(120.0 - i);
        return angles;
    }

    private static void FillArcAnnulus(DrawingContext ctx, Point center,
                                        double outerR, double innerR, Color color)
    {
        var angles    = ArcAngles();
        var geo       = new StreamGeometry();
        using var sgc = geo.Open();

        // Outer arc forward
        sgc.BeginFigure(WheelGeometry.PointOnCircle(angles[0], outerR, center), true);
        for (var i = 1; i <= ArcSteps; i++)
            sgc.LineTo(WheelGeometry.PointOnCircle(angles[i], outerR, center));
        // Inner arc reversed
        for (var i = ArcSteps; i >= 0; i--)
            sgc.LineTo(WheelGeometry.PointOnCircle(angles[i], innerR, center));
        sgc.EndFigure(true);

        ctx.DrawGeometry(new SolidColorBrush(color), null, geo);
    }

    private static void FillArcDisc(DrawingContext ctx, Point center, double r, Color color)
    {
        var angles    = ArcAngles();
        var geo       = new StreamGeometry();
        using var sgc = geo.Open();

        sgc.BeginFigure(center, true);
        foreach (var a in angles)
            sgc.LineTo(WheelGeometry.PointOnCircle(a, r, center));
        sgc.EndFigure(true);

        ctx.DrawGeometry(new SolidColorBrush(color), null, geo);
    }

    private static void StrokeArcBoundary(DrawingContext ctx, Point center, double r, Pen pen)
    {
        var angles    = ArcAngles();
        var geo       = new StreamGeometry();
        using var sgc = geo.Open();

        sgc.BeginFigure(WheelGeometry.PointOnCircle(angles[0], r, center), false);
        for (var i = 1; i <= ArcSteps; i++)
            sgc.LineTo(WheelGeometry.PointOnCircle(angles[i], r, center));
        sgc.EndFigure(false);

        ctx.DrawGeometry(null, pen, geo);
    }

    private static void DrawTextCentered(DrawingContext ctx, string text, Point center,
                                          double fontSize, string? fontFamily, IBrush brush)
    {
        var typeface  = fontFamily != null ? new Typeface(fontFamily) : Typeface.Default;
        var formatted = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface, fontSize, brush);

        ctx.DrawText(formatted,
            new Point(center.X - formatted.Width  / 2,
                      center.Y - formatted.Height / 2));
    }
}
