// DrawDeclArc.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public static class DrawDeclArc
{
    private const double FLabelOuter  = 0.99;
    private const double FLabelInner  = 0.91;
    private const double FDeg1Inner   = 0.87;
    private const double FTickInner   = 0.82;
    private const double FPlanetGlyph = 0.74;
    private const double FCrossArm    = 0.035;

    private const double FTick5Deg     = 0.080;
    private const double FTickWholeDeg = 0.030;
    private const double FTickHalfDeg  = 0.012;

    private const double FDegLabel    = 0.949;
    private const double ArcHalfAngle = 120.0;
    private const int    ArcSteps     = 240;

    public static (Point center, double R) ArcGeometry(Size size)
    {
        var w      = size.Width;
        var sin120 = Math.Sin(ArcHalfAngle * Math.PI / 180.0);
        var mSide  = w * 0.08;
        var r      = (w - 2.0 * mSide) / (2.0 * sin120);
        var mTop   = w * 0.04;
        var center = new Point(w / 2.0, mTop + r);
        return (center, r);
    }

    public static double VisualAngle(double decl) => WheelGeometry.Normalise(-decl * 4.0);

    public static void DrawBackground(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        FillArcAnnulus(ctx, center, R * FLabelOuter, R * FLabelInner, theme.OuterCircleBackground);
        FillArcDisc(ctx, center, R * FLabelInner, Colors.White);
    }

    public static void DrawRingStrokes(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var sw  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, R);
        var pen = new Pen(new SolidColorBrush(theme.CircleStroke), sw);
        pen.Freeze();
        foreach (var f in new[] { FLabelOuter, FLabelInner, FDeg1Inner, FTickInner })
            StrokeArcBoundary(ctx, center, R * f, pen);
    }

    public static void Draw1DegTicks(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var rOuter = R * FLabelInner;
        var rInner = R * FDeg1Inner;
        var sw     = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 0.8, R);
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), sw);
        pen.Freeze();
        for (var i = -30; i <= 30; i++)
        {
            var angle = VisualAngle(i);
            var p1    = WheelGeometry.PointOnCircle(angle, rOuter, center);
            var p2    = WheelGeometry.PointOnCircle(angle, rInner, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    public static void DrawHalfDegTicks(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var rOuter = R * FDeg1Inner;
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), 1.0);
        pen.Freeze();
        for (var i = -60; i <= 60; i++)
        {
            var decl  = i * 0.5;
            var angle = VisualAngle(decl);

            double tickFrac;
            if (i % 10 == 0)     tickFrac = FTick5Deg;
            else if (i % 2 == 0) tickFrac = FTickWholeDeg;
            else                  tickFrac = FTickHalfDeg;

            var rInner = rOuter - R * tickFrac;
            var p1     = WheelGeometry.PointOnCircle(angle, rOuter, center);
            var p2     = WheelGeometry.PointOnCircle(angle, rInner, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    public static void DrawDegreeLabels(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var labelR   = R * FDegLabel;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction * 1.3, R);
        var brush    = new SolidColorBrush(theme.PlanetText);
        brush.Freeze();
        for (var decl = -30; decl <= 30; decl += 5)
        {
            var angle = VisualAngle(decl);
            var pt    = WheelGeometry.PointOnCircle(angle, labelR, center);
            DrawTextCentered(ctx, $"{Math.Abs(decl)}", pt, fontSize, null, brush);
        }
    }

    public static void DrawCenterCross(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var arm = R * FCrossArm;
        var sw  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, R);
        var pen = new Pen(new SolidColorBrush(theme.CircleStroke), sw);
        pen.Freeze();
        ctx.DrawLine(pen, new Point(center.X - arm, center.Y), new Point(center.X + arm, center.Y));
        ctx.DrawLine(pen, new Point(center.X, center.Y - arm), new Point(center.X, center.Y + arm));
    }

    public static void DrawArrow(DrawingContext ctx, Point center, double R, WheelTheme theme)
    {
        var tailR   = R * FCrossArm * 2.5;
        var tipR    = R * FTickInner - 2;
        var strokeW = Math.Max(2.0, R * 0.012);
        var pen     = new Pen(new SolidColorBrush(theme.CircleStroke), strokeW);
        pen.Freeze();

        var tail = WheelGeometry.PointOnCircle(0.0, tailR, center);
        var tip  = WheelGeometry.PointOnCircle(0.0, tipR,  center);
        ctx.DrawLine(pen, tail, tip);

        var wing = R * 0.04;
        ctx.DrawLine(pen, tip, new Point(tip.X - wing, tip.Y + wing));
        ctx.DrawLine(pen, tip, new Point(tip.X + wing, tip.Y + wing));
    }

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
        pen.Freeze();
        foreach (var item in items)
        {
            var exactAngle = VisualAngle(item.Declination);
            var p1 = WheelGeometry.PointOnCircle(item.VisualAngle, glyphR, center);
            var p2 = WheelGeometry.PointOnCircle(exactAngle,       ringR,  center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    public static void DrawGlyphs(DrawingContext ctx, Point center, double R,
                                   IEnumerable<DeclArcItem> items, WheelTheme theme)
    {
        var r        = R * FPlanetGlyph;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PlanetGlyphFontFraction, R);
        var brush    = new SolidColorBrush(theme.PlanetGlyph);
        brush.Freeze();
        foreach (var item in items)
        {
            var pt = WheelGeometry.PointOnCircle(item.VisualAngle, r, center);
            DrawTextCentered(ctx, item.Glyph, pt, fontSize, "EnigmaAstrology2", brush);
        }
    }

    public static void DrawNSLabels(DrawingContext ctx, Point center, double R,
                                     string northLabel, string southLabel, WheelTheme theme)
    {
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction * 6.0, R);
        var col      = theme.PlanetText;
        var brush    = new SolidColorBrush(Color.FromArgb((byte)(0.25 * 255), col.R, col.G, col.B));
        brush.Freeze();
        var northPt = WheelGeometry.PointOnCircle(240.0, R * FPlanetGlyph * 0.65, center);
        DrawTextCentered(ctx, northLabel, northPt, fontSize, null, brush);
        var southPt = WheelGeometry.PointOnCircle(120.0, R * FPlanetGlyph * 0.65, center);
        DrawTextCentered(ctx, southLabel, southPt, fontSize, null, brush);
    }

    public static void DrawOverlay(DrawingContext ctx, Point center, double R,
                                    IReadOnlyList<DeclArcItem> items,
                                    Factors selectedFactor,
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
        redPen.Freeze();
        fadePen.Freeze();

        var selAngle = selected.VisualAngle;
        var oppAngle = WheelGeometry.Normalise(selAngle + 180.0);
        var p1 = WheelGeometry.PointOnCircle(selAngle, glyphR, center);
        var p2 = WheelGeometry.PointOnCircle(oppAngle, glyphR, center);
        ctx.DrawLine(redPen, p1, p2);

        foreach (var (b, c) in MidpointPairs(selected, items, orbDeg))
        {
            var bp = WheelGeometry.PointOnCircle(b.VisualAngle, glyphR, center);
            var cp = WheelGeometry.PointOnCircle(c.VisualAngle, glyphR, center);
            ctx.DrawLine(fadePen, bp, cp);
        }
    }

    private const double HitRadius = 20.0;

    public static Factors? NearestFactor(Point location, Point center, double R,
                                          IReadOnlyList<DeclArcItem> items)
    {
        Factors? best     = null;
        var      bestDist = double.MaxValue;
        foreach (var item in items)
        {
            var pt   = WheelGeometry.PointOnCircle(item.VisualAngle, R * FPlanetGlyph, center);
            var dx   = location.X - pt.X;
            var dy   = location.Y - pt.Y;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < HitRadius && dist < bestDist) { best = item.Factor; bestDist = dist; }
        }
        return best;
    }

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
            var mid = (others[i].Declination + others[j].Declination) / 2.0;
            if (Math.Abs(selected.Declination - mid) <= orbDeg)
                result.Add((others[i], others[j]));
        }
        return result;
    }

    private static double[] ArcAngles()
    {
        var angles = new double[ArcSteps + 1];
        for (var i = 0; i <= ArcSteps; i++)
            angles[i] = WheelGeometry.Normalise(120.0 - i);
        return angles;
    }

    private static void FillArcAnnulus(DrawingContext ctx, Point center,
                                        double outerR, double innerR, Color color)
    {
        var angles = ArcAngles();
        var geo    = new StreamGeometry();
        var sgc    = geo.Open();
        sgc.BeginFigure(WheelGeometry.PointOnCircle(angles[0], outerR, center), true, true);
        for (var i = 1; i <= ArcSteps; i++)
            sgc.LineTo(WheelGeometry.PointOnCircle(angles[i], outerR, center), true, false);
        for (var i = ArcSteps; i >= 0; i--)
            sgc.LineTo(WheelGeometry.PointOnCircle(angles[i], innerR, center), true, false);
        sgc.Close();
        geo.Freeze();
        ctx.DrawGeometry(new SolidColorBrush(color), null, geo);
    }

    private static void FillArcDisc(DrawingContext ctx, Point center, double r, Color color)
    {
        var angles = ArcAngles();
        var geo    = new StreamGeometry();
        var sgc    = geo.Open();
        sgc.BeginFigure(center, true, true);
        foreach (var a in angles)
            sgc.LineTo(WheelGeometry.PointOnCircle(a, r, center), true, false);
        sgc.Close();
        geo.Freeze();
        ctx.DrawGeometry(new SolidColorBrush(color), null, geo);
    }

    private static void StrokeArcBoundary(DrawingContext ctx, Point center, double r, Pen pen)
    {
        var angles = ArcAngles();
        var geo    = new StreamGeometry();
        var sgc    = geo.Open();
        sgc.BeginFigure(WheelGeometry.PointOnCircle(angles[0], r, center), false, false);
        for (var i = 1; i <= ArcSteps; i++)
            sgc.LineTo(WheelGeometry.PointOnCircle(angles[i], r, center), true, false);
        sgc.Close();
        geo.Freeze();
        ctx.DrawGeometry(null, pen, geo);
    }

    internal static void DrawTextCentered(DrawingContext ctx, string text, Point center,
                                           double fontSize, string? fontFamily, Brush brush)
    {
        var typeface  = fontFamily != null
            ? new Typeface(new FontFamily(fontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal)
            : new Typeface("Segoe UI");
        var formatted = new FormattedText(
            text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            typeface, fontSize, brush, 1.0);
        ctx.DrawText(formatted,
            new Point(center.X - formatted.Width  / 2,
                      center.Y - formatted.Height / 2));
    }
}
