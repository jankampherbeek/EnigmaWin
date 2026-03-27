// RingWheelCanvas.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// Canvas that renders a Ring-style horoscope wheel by overriding Render().
/// One circle shows all house cusps as spokes from the centre; planets sit on the ring;
/// position labels and aspect lines are drawn around the ring.
/// Set PlotData, Theme, and ShowAspects before the control is shown.
/// </summary>
public partial class RingWheelCanvas : UserControl
{
    public static readonly StyledProperty<WheelPlotData> PlotDataProperty =
        AvaloniaProperty.Register<RingWheelCanvas, WheelPlotData>(
            nameof(PlotData), WheelPlotData.Empty);

    public static readonly StyledProperty<WheelTheme> ThemeProperty =
        AvaloniaProperty.Register<RingWheelCanvas, WheelTheme>(
            nameof(Theme), WheelTheme.Color);

    public static readonly StyledProperty<bool> ShowAspectsProperty =
        AvaloniaProperty.Register<RingWheelCanvas, bool>(
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

    public RingWheelCanvas()
    {
        InitializeComponent();

        PlotDataProperty.Changed.AddClassHandler<RingWheelCanvas>((c, _) => c.InvalidateVisual());
        ThemeProperty.Changed.AddClassHandler<RingWheelCanvas>((c, _) => c.InvalidateVisual());
        ShowAspectsProperty.Changed.AddClassHandler<RingWheelCanvas>((c, _) => c.InvalidateVisual());
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

        ctx.FillRectangle(Brushes.White, new Rect(size));

        DrawRingWheel.DrawCircle(ctx, center, outerRadius, theme);

        if (data.HasTime)
        {
            DrawRingWheel.DrawCuspLines(ctx, center, outerRadius, data, theme);
            DrawRingWheel.DrawCuspLabels(ctx, center, outerRadius, data, theme);
            DrawRingWheel.DrawInterceptedSignGlyphs(ctx, center, outerRadius, data, theme);
        }

        if (ShowAspects)
            DrawRingWheel.DrawAspectLines(ctx, center, outerRadius, data, theme);

        DrawRingWheel.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawRingWheel.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }
}
