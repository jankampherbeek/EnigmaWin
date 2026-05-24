// DrawFrenchSigns.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Drawing helpers for the French-style zodiac sign ring:
/// element sector fills, sign separators, sign glyphs, and degree tick marks.
/// </summary>
public static class DrawFrenchSigns
{
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

    public static void DrawSignGlyphs(DrawingContext ctx, Point center, double outerRadius,
                                       double ascLong, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var glyphRadius  = outerRadius * FrenchWheelMetrics.SignGlyph;
        var fontSize     = FrenchWheelMetrics.FontSize(WheelMetrics.SignGlyphFontFraction, outerRadius);
        var offset       = DrawSigns.SignOffsetAsc(ascLong);
        var ascSignIndex = (int)(ascLong / 30.0);
        var typeface     = WheelMetrics.GlyphTypeface;
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

    private static void DrawTextAt(DrawingContext ctx, string text, Point center,
                                    double fontSize, Typeface typeface, Brush brush)
    {
        var ft = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush,
            1.0);

        ctx.DrawText(ft, new Point(center.X - ft.Width / 2, center.Y - ft.Height / 2));
    }
}
