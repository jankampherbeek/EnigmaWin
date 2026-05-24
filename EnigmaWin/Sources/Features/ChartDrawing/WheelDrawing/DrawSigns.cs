// DrawSigns.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Drawing helpers for the zodiac sign ring.</summary>
public static class DrawSigns
{
    public static double SignOffsetAsc(double ascLong) =>
        30.0 - ascLong % 30.0;

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

    public static void DrawSignGlyphs(DrawingContext ctx, Point center, double outerRadius,
                                       double ascLong, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var glyphRadius  = outerRadius * WheelMetrics.SignGlyph;
        var fontSize     = WheelMetrics.FontSize(WheelMetrics.SignGlyphFontFraction, outerRadius);
        var offset       = SignOffsetAsc(ascLong);
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

    public static void DrawAnnularSector(DrawingContext ctx, Point center,
                                          double innerR, double outerR,
                                          double startAngle, double endAngle,
                                          Brush fill)
    {
        const int steps = 10;
        var geo = new StreamGeometry();
        using var sgc = geo.Open();

        var outerPts = new Point[steps + 1];
        for (var i = 0; i <= steps; i++)
        {
            var frac  = (double)i / steps;
            var angle = startAngle + (endAngle - startAngle) * frac;
            outerPts[i] = WheelGeometry.PointOnCircle(angle, outerR, center);
        }

        var innerPts = new Point[steps + 1];
        for (var i = 0; i <= steps; i++)
        {
            var frac  = (double)(steps - i) / steps;
            var angle = startAngle + (endAngle - startAngle) * frac;
            innerPts[i] = WheelGeometry.PointOnCircle(angle, innerR, center);
        }

        sgc.BeginFigure(outerPts[0], true, true);
        for (var i = 1; i <= steps; i++)
            sgc.LineTo(outerPts[i], true, false);
        for (var i = 0; i <= steps; i++)
            sgc.LineTo(innerPts[i], true, false);

        ctx.DrawGeometry(fill, null, geo);
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
