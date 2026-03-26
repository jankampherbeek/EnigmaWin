// FrenchWheelCanvas.axaml.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// Canvas that renders a French-style horoscope wheel by overriding Render().
/// In this layout the zodiac sign ring sits inside the house ring and planets
/// are drawn outside the house ring. Degree ticks project outward from the sign ring.
/// Set PlotData, Theme, and ShowAspects before the control is shown.
/// </summary>
public partial class FrenchWheelCanvas : UserControl
{
    public static readonly StyledProperty<WheelPlotData> PlotDataProperty =
        AvaloniaProperty.Register<FrenchWheelCanvas, WheelPlotData>(
            nameof(PlotData), WheelPlotData.Empty);

    public static readonly StyledProperty<WheelTheme> ThemeProperty =
        AvaloniaProperty.Register<FrenchWheelCanvas, WheelTheme>(
            nameof(Theme), WheelTheme.Color);

    public static readonly StyledProperty<bool> ShowAspectsProperty =
        AvaloniaProperty.Register<FrenchWheelCanvas, bool>(
            nameof(ShowAspects), true);

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

    public bool ShowAspects
    {
        get => GetValue(ShowAspectsProperty);
        set => SetValue(ShowAspectsProperty, value);
    }

    public FrenchWheelCanvas()
    {
        InitializeComponent();

        PlotDataProperty.Changed.AddClassHandler<FrenchWheelCanvas>((c, _) => c.InvalidateVisual());
        ThemeProperty.Changed.AddClassHandler<FrenchWheelCanvas>((c, _) => c.InvalidateVisual());
        ShowAspectsProperty.Changed.AddClassHandler<FrenchWheelCanvas>((c, _) => c.InvalidateVisual());
    }

    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);

        var size        = Bounds.Size;
        var diameter    = System.Math.Min(size.Width, size.Height);
        var outerRadius = diameter / 2.0;
        if (outerRadius <= 0) return;

        var center = new Point(size.Width / 2.0, size.Height / 2.0);
        var data   = PlotData;
        var theme  = Theme;
        var asc    = data.AscendantLongitude;

        // White background
        ctx.FillRectangle(Brushes.White, new Rect(size));

        DrawFrenchCircles.Draw(ctx, center, outerRadius, theme);
        DrawFrenchSigns.DrawElementSectors(ctx, center, outerRadius, asc, theme);
        DrawFrenchSigns.DrawSignSeparators(ctx, center, outerRadius, asc, theme);
        DrawFrenchSigns.DrawSignGlyphs(ctx, center, outerRadius, asc, theme);
        DrawFrenchSigns.DrawDegreeLines(ctx, center, outerRadius, asc, theme);

        if (data.HasTime)
        {
            DrawFrenchCusps.DrawCuspLines(ctx, center, outerRadius, data, theme);
            DrawFrenchCusps.DrawHouseNumbers(ctx, center, outerRadius, data, theme);
            DrawFrenchCusps.DrawCardinalGlyphs(ctx, center, outerRadius, data, theme);
        }

        if (ShowAspects)
            DrawAspects.Draw(ctx, center, outerRadius, data, theme, FrenchWheelMetrics.OuterAspect);

        DrawFrenchPlanets.DrawPlanetConnectLines(ctx, center, outerRadius, data, theme);
        DrawFrenchPlanets.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawFrenchPlanets.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }
}
