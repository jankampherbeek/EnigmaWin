// DrawCusps.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Drawing helpers for house cusps, cardinal axis lines, cardinal labels (A/D/M/I),
/// and cusp position texts.
/// </summary>
public static class DrawCusps
{
    // MARK: - Cusp lines

    /// <summary>Draws a radial line for each house cusp inside the house ring.</summary>
    public static void DrawCuspLines(DrawingContext ctx, Point center, double outerRadius,
                                      WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var innerR  = outerRadius * WheelMetrics.OuterAspect;
        var outerR  = outerRadius * WheelMetrics.OuterHouse;
        var thin    = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction,       outerRadius);
        var thick   = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 2.0, outerRadius);
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

    // MARK: - Cardinal lines

    /// <summary>Draws the ASC, DSC, MC, and IC lines extending into the sign ring.</summary>
    public static void DrawCardinalLines(DrawingContext ctx, Point center, double outerRadius,
                                          WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var innerR  = outerRadius * WheelMetrics.OuterSign;
        var outerR  = outerRadius * WheelMetrics.OuterCircle;
        var thick   = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 2.0, outerRadius);
        var ascLong = data.AscendantLongitude;
        var cl2     = theme.CuspLine;
        var color   = Color.FromArgb((byte)(WheelMetrics.CuspLineOpacity * 255), cl2.R, cl2.G, cl2.B);
        var pen     = new Pen(new SolidColorBrush(color), thick);

        var mcAngle = WheelGeometry.MundaneAngle(data.McLongitude, ascLong);
        var icAngle = WheelGeometry.Normalise(mcAngle + 180.0);

        foreach (var angle in new[] { 90.0, 270.0, mcAngle, icAngle })
        {
            var p1 = WheelGeometry.PointOnCircle(angle, innerR, center);
            var p2 = WheelGeometry.PointOnCircle(angle, outerR, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // MARK: - Cardinal labels

    /// <summary>Draws A, D, M, I labels at the ASC, DSC, MC, IC positions.</summary>
    public static void DrawCardinalLabels(DrawingContext ctx, Point center, double outerRadius,
                                           WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * WheelMetrics.CardinalIndicator;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.CardinalFontFraction, outerRadius);
        var ascLong  = data.AscendantLongitude;
        var mcAngle  = WheelGeometry.MundaneAngle(data.McLongitude, ascLong);
        var brush    = new SolidColorBrush(theme.CardinalIndicator);
        var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);

        var labels = new (string Label, double Angle)[]
        {
            ("A", 90.0),
            ("D", 270.0),
            ("M", mcAngle),
            ("I", WheelGeometry.Normalise(mcAngle + 180.0)),
        };

        foreach (var (label, angle) in labels)
        {
            var pt = WheelGeometry.PointOnCircle(angle, r, center);
            DrawTextAt(ctx, label, pt, fontSize, typeface, brush);
        }
    }

    // MARK: - Cusp position texts

    /// <summary>Draws the "deg°min'" position text alongside each cusp line.</summary>
    public static void DrawCuspTexts(DrawingContext ctx, Point center, double outerRadius,
                                      WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * WheelMetrics.CuspText;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, outerRadius);
        var ascLong  = data.AscendantLongitude;
        var brush    = new SolidColorBrush(theme.CuspText);
        var typeface = Typeface.Default;

        foreach (var cuspLong in data.CuspLongitudes)
        {
            var angle = WheelGeometry.MundaneAngle(cuspLong, ascLong);
            var pt    = WheelGeometry.PointOnCircle(angle, r, center);
            var text  = CuspPositionText(cuspLong);
            DrawRotatedTextAt(ctx, text, pt, angle, fontSize, typeface, brush);
        }
    }

    // MARK: - Helpers

    /// <summary>Formats a cusp longitude as "deg°min'" within the sign.</summary>
    public static string CuspPositionText(double longitude)
    {
        var inSign   = longitude % 30.0;
        var totalMin = (int)(Math.Abs(inSign) * 60);
        return $"{totalMin / 60}°{totalMin % 60:D2}'";
    }

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
                                           double wheelAngle, double fontSize,
                                           Typeface typeface, IBrush brush)
    {
        var ft = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush);

        // Rotate so text reads perpendicular to the cusp line and stays legible
        var rotDeg = wheelAngle is <= 90.0 or > 270.0
            ? 360.0 - wheelAngle
            : 540.0 - wheelAngle;

        using var _ = ctx.PushTransform(
            Matrix.CreateRotation(rotDeg * Math.PI / 180.0) *
            Matrix.CreateTranslation(pt.X, pt.Y));

        ctx.DrawText(ft, new Point(-ft.Width / 2, -ft.Height / 2));
    }
}
