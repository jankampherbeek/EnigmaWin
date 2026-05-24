// DrawPlanets.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Drawing helpers for planet glyphs, position texts, and connect lines.</summary>
public static class DrawPlanets
{
    public static void DrawPlanetConnectLines(DrawingContext ctx, Point center, double outerRadius,
                                               WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var outerR = outerRadius * WheelMetrics.OuterHouse;
        var innerR = outerRadius * WheelMetrics.InnerConnection;
        var stroke = WheelMetrics.StrokeWidth(WheelMetrics.ConnectLineFraction, outerRadius);
        var pc     = theme.PlanetConnectLine;
        var color  = Color.FromArgb((byte)(WheelMetrics.ConnectLineOpacity * 255), pc.R, pc.G, pc.B);
        var pen    = new Pen(new SolidColorBrush(color), stroke);

        foreach (var item in data.PlanetItems)
        {
            var p1 = WheelGeometry.PointOnCircle(item.MundaneAngle, outerR, center);
            var p2 = WheelGeometry.PointOnCircle(item.PlotAngle,    innerR, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

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

    public static void DrawPlanetTexts(DrawingContext ctx, Point center, double outerRadius,
                                        WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * (WheelMetrics.PlanetGlyph + 0.09);
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, outerRadius);
        var typeface = new Typeface("Segoe UI");
        var brush    = new SolidColorBrush(theme.PlanetText);

        foreach (var item in data.PlanetItems)
        {
            var pa = item.PlotAngle;
            var pt = WheelGeometry.PointOnCircle(pa, r, center);

            double rotDeg;
            if (pa < 180.0)
                rotDeg = 90.0 - pa;
            else
                rotDeg = 270.0 - pa;

            DrawRotatedTextAt(ctx, item.PositionText, pt, rotDeg, fontSize, typeface, brush);
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

    private static void DrawRotatedTextAt(DrawingContext ctx, string text, Point pt,
                                           double rotDeg, double fontSize,
                                           Typeface typeface, Brush brush)
    {
        var ft = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush,
            1.0);

        var radians = rotDeg * Math.PI / 180.0;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        var transform = new MatrixTransform(new Matrix(cos, sin, -sin, cos, pt.X, pt.Y));
        ctx.PushTransform(transform);
        ctx.DrawText(ft, new Point(-ft.Width / 2, -ft.Height / 2));
        ctx.Pop();
    }
}
