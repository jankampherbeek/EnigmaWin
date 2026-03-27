// DrawDial90.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using Avalonia;
using Avalonia.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>All static rendering methods for the 90° dial wheel.</summary>
public static class DrawDial90
{
    // MARK: - Layout constants (fraction of outerRadius)

    private const double FLabelOuter = 0.99;   // outer boundary of label ring
    private const double FLabelInner = 0.90;   // inner boundary of label ring / outer of 1° ring
    private const double FDeg1Inner  = 0.87;   // inner boundary of 1° ring / outer of 0.5° ring
    private const double FTickInner  = 0.82;   // inner boundary of 0.5° ring (planet area starts here)

    private const double FPlanetGlyph = 0.78;  // planet glyph radius
    private const double FPlanetText  = 0.59;  // planet degree-text radius

    private const double FCrossArm    = 0.04;  // centre cross arm half-length

    // Tick lengths (fraction of outerRadius, pointing inward from fDeg1Inner)
    private const double FTick1    = 0.015;    // 1° tick (uniform)
    private const double FHalfTick = 0.008;    // 0.5° tick (shorter)

    // Degree labels step
    private const int LabelStep = 5;

    // MARK: - Background

    public static void DrawBackground(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        // Label ring background (light blue)
        var rOuter = outerR * FLabelOuter;
        var rInner = outerR * FLabelInner;
        DrawAnnularRing(ctx, center, rInner, rOuter, theme.OuterCircleBackground);

        // Inner disc (white)
        ctx.DrawEllipse(Brushes.White, null, center, rInner, rInner);
    }

    // MARK: - Ring strokes

