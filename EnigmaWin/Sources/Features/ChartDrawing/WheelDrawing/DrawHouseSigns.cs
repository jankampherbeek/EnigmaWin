// DrawHouseSigns.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Drawing helpers for the sign ring in a house-based wheel.
/// Sign sector boundaries are proportional to actual house sizes rather than equal 30°.
/// </summary>
public static class DrawHouseSigns
{
    // MARK: - Sign boundary angles

    /// <summary>
    /// Returns 13 visual angles — one per sign boundary (0°, 30°, …, 330°) plus
    /// a closing value equal to angles[0] + 360°. The sequence is monotonically increasing.
    /// </summary>
    public static double[] SignBoundaryAngles(double[] cusps)
    {
        var angles = new List<double>(13);
        for (var i = 0; i < 12; i++)
        {
            var angle = HouseWheelPlotDataBuilder.EclipticToHouseAngle(i * 30.0, cusps);
            if (angles.Count > 0 && angle <= angles[angles.Count - 1])
                angle += 360.0;
            angles.Add(angle);
        }
        angles.Add(angles[0] + 360.0);
        return angles.ToArray();
    }

    // MARK: - Element sector fills

    /// <summary>Fills each sign sector with its element color, proportional to house sizes.</summary>
    public static void DrawSignSectors(DrawingContext ctx, Point center, double outerRadius,
                                        double[] cusps, WheelTheme? theme = null)
    {
        if (cusps.Length < 12) return;
        theme ??= WheelTheme.Color;
        var innerR  = outerRadius * WheelMetrics.OuterHouse;
        var outerR  = outerRadius * WheelMetrics.OuterSign;
        var angles  = SignBoundaryAngles(cusps);

        for (var i = 0; i < 12; i++)
        {
            var sign  = (Signs)(i + 1);
            var color = theme.SignSectorColor(sign);
            if (color == Colors.Transparent) continue;

            DrawSigns.DrawAnnularSector(ctx, center, innerR, outerR,
                angles[i], angles[i + 1],
                new SolidColorBrush(color));
        }
    }

    // MARK: - Sign separators

    /// <summary>Draws a radial line at each sign boundary in the sign ring.</summary>
    public static void DrawSignSeparators(DrawingContext ctx, Point center, double outerRadius,
                                           double[] cusps, WheelTheme? theme = null)
    {
        if (cusps.Length < 12) return;
        theme ??= WheelTheme.Color;
        var innerR  = outerRadius * WheelMetrics.OuterHouse;
        var outerR  = outerRadius * WheelMetrics.OuterSign;
        var stroke  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius);
        var pen     = new Pen(new SolidColorBrush(theme.SignSeparator), stroke);
        var angles  = SignBoundaryAngles(cusps);

        for (var i = 0; i < 12; i++)
        {
            var p1 = WheelGeometry.PointOnCircle(angles[i], innerR, center);
            var p2 = WheelGeometry.PointOnCircle(angles[i], outerR, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // MARK: - Sign glyphs

    /// <summary>Draws the zodiac sign glyph at the midpoint of each sign sector.</summary>
    public static void DrawSignGlyphs(DrawingContext ctx, Point center, double outerRadius,
                                       double[] cusps, WheelTheme? theme = null)
    {
        if (cusps.Length < 12) return;
        theme ??= WheelTheme.Color;
        var glyphR   = outerRadius * WheelMetrics.SignGlyph;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.SignGlyphFontFraction, outerRadius);
        var typeface = new Typeface("EnigmaAstrology2");
        var brush    = new SolidColorBrush(theme.SignGlyph);
        var angles   = SignBoundaryAngles(cusps);

        for (var i = 0; i < 12; i++)
        {
            var midAngle = (angles[i] + angles[i + 1]) / 2.0;
            var pt       = WheelGeometry.PointOnCircle(midAngle, glyphR, center);
            var sign     = (Signs)(i + 1);
            var glyph    = GlyphSelector.GetGlyphForSign(sign);
            DrawTextAt(ctx, glyph, pt, fontSize, typeface, brush);
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
