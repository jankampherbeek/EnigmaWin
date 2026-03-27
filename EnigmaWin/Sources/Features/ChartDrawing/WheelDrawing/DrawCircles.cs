// DrawCircles.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia;
using Avalonia.Media;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Draws the concentric background circles that form the wheel structure.</summary>
public static class DrawCircles
{
    /// <summary>
    /// Draws the four filled rings of the wheel (outer, sign, house, aspect) with strokes.
    /// </summary>
    public static void Draw(DrawingContext ctx, Point center, double outerRadius,
                            WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var stroke = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius);
        var pen    = new Pen(new SolidColorBrush(theme.CircleStroke), stroke);

        DrawRing(ctx, center, outerRadius * WheelMetrics.OuterCircle,
            new SolidColorBrush(theme.OuterCircleBackground), null);

        DrawRing(ctx, center, outerRadius * WheelMetrics.OuterSign,
            new SolidColorBrush(theme.SignRingBackground), pen);

        DrawRing(ctx, center, outerRadius * WheelMetrics.OuterHouse,
            new SolidColorBrush(theme.HouseRingBackground), pen);

        DrawRing(ctx, center, outerRadius * WheelMetrics.OuterAspect,
            new SolidColorBrush(theme.AspectCircleBackground), pen);
    }

    private static void DrawRing(DrawingContext ctx, Point center, double radius,
                                  IBrush fill, Pen? pen)
    {
        var r    = radius;
        var rect = new Rect(center.X - r, center.Y - r, r * 2, r * 2);
        ctx.DrawEllipse(fill, pen, center, r, r);
    }
}
