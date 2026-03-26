// DrawAspects.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using Avalonia;
using Avalonia.Media;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Draws aspect lines inside the inner aspect circle.</summary>
public static class DrawAspects
{
    /// <summary>
    /// Draws a line for each aspect in WheelPlotData.AspectItems.
    /// Line width scales with exactness (1.0 = exact, 0.0 = at orb edge).
    /// </summary>
    public static void Draw(DrawingContext ctx, Point center, double outerRadius,
                             WheelPlotData data, WheelTheme? theme = null,
                             double aspectRadiusFraction = WheelMetrics.OuterAspect)
    {
        if (data.AspectItems.Length == 0) return;

        theme ??= WheelTheme.Color;
        var r         = outerRadius * aspectRadiusFraction;
        var maxStroke = WheelMetrics.StrokeWidth(WheelMetrics.AspectLineFraction, outerRadius);
        var minStroke = System.Math.Max(0.5, maxStroke * 0.15);

        foreach (var item in data.AspectItems)
        {
            var p1        = WheelGeometry.PointOnCircle(item.Angle1, r, center);
            var p2        = WheelGeometry.PointOnCircle(item.Angle2, r, center);
            var lineWidth = minStroke + (maxStroke - minStroke) * item.Exactness;
            var ac        = theme.AspectLineColor(item.Color);
            var drawColor = Color.FromArgb((byte)(WheelMetrics.AspectOpacity * 255), ac.R, ac.G, ac.B);
            var pen = new Pen(new SolidColorBrush(drawColor), lineWidth);
            ctx.DrawLine(pen, p1, p2);
        }
    }
}
