// DrawSigns.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Drawing helpers for the zodiac sign ring:
/// element sector fills, sign separators, sign glyphs, and degree tick marks.
/// </summary>
public static class DrawSigns
{
    /// <summary>Offset in degrees from the ascendant to the first sign boundary (CCW).</summary>
    public static double SignOffsetAsc(double ascLong) =>
        30.0 - ascLong % 30.0;

    // MARK: - Element sector fills

    /// <summary>Fills each 30° sign sector with its element color.</summary>
    public static void DrawElementSectors(DrawingContext ctx, Point center, double outerRadius,
                                           double ascLong, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var innerR      = outerRadius * WheelMetrics.OuterHouse;
        var outerR      = outerRadius * WheelMetrics.OuterSign;
        var offset      = SignOffsetAsc(ascLong);
        var ascSignIndex = (int)(ascLong / 30.0);

        for (var i = 0; i < 12; i++)
        {
            var startAngle = i * 30.0 + offset + 90.0;
            var signIndex  = (ascSignIndex + 1 + i) % 12;
            var sign       = (Signs)(signIndex + 1);
            var color      = theme.SignSectorColor(sign);
            if (color == Colors.Transparent) continue;

            DrawAnnularSector(ctx, center, innerR, outerR,
                startAngle, startAngle + 30.0,
                new SolidColorBrush(color));
        }
    }

    // MARK: - Sign separators

    /// <summary>Draws a radial line at each 30° sign boundary in the sign ring.</summary>
    public static void DrawSignSeparators(DrawingContext ctx, Point center, double outerRadius,
                                           double ascLong, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var innerR  = outerRadius * WheelMetrics.OuterHouse;
        var outerR  = outerRadius * WheelMetrics.OuterSign;
        var offset  = SignOffsetAsc(ascLong);
        var stroke  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius);
        var pen     = new Pen(new SolidColorBrush(theme.SignSeparator), stroke);

        for (var i = 0; i < 12; i++)
        {
            var angle = i * 30.0 + offset + 90.0;
            var p1    = WheelGeometry.PointOnCircle(angle, innerR, center);
            var p2    = WheelGeometry.PointOnCircle(angle, outerR, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // MARK: - Sign glyphs

    /// <summary>Draws the zodiac sign glyph at the midpoint of each sign sector.</summary>
    public static void DrawSignGlyphs(DrawingContext ctx, Point center, double outerRadius,
                                       double ascLong, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var glyphRadius  = outerRadius * WheelMetrics.SignGlyph;
        var fontSize     = WheelMetrics.FontSize(WheelMetrics.SignGlyphFontFraction, outerRadius);
        var offset       = SignOffsetAsc(ascLong);
        var ascSignIndex = (int)(ascLong / 30.0);
        var typeface     = new Typeface("EnigmaAstrology2");
        var brush        = new SolidColorBrush(theme.SignGlyph);

        for (var i = 0; i < 12; i++)
        {
            var midAngle  = i * 30.0 + offset + 90.0 + 15.0;
            var pt        = WheelGeometry.PointOnCircle(midAngle, glyphRadius, center);
            var signIndex = (ascSignIndex + 1 + i) % 12;
            var sign      = (Signs)(signIndex + 1);
            var glyph     = GlyphSelector.GetGlyphForSign(sign);

            DrawTextAt(ctx, glyph, pt, fontSize, typeface, brush);
        }
    }

    // MARK: - Degree ticks

    /// <summary>Draws 360 tick marks around the inner edge of the sign ring (5° ticks are longer).</summary>
    public static void DrawDegreeLines(DrawingContext ctx, Point center, double outerRadius,
                                        double ascLong, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var startR = outerRadius * WheelMetrics.OuterHouse;
        var shortR = outerRadius * WheelMetrics.Degrees;
        var longR  = outerRadius * WheelMetrics.Degrees5;
        var offset = SignOffsetAsc(ascLong);
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), 1.0);

        for (var i = 0; i < 360; i++)
        {
            var angle = i + offset + 90.0;
            var endR  = (i % 5 == 0) ? longR : shortR;
            var p1    = WheelGeometry.PointOnCircle(angle, endR,   center);
            var p2    = WheelGeometry.PointOnCircle(angle, startR, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // MARK: - Geometry helper

    /// <summary>Fills an annular sector between two radii from startAngle to endAngle.</summary>
    public static void DrawAnnularSector(DrawingContext ctx, Point center,
                                          double innerR, double outerR,
                                          double startAngle, double endAngle,
                                          IBrush fill)
    {
        const int steps = 10;
        var geo = new StreamGeometry();
        using var sgc = geo.Open();

        // Build outer arc points
        var outerPts = new Point[steps + 1];
        for (var i = 0; i <= steps; i++)
        {
            var frac  = (double)i / steps;
            var angle = startAngle + (endAngle - startAngle) * frac;
            outerPts[i] = WheelGeometry.PointOnCircle(angle, outerR, center);
        }

        // Build inner arc points (reversed)
        var innerPts = new Point[steps + 1];
        for (var i = 0; i <= steps; i++)
        {
            var frac  = (double)(steps - i) / steps;
            var angle = startAngle + (endAngle - startAngle) * frac;
            innerPts[i] = WheelGeometry.PointOnCircle(angle, innerR, center);
        }

        sgc.BeginFigure(outerPts[0], true);
        for (var i = 1; i <= steps; i++)
            sgc.LineTo(outerPts[i]);
        for (var i = 0; i <= steps; i++)
            sgc.LineTo(innerPts[i]);
        sgc.EndFigure(true);

        ctx.DrawGeometry(fill, null, geo);
    }

    // MARK: - Text helper

    private static void DrawTextAt(DrawingContext ctx, string text, Point center,
                                    double fontSize, Typeface typeface, IBrush brush)
    {
        var ft = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush);

        ctx.DrawText(ft, new Point(center.X - ft.Width / 2, center.Y - ft.Height / 2));
    }
}
