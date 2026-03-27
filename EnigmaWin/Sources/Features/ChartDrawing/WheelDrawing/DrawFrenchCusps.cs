// DrawFrenchCusps.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Drawing helpers for the French-style house ring:
/// cusp lines (between DegreeR and HouseR), Roman numeral house numbers,
/// and ASC/MC astrological glyphs in the house ring.
/// </summary>
public static class DrawFrenchCusps
{
    private static readonly string[] RomanNumerals =
        ["I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII"];

    // MARK: - Cusp lines

    /// <summary>Draws a radial line for each house cusp inside the house ring (DegreeR..HouseR).</summary>
    public static void DrawCuspLines(DrawingContext ctx, Point center, double outerRadius,
                                      WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var innerR  = outerRadius * FrenchWheelMetrics.DegreeR;
        var outerR  = outerRadius * FrenchWheelMetrics.OuterHouse;
        var thin    = FrenchWheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction,       outerRadius);
        var thick   = FrenchWheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 2.0, outerRadius);
        var ascLong = data.AscendantLongitude;
        var cl      = theme.CuspLine;
        var color   = Color.FromArgb((byte)(WheelMetrics.CuspLineOpacity * 255), cl.R, cl.G, cl.B);

        for (var i = 0; i < data.CuspLongitudes.Length; i++)
        {
            var angle     = WheelGeometry.MundaneAngle(data.CuspLongitudes[i], ascLong);
            var lineWidth = (i % 3 == 0) ? thick : thin;
            var pen       = new Pen(new SolidColorBrush(color), lineWidth);
            var p1        = WheelGeometry.PointOnCircle(angle, innerR, center);
            var p2        = WheelGeometry.PointOnCircle(angle, outerR, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // MARK: - House numbers (Roman numerals)

    /// <summary>
    /// Draws Roman numerals I–XII at the midpoint between adjacent cusp lines,
    /// positioned at the midpoint of the house ring.
    /// </summary>
    public static void DrawHouseNumbers(DrawingContext ctx, Point center, double outerRadius,
                                         WheelPlotData data, WheelTheme? theme = null)
    {
        if (data.CuspLongitudes.Length < 12) return;

        theme ??= WheelTheme.Color;
        var r        = outerRadius * FrenchWheelMetrics.HouseNumber;
        var fontSize = FrenchWheelMetrics.FontSize(WheelMetrics.CardinalFontFraction, outerRadius);
        var ascLong  = data.AscendantLongitude;
        var brush    = new SolidColorBrush(theme.CuspText);
        var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);

        for (var i = 0; i < 12; i++)
        {
            var a1 = WheelGeometry.MundaneAngle(data.CuspLongitudes[i], ascLong);
            var a2 = WheelGeometry.MundaneAngle(data.CuspLongitudes[(i + 1) % 12], ascLong);
            if (a2 <= a1) a2 += 360.0;
            var midAngle = WheelGeometry.Normalise((a1 + a2) / 2.0);

            var pt = WheelGeometry.PointOnCircle(midAngle, r, center);
            DrawTextAt(ctx, RomanNumerals[i], pt, fontSize, typeface, brush);
        }
    }

    // MARK: - Cardinal glyphs (ASC and MC astrological symbols)

    /// <summary>
    /// Draws the astrological glyph for ASC and MC in the house ring,
    /// at the cusp line positions.
    /// </summary>
    public static void DrawCardinalGlyphs(DrawingContext ctx, Point center, double outerRadius,
                                           WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * FrenchWheelMetrics.CardinalGlyph;
        var fontSize = FrenchWheelMetrics.FontSize(WheelMetrics.PlanetGlyphFontFraction, outerRadius);
        var ascLong  = data.AscendantLongitude;
        var typeface = new Typeface("EnigmaAstrology2");
        var brush    = new SolidColorBrush(theme.CardinalIndicator);

        var ascAngle = WheelGeometry.MundaneAngle(data.AscendantLongitude, ascLong); // = 90°
        var mcAngle  = WheelGeometry.MundaneAngle(data.McLongitude, ascLong);

        foreach (var (factor, angle) in new[] {
            (Factors.Ascendant, ascAngle),
            (Factors.Mc, mcAngle)
        })
        {
            var pt    = WheelGeometry.PointOnCircle(angle, r, center);
            var glyph = GlyphSelector.GetGlyphForFactor(factor);
            DrawTextAt(ctx, glyph, pt, fontSize, typeface, brush);
        }
    }

    // MARK: - Text helper

    private static void DrawTextAt(DrawingContext ctx, string text, Point center,
                                    double fontSize, Typeface typeface, IBrush brush)
    {
        var ft = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush);

        ctx.DrawText(ft, new Point(center.X - ft.Width / 2, center.Y - ft.Height / 2));
    }
}
