// HouseWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// FrameworkElement that renders a house-based horoscope wheel by overriding OnRender().
/// Houses are displayed as equal 30° sectors; sign boundaries are scaled proportionally.
/// </summary>
public class HouseWheelCanvas : FrameworkElement
{
    public static readonly DependencyProperty PlotDataProperty =
        DependencyProperty.Register(nameof(PlotData), typeof(WheelPlotData), typeof(HouseWheelCanvas),
            new PropertyMetadata(WheelPlotData.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(HouseWheelCanvas),
            new PropertyMetadata(WheelTheme.Color, OnVisualPropertyChanged));

    public WheelPlotData PlotData
    {
        get => (WheelPlotData)GetValue(PlotDataProperty);
        set => SetValue(PlotDataProperty, value);
    }

    public WheelTheme Theme
    {
        get => (WheelTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((HouseWheelCanvas)d).InvalidateVisual();

    protected override void OnRender(DrawingContext ctx)
    {
        base.OnRender(ctx);

        var w           = ActualWidth;
        var h           = ActualHeight;
        var diameter    = Math.Min(w, h);
        var outerRadius = diameter / 2.0;
        if (outerRadius <= 0) return;

        var center = new Point(w / 2.0, h / 2.0);
        var data   = PlotData;
        var theme  = Theme;
        var cusps  = data.CuspLongitudes;

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

        DrawHouseCircles(ctx, center, outerRadius, theme);

        if (cusps.Length >= 12)
        {
            DrawHouseSigns.DrawSignSectors(ctx, center, outerRadius, cusps, theme);
            DrawHouseSigns.DrawSignSeparators(ctx, center, outerRadius, cusps, theme);
            DrawHouseSigns.DrawSignGlyphs(ctx, center, outerRadius, cusps, theme);
        }

        if (data.HasTime)
        {
            DrawHouseCuspLines(ctx, center, outerRadius, theme);
            DrawHouseCuspTexts(ctx, center, outerRadius, data, theme);
            DrawHouseCardinalLabels(ctx, center, outerRadius, data, theme);
        }

        DrawPlanets.DrawPlanetConnectLines(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }

    private static void DrawHouseCircles(DrawingContext ctx, Point center,
                                          double outerRadius, WheelTheme theme)
    {
        var stroke = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius);

        ctx.DrawEllipse(new SolidColorBrush(theme.OuterCircleBackground), null,
            center, outerRadius * WheelMetrics.OuterCircle, outerRadius * WheelMetrics.OuterCircle);

        var signR  = outerRadius * WheelMetrics.OuterSign;
        var pen    = new Pen(new SolidColorBrush(theme.CircleStroke), stroke);
        ctx.DrawEllipse(new SolidColorBrush(theme.SignRingBackground), pen, center, signR, signR);

        var houseR = outerRadius * WheelMetrics.OuterHouse;
        ctx.DrawEllipse(new SolidColorBrush(theme.HouseRingBackground), pen, center, houseR, houseR);
    }

    private static void DrawHouseCuspLines(DrawingContext ctx, Point center,
                                            double outerRadius, WheelTheme theme)
    {
        var outerR = outerRadius * WheelMetrics.OuterHouse;
        var thin   = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction,       outerRadius);
        var thick  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 2.0, outerRadius);
        var cl     = theme.CuspLine;
        var color  = Color.FromArgb((byte)(WheelMetrics.CuspLineOpacity * 255), cl.R, cl.G, cl.B);

        for (var i = 0; i < 12; i++)
        {
            var angle     = 90.0 + i * 30.0;
            var lineWidth = (i % 3 == 0) ? thick : thin;
            var pen       = new Pen(new SolidColorBrush(color), lineWidth);
            ctx.DrawLine(pen, center, WheelGeometry.PointOnCircle(angle, outerR, center));
        }
    }

    private static void DrawHouseCuspTexts(DrawingContext ctx, Point center,
                                            double outerRadius, WheelPlotData data,
                                            WheelTheme theme)
    {
        if (data.CuspLongitudes.Length < 12) return;
        var r        = outerRadius * 0.20;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, outerRadius);
        var typeface = new Typeface("Segoe UI");
        var brush    = new SolidColorBrush(theme.CardinalIndicator);

        for (var i = 0; i < 12; i++)
        {
            var angle  = 90.0 + i * 30.0;
            var pt     = WheelGeometry.PointOnCircle(angle, r, center);
            var text   = DrawCusps.CuspPositionText(data.CuspLongitudes[i]);

            var rotDeg = angle is <= 90.0 or > 270.0
                ? 360.0 - angle
                : 540.0 - angle;

            var ft = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                brush,
                1.0);

            var rad = rotDeg * Math.PI / 180.0;
            var cos = Math.Cos(rad);
            var sin = Math.Sin(rad);
            var transform = new MatrixTransform(new Matrix(cos, sin, -sin, cos, pt.X, pt.Y));
            ctx.PushTransform(transform);
            ctx.DrawText(ft, new Point(-ft.Width / 2, -ft.Height / 2));
            ctx.Pop();
        }
    }

    private static void DrawHouseCardinalLabels(DrawingContext ctx, Point center,
                                                 double outerRadius, WheelPlotData data,
                                                 WheelTheme theme)
    {
        if (data.CuspLongitudes.Length < 12) return;
        var cusps    = data.CuspLongitudes;
        var r        = outerRadius * WheelMetrics.CardinalIndicator;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.CardinalFontFraction, outerRadius);
        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        var brush    = new SolidColorBrush(theme.CardinalIndicator);

        var ascAngle = HouseWheelPlotDataBuilder.EclipticToHouseAngle(data.AscendantLongitude, cusps);
        var dscAngle = HouseWheelPlotDataBuilder.EclipticToHouseAngle(
            WheelGeometry.Normalise(data.AscendantLongitude + 180.0), cusps);
        var mcAngle  = HouseWheelPlotDataBuilder.EclipticToHouseAngle(data.McLongitude, cusps);
        var icAngle  = HouseWheelPlotDataBuilder.EclipticToHouseAngle(
            WheelGeometry.Normalise(data.McLongitude + 180.0), cusps);

        var labels = new (string Label, double Angle)[]
        {
            ("A", ascAngle),
            ("D", dscAngle),
            ("M", mcAngle),
            ("I", icAngle),
        };

        foreach (var (label, angle) in labels)
        {
            var pt = WheelGeometry.PointOnCircle(angle, r, center);
            var ft = new FormattedText(
                label,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                brush,
                1.0);

            ctx.DrawText(ft, new Point(pt.X - ft.Width / 2, pt.Y - ft.Height / 2));
        }
    }
}
