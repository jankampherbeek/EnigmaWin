// DrawAscMc.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Draws indicator lines and labels for the Ascendant and MC at arbitrary ecliptic longitudes,
/// e.g. harmonic positions. Pass null for either to skip it. wheelAscLongitude is the longitude placed at the left.
/// </summary>
public static class DrawAscMc
{
    public static void DrawAscMcLines(DrawingContext ctx, Point center, double outerRadius,
                                      double? ascLongitude, double? mcLongitude,
                                      double wheelAscLongitude, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var innerR = outerRadius * WheelMetrics.OuterSign;
        var outerR = outerRadius * WheelMetrics.OuterCircle;
        var thick  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 2.0, outerRadius);
        var cl     = theme.CuspLine;
        var color  = Color.FromArgb((byte)(WheelMetrics.CuspLineOpacity * 255), cl.R, cl.G, cl.B);
        var pen    = new Pen(new SolidColorBrush(color), thick);

        foreach (var longitude in new[] { ascLongitude, mcLongitude })
        {
            if (longitude is not { } lon) continue;
            var angle = WheelGeometry.MundaneAngle(lon, wheelAscLongitude);
            var p1    = WheelGeometry.PointOnCircle(angle, innerR, center);
            var p2    = WheelGeometry.PointOnCircle(angle, outerR, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    public static void DrawAscMcLabels(DrawingContext ctx, Point center, double outerRadius,
                                       double? ascLongitude, double? mcLongitude,
                                       double wheelAscLongitude, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * WheelMetrics.CardinalIndicator;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.CardinalFontFraction, outerRadius);
        var brush    = new SolidColorBrush(theme.CardinalIndicator);
        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

        foreach (var (longitude, label) in new[] { (ascLongitude, "A"), (mcLongitude, "M") })
        {
            if (longitude is not { } lon) continue;
            var angle = WheelGeometry.MundaneAngle(lon, wheelAscLongitude);
            var pt    = WheelGeometry.PointOnCircle(angle, r, center);
            var ft    = new FormattedText(label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                          typeface, fontSize, brush, 1.0);
            ctx.DrawText(ft, new Point(pt.X - ft.Width / 2, pt.Y - ft.Height / 2));
        }
    }
}
