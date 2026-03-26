// HouseWheelCanvas.axaml.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// Canvas that renders a house-based horoscope wheel by overriding Render().
/// Houses are displayed as equal 30° sectors; sign boundaries are scaled proportionally.
/// Set PlotData and Theme before the control is shown.
/// </summary>
public partial class HouseWheelCanvas : UserControl
{
    public static readonly StyledProperty<WheelPlotData> PlotDataProperty =
        AvaloniaProperty.Register<HouseWheelCanvas, WheelPlotData>(
            nameof(PlotData), WheelPlotData.Empty);

    public static readonly StyledProperty<WheelTheme> ThemeProperty =
        AvaloniaProperty.Register<HouseWheelCanvas, WheelTheme>(
            nameof(Theme), WheelTheme.Color);

    public WheelPlotData PlotData
    {
        get => GetValue(PlotDataProperty);
        set => SetValue(PlotDataProperty, value);
    }

    public WheelTheme Theme
    {
        get => GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    public HouseWheelCanvas()
    {
        InitializeComponent();

        PlotDataProperty.Changed.AddClassHandler<HouseWheelCanvas>((c, _) => c.InvalidateVisual());
        ThemeProperty.Changed.AddClassHandler<HouseWheelCanvas>((c, _) => c.InvalidateVisual());
    }

    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);

        var size        = Bounds.Size;
        var diameter    = Math.Min(size.Width, size.Height);
        var outerRadius = diameter / 2.0;
        if (outerRadius <= 0) return;

        var center      = new Point(size.Width / 2.0, size.Height / 2.0);
        var data        = PlotData;
        var theme       = Theme;
        var cusps       = data.CuspLongitudes;

        // White background
        ctx.FillRectangle(Brushes.White, new Rect(size));

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

    // MARK: - Circles

    private static void DrawHouseCircles(DrawingContext ctx, Point center,
                                          double outerRadius, WheelTheme theme)
    {
        var stroke = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius);

        // Outer background circle (no stroke)
        FillCircle(ctx, center, outerRadius * WheelMetrics.OuterCircle,
            new SolidColorBrush(theme.OuterCircleBackground));

        // Sign ring
        var signR = outerRadius * WheelMetrics.OuterSign;
        FillCircle(ctx, center, signR, new SolidColorBrush(theme.SignRingBackground));
        StrokeCircle(ctx, center, signR, new Pen(new SolidColorBrush(theme.CircleStroke), stroke));

        // House ring
        var houseR = outerRadius * WheelMetrics.OuterHouse;
        FillCircle(ctx, center, houseR, new SolidColorBrush(theme.HouseRingBackground));
        StrokeCircle(ctx, center, houseR, new Pen(new SolidColorBrush(theme.CircleStroke), stroke));
    }

    // MARK: - House cusp lines (equal-spaced; cusp 1 at 90°)

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

    // MARK: - Cusp position texts (near center, along cusp line)

    private static void DrawHouseCuspTexts(DrawingContext ctx, Point center,
                                            double outerRadius, WheelPlotData data,
                                            WheelTheme theme)
    {
        if (data.CuspLongitudes.Length < 12) return;
        var r        = outerRadius * 0.20;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, outerRadius);
        var typeface = Typeface.Default;
        var brush    = new SolidColorBrush(theme.CardinalIndicator);

        for (var i = 0; i < 12; i++)
        {
            var angle  = 90.0 + i * 30.0;
            var pt     = WheelGeometry.PointOnCircle(angle, r, center);
            var text   = DrawCusps.CuspPositionText(data.CuspLongitudes[i]);

            // Rotate text perpendicular to the cusp line, legible from outside
            var rotDeg = angle is <= 90.0 or > 270.0
                ? 360.0 - angle
                : 540.0 - angle;

            var ft = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                brush);

            using var _ = ctx.PushTransform(
                Matrix.CreateRotation(rotDeg * Math.PI / 180.0) *
                Matrix.CreateTranslation(pt.X, pt.Y));

            ctx.DrawText(ft, new Point(-ft.Width / 2, -ft.Height / 2));
        }
    }

    // MARK: - Cardinal labels (A / D / M / I at their actual visual positions)

    private static void DrawHouseCardinalLabels(DrawingContext ctx, Point center,
                                                 double outerRadius, WheelPlotData data,
                                                 WheelTheme theme)
    {
        if (data.CuspLongitudes.Length < 12) return;
        var cusps    = data.CuspLongitudes;
        var r        = outerRadius * WheelMetrics.CardinalIndicator;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.CardinalFontFraction, outerRadius);
        var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);
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
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                brush);

            ctx.DrawText(ft, new Point(pt.X - ft.Width / 2, pt.Y - ft.Height / 2));
        }
    }

    // MARK: - Geometry helpers

    private static void FillCircle(DrawingContext ctx, Point center, double radius, IBrush fill)
    {
        var rect = new Rect(center.X - radius, center.Y - radius, radius * 2, radius * 2);
        ctx.DrawEllipse(fill, null, center, radius, radius);
    }

    private static void StrokeCircle(DrawingContext ctx, Point center, double radius, Pen pen)
    {
        ctx.DrawEllipse(null, pen, center, radius, radius);
    }
}