    public static void DrawRingStrokes(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var sw  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerR);
        var pen = new Pen(new SolidColorBrush(theme.CircleStroke), sw);
        foreach (var frac in new[] { FLabelOuter, FLabelInner, FDeg1Inner, FTickInner })
        {
            var r = outerR * frac;
            ctx.DrawEllipse(null, pen, center, r, r);
        }
    }

    // MARK: - Degree label ring

    public static void DrawDegreeLabels(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var labelR   = outerR * ((FLabelInner + FLabelOuter) / 2.0);
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction * 1.3, outerR);
        var brush    = new SolidColorBrush(theme.PlanetText);

        // 18 labels: 0, 5, 10, …, 85  (every LabelStep degrees within 90)
        for (var i = 0; i < 90; i += LabelStep)
        {
            var visualAngle = i * 4.0;   // dial angle for this degree
            var pt          = WheelGeometry.PointOnCircle(visualAngle, labelR, center);
            DrawTextCentered(ctx, $"{i}", pt, fontSize, null, brush);
        }
    }

    // MARK: - Tick rings

    /// <summary>Draws 90 uniform 1-degree ticks in the 1° ring.</summary>
    public static void Draw1DegTicks(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var rOuter = outerR * FLabelInner;
        var rInner = rOuter - outerR * FTick1;
        var sw     = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 0.8, outerR);
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), sw);

        for (var i = 0; i < 90; i++)
        {
            var angle = i * 4.0;
            var p1    = WheelGeometry.PointOnCircle(angle, rOuter, center);
            var p2    = WheelGeometry.PointOnCircle(angle, rInner, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    /// <summary>Draws 180 half-degree ticks in the 0.5° ring (varying length).</summary>
    public static void DrawHalfDegTicks(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var rOuter = outerR * FDeg1Inner;
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), 1.0);

        for (var i = 0; i < 180; i++)
        {
            // Each half-degree = 2 visual degrees
            var angle     = i * 2.0;
            var tickFrac  = (i % 2 == 0) ? FTick1 : FHalfTick;
            var rInner    = rOuter - outerR * tickFrac;
            var p1        = WheelGeometry.PointOnCircle(angle, rOuter, center);
            var p2        = WheelGeometry.PointOnCircle(angle, rInner, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // MARK: - Centre cross

    public static void DrawCenterCross(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var arm = outerR * FCrossArm;
        var sw  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerR);
        var pen = new Pen(new SolidColorBrush(theme.CircleStroke), sw);

        ctx.DrawLine(pen, new Point(center.X - arm, center.Y), new Point(center.X + arm, center.Y));
        ctx.DrawLine(pen, new Point(center.X, center.Y - arm), new Point(center.X, center.Y + arm));
    }

    // MARK: - Planets

    public static void DrawConnectLines(DrawingContext ctx, Point center, double outerR,
                                        WheelPlotData data, WheelTheme theme)
    {
        var glyphR = outerR * FPlanetGlyph;
        var ringR  = outerR * FTickInner;
        var sw     = WheelMetrics.StrokeWidth(WheelMetrics.ConnectLineFraction, outerR);
        var color  = theme.PlanetConnectLine;
        var pen    = new Pen(new SolidColorBrush(
            Avalonia.Media.Color.FromArgb(
                (byte)(WheelMetrics.ConnectLineOpacity * 255),
                color.R, color.G, color.B)), sw);

        foreach (var item in data.PlanetItems)
        {
            var p1 = WheelGeometry.PointOnCircle(item.PlotAngle,    glyphR, center);
            var p2 = WheelGeometry.PointOnCircle(item.MundaneAngle, ringR,  center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    public static void DrawPlanetGlyphs(DrawingContext ctx, Point center, double outerR,
                                         WheelPlotData data, WheelTheme theme)
    {
        var r        = outerR * FPlanetGlyph;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PlanetGlyphFontFraction, outerR);
        var brush    = new SolidColorBrush(theme.PlanetGlyph);

        foreach (var item in data.PlanetItems)
        {
            var pt = WheelGeometry.PointOnCircle(item.PlotAngle, r, center);
            DrawTextCentered(ctx, item.Glyph, pt, fontSize, "EnigmaAstrology2", brush);
        }
    }

    public static void DrawPlanetTexts(DrawingContext ctx, Point center, double outerR,
                                        WheelPlotData data, WheelTheme theme)
    {
        var r        = outerR * FPlanetText;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, outerR);
        var brush    = new SolidColorBrush(theme.PlanetText);

        foreach (var item in data.PlanetItems)
        {
            var pt        = WheelGeometry.PointOnCircle(item.PlotAngle, r, center);
            var signGlyph = GlyphSelector.GetGlyphForSign(LongitudeToSign(item.EclipticLongitude));
            DrawRotatedTextWithGlyph(ctx, item.PositionText, signGlyph, pt, item.PlotAngle, fontSize, brush);
        }
    }

    private static Signs LongitudeToSign(double longitude)
    {
        var signIndex = (int)(longitude / 30.0) % 12;
        return (Signs)(signIndex + 1);
    }

    // MARK: - Drawing helpers

    private static void DrawAnnularRing(DrawingContext ctx, Point center,
                                         double innerR, double outerR, Avalonia.Media.Color color)
    {
        // Build an annular ring by drawing a filled outer circle and then
        // redrawing the inner area white. Simpler than building a PathGeometry.
        ctx.DrawEllipse(new SolidColorBrush(color), null, center, outerR, outerR);
        ctx.DrawEllipse(Brushes.White, null, center, innerR, innerR);
    }

    private static void DrawTextCentered(DrawingContext ctx, string text, Point center,
                                          double fontSize, string? fontFamily, IBrush brush)
    {
        var typeface  = fontFamily != null
            ? new Typeface(fontFamily)
            : Typeface.Default;
        var formatted = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush);

        var origin = new Point(center.X - formatted.Width / 2, center.Y - formatted.Height / 2);
        ctx.DrawText(formatted, origin);
    }

    /// <summary>
    /// Draws the position text (system font) followed by the sign glyph (EnigmaAstrology2),
    /// rotated radially. Mirrors the Apple Dial90 approach of two adjacent Text spans.
    /// </summary>
    private static void DrawRotatedTextWithGlyph(DrawingContext ctx, string posText, string signGlyph,
                                                  Point pt, double angleDeg, double fontSize, IBrush brush)
    {
        double rotDeg = angleDeg < 180.0 ? 90.0 - angleDeg : 270.0 - angleDeg;

        var posFormatted = new FormattedText(
            posText,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            fontSize,
            brush);

        var glyphFormatted = new FormattedText(
            " " + signGlyph,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("EnigmaAstrology2"),
            fontSize,
            brush);

        var totalWidth = posFormatted.Width + glyphFormatted.Width;
        var rad        = rotDeg * Math.PI / 180.0;
        var transform  = new Matrix(
            Math.Cos(rad), Math.Sin(rad),
           -Math.Sin(rad), Math.Cos(rad),
            pt.X, pt.Y);

        using (ctx.PushTransform(transform))
        {
            double startX = angleDeg < 180.0 ? -totalWidth : 0.0;
            double baseY  = -posFormatted.Height / 2;

            ctx.DrawText(posFormatted,   new Point(startX,                    baseY));
            ctx.DrawText(glyphFormatted, new Point(startX + posFormatted.Width, baseY));
        }
    }
}
