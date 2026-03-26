// DrawPlanets.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Drawing helpers for planet glyphs, position texts, and connect lines.</summary>
public static class DrawPlanets
{
    // MARK: - Connect lines

    /// <summary>
    /// Draws a thin line from the planet's visual (plot) position to its exact mundane position
    /// on the inner edge of the house ring.
    /// </summary>
    public static void DrawPlanetConnectLines(DrawingContext ctx, Point center, double outerRadius,
                                               WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var outerR = outerRadius * WheelMetrics.OuterConnection;
        var innerR = outerRadius * WheelMetrics.OuterAspect;
        var stroke = WheelMetrics.StrokeWidth(WheelMetrics.ConnectLineFraction, outerRadius);
        var pc     = theme.PlanetConnectLine;
        var color  = Color.FromArgb((byte)(WheelMetrics.ConnectLineOpacity * 255), pc.R, pc.G, pc.B);
        var pen    = new Pen(new SolidColorBrush(color), stroke);

        foreach (var item in data.PlanetItems)
        {
            var p1 = WheelGeometry.PointOnCircle(item.PlotAngle,    outerR, center);
            var p2 = WheelGeometry.PointOnCircle(item.MundaneAngle, innerR, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // MARK: - Glyphs

    /// <summary>Draws the astrological glyph for each planet at its plot position.</summary>
    public static void DrawPlanetGlyphs(DrawingContext ctx, Point center, double outerRadius,
                                         WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * WheelMetrics.PlanetGlyph;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PlanetGlyphFontFraction, outerRadius);
        var typeface = new Typeface("EnigmaAstrology2");
        var brush    = new SolidColorBrush(theme.PlanetGlyph);

        foreach (var item in data.PlanetItems)
        {
            var pt = WheelGeometry.PointOnCircle(item.PlotAngle, r, center);
            DrawTextAt(ctx, item.Glyph, pt, fontSize, typeface, brush);
        }
    }

    // MARK: - Position texts

    /// <summary>Draws the "deg°min'" position text radially beside each planet glyph.</summary>
    public static void DrawPlanetTexts(DrawingContext ctx, Point center, double outerRadius,
                                        WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * (WheelMetrics.PlanetGlyph + 0.09);
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, outerRadius);
        var typeface = Typeface.Default;
        var brush    = new SolidColorBrush(theme.PlanetText);

        foreach (var item in data.PlanetItems)
        {
            var pa = item.PlotAngle;
            var pt = WheelGeometry.PointOnCircle(pa, r, center);

            // Text reads outward from the center; flip for the right half
            double rotDeg;
            if (pa < 180.0)
                rotDeg = 90.0 - pa;
            else
                rotDeg = 270.0 - pa;

            DrawRotatedTextAt(ctx, item.PositionText, pt, rotDeg, fontSize, typeface, brush);
        }
    }

    // MARK: - Helpers

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

    private static void DrawRotatedTextAt(DrawingContext ctx, string text, Point pt,
                                           double rotDeg, double fontSize,
                                           Typeface typeface, IBrush brush)
    {
        var ft = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush);

        using var _ = ctx.PushTransform(
            Matrix.CreateRotation(rotDeg * System.Math.PI / 180.0) *
            Matrix.CreateTranslation(pt.X, pt.Y));

        ctx.DrawText(ft, new Point(-ft.Width / 2, -ft.Height / 2));
    }
}
