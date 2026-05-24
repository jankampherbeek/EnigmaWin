// DrawFrenchCircles.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Media;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Draws the concentric background circles for the French-style wheel.
/// Ring structure (outside to inside):
///   outer (1.00) → house ring background (0.78) → degree zone → zodiac ring (0.60) → aspect circle (0.36)
/// Strokes are drawn at: OuterHouse, DegreeR, OuterSign, OuterAspect.
/// </summary>
public static class DrawFrenchCircles
{
    public static void Draw(DrawingContext ctx, Point center, double outerRadius,
                            WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var stroke = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius);
        var pen    = new Pen(new SolidColorBrush(theme.CircleStroke), stroke);

        DrawRing(ctx, center, outerRadius * FrenchWheelMetrics.OuterCircle,
            new SolidColorBrush(theme.OuterCircleBackground), null);

        DrawRing(ctx, center, outerRadius * FrenchWheelMetrics.OuterHouse,
            new SolidColorBrush(theme.HouseRingBackground), pen);

        DrawRing(ctx, center, outerRadius * FrenchWheelMetrics.OuterSign,
            new SolidColorBrush(theme.SignRingBackground), pen);

        DrawRing(ctx, center, outerRadius * FrenchWheelMetrics.OuterAspect,
            new SolidColorBrush(theme.AspectCircleBackground), pen);

        DrawRing(ctx, center, outerRadius * FrenchWheelMetrics.DegreeR,
            null, pen);
    }

    private static void DrawRing(DrawingContext ctx, Point center, double radius,
                                  Brush? fill, Pen? pen)
    {
        ctx.DrawEllipse(fill ?? Brushes.Transparent, pen, center, radius, radius);
    }
}
