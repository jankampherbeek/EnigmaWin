// DrawCircles.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Media;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Draws the concentric background circles that form the wheel structure.</summary>
public static class DrawCircles
{
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
                                  Brush fill, Pen? pen)
    {
        ctx.DrawEllipse(fill, pen, center, radius, radius);
    }
}
