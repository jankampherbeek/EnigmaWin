// DrawFrenchSigns.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using System;
using Avalonia;
using Avalonia.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Drawing helpers for the French-style zodiac sign ring:
/// element sector fills, sign separators, sign glyphs, and degree tick marks.
/// In the French layout the sign ring is narrow (fAspectR..fZodiacR) and
/// degree ticks project outward from the sign ring boundary.
/// </summary>
public static class DrawFrenchSigns
{
    // MARK: - Element sector fills

    /// <summary>Fills each 30° sign sector between the aspect circle and the zodiac ring.</summary>
    public static void DrawElementSectors(DrawingContext ctx, Point center, double outerRadius,
                                           double ascLong, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var innerR       = outerRadius * FrenchWheelMetrics.OuterAspect;
        var outerR       = outerRadius * FrenchWheelMetrics.OuterSign;
        var offset       = DrawSigns.SignOffsetAsc(ascLong);
        var ascSignIndex = (int)(ascLong / 30.0);

        for (var i = 0; i < 12; i++)
        {
            var startAngle = i * 30.0 + offset + 90.0;
            var signIndex  = (ascSignIndex + 1 + i) % 12;
            var sign       = (Signs)(signIndex + 1);
            var color      = theme.SignSectorColor(sign);
            if (color == Colors.Transparent) continue;

            DrawSigns.DrawAnnularSector(ctx, center, innerR, outerR,
                startAngle, startAngle + 30.0,
                new SolidColorBrush(color));
        }
    }

    // MARK: - Sign separators

    /// <summary>Draws a radial line at each 30° sign boundary between the aspect and zodiac rings.</summary>
    public static void DrawSignSeparators(DrawingContext ctx, Point center, double outerRadius,
                                           double ascLong, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var innerR = outerRadius * FrenchWheelMetrics.OuterAspect;
        var outerR = outerRadius * FrenchWheelMetrics.OuterSign;
        var offset = DrawSigns.SignOffsetAsc(ascLong);
        var stroke = FrenchWheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius);
        var pen    = new Pen(new SolidColorBrush(theme.SignSeparator), stroke);

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
        var glyphRadius  = outerRadius * FrenchWheelMetrics.SignGlyph;
        var fontSize     = FrenchWheelMetrics.FontSize(WheelMetrics.SignGlyphFontFraction, outerRadius);
        var offset       = DrawSigns.SignOffsetAsc(ascLong);
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

    // MARK: - Degree ticks (project outward from the zodiac ring)

    /// <summary>
    /// Draws 360 tick marks starting at the outer edge of the zodiac ring,
    /// projecting outward toward the house ring. 5° ticks are longer than 1° ticks.
    /// </summary>
    public static void DrawDegreeLines(DrawingContext ctx, Point center, double outerRadius,
                                        double ascLong, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var baseR   = outerRadius * FrenchWheelMetrics.OuterSign;
        var short5R = baseR + outerRadius * FrenchWheelMetrics.Tick1;
        var long5R  = baseR + outerRadius * FrenchWheelMetrics.Tick5;
        var offset  = DrawSigns.SignOffsetAsc(ascLong);
        var pen     = new Pen(new SolidColorBrush(theme.DegreeTickStroke), 1.0);

        for (var i = 0; i < 360; i++)
        {
            var angle = i + offset + 90.0;
            var tipR  = (i % 5 == 0) ? long5R : short5R;
            var p1    = WheelGeometry.PointOnCircle(angle, baseR, center);
            var p2    = WheelGeometry.PointOnCircle(angle, tipR,  center);
            ctx.DrawLine(pen, p1, p2);
        }
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
