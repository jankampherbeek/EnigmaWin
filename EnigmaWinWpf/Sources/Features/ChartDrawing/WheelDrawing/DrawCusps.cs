// DrawCusps.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Drawing helpers for house cusps, cardinal axis lines, cardinal labels, and cusp texts.</summary>
public static class DrawCusps
{
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

    public static void DrawCardinalLabels(DrawingContext ctx, Point center, double outerRadius,
                                           WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * WheelMetrics.CardinalIndicator;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.CardinalFontFraction, outerRadius);
        var ascLong  = data.AscendantLongitude;
        var mcAngle  = WheelGeometry.MundaneAngle(data.McLongitude, ascLong);
        var brush    = new SolidColorBrush(theme.CardinalIndicator);
        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

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

    public static void DrawCuspTexts(DrawingContext ctx, Point center, double outerRadius,
                                      WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * WheelMetrics.CuspText;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, outerRadius);
        var ascLong  = data.AscendantLongitude;
        var brush    = new SolidColorBrush(theme.CuspText);
        var typeface = new Typeface("Segoe UI");

        foreach (var cuspLong in data.CuspLongitudes)
        {
            var angle = WheelGeometry.MundaneAngle(cuspLong, ascLong);
            var pt    = WheelGeometry.PointOnCircle(angle, r, center);
            var text  = CuspPositionText(cuspLong);
            DrawRotatedTextAt(ctx, text, pt, angle, fontSize, typeface, brush);
        }
    }

    public static string CuspPositionText(double longitude)
    {
        var inSign   = longitude % 30.0;
        var totalMin = (int)(Math.Abs(inSign) * 60);
        return $"{totalMin / 60}°{totalMin % 60:D2}'";
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
                                           double wheelAngle, double fontSize,
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

        var rotDeg = wheelAngle is <= 90.0 or > 270.0
            ? 360.0 - wheelAngle
            : 540.0 - wheelAngle;

        var transform = new MatrixTransform(
            CreateRotationTranslation(rotDeg * Math.PI / 180.0, pt.X, pt.Y));
        ctx.PushTransform(transform);
        ctx.DrawText(ft, new Point(-ft.Width / 2, -ft.Height / 2));
        ctx.Pop();
    }

    private static Matrix CreateRotationTranslation(double radians, double tx, double ty)
    {
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return new Matrix(cos, sin, -sin, cos, tx, ty);
    }
}
