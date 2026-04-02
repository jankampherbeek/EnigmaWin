// HarmonicsWheelCanvas.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

/// <summary>
/// Canvas that renders a zodiac-based wheel for harmonics.
/// Like ZodiacWheelCanvas but:
///   - 0° Aries at left (AscendantLongitude=0, set by VM)
///   - No aspects, no house cusps, no cardinal lines or labels
/// </summary>
public partial class HarmonicsWheelCanvas : UserControl
{
    public static readonly StyledProperty<WheelPlotData> PlotDataProperty =
        AvaloniaProperty.Register<HarmonicsWheelCanvas, WheelPlotData>(
            nameof(PlotData), WheelPlotData.Empty);

    public static readonly StyledProperty<WheelTheme> ThemeProperty =
        AvaloniaProperty.Register<HarmonicsWheelCanvas, WheelTheme>(
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

    public HarmonicsWheelCanvas()
    {
        InitializeComponent();

        PlotDataProperty.Changed.AddClassHandler<HarmonicsWheelCanvas>((c, _) => c.InvalidateVisual());
        ThemeProperty.Changed.AddClassHandler<HarmonicsWheelCanvas>((c, _) => c.InvalidateVisual());
    }

    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);

        var size        = Bounds.Size;
        var diameter    = Math.Min(size.Width, size.Height);
        var outerRadius = diameter / 2.0;
        if (outerRadius <= 0) return;

        var center = new Point(size.Width / 2.0, size.Height / 2.0);
        var data   = PlotData;
        var theme  = Theme;

        // AscendantLongitude is already 0 (EffectiveData applied by VM)
        // so 0° Aries is at the left (9 o'clock position)
        var asc = data.AscendantLongitude;

        ctx.FillRectangle(Brushes.White, new Rect(size));

        DrawCircles.Draw(ctx, center, outerRadius, theme);
        DrawSigns.DrawElementSectors(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawSignSeparators(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawSignGlyphs(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawDegreeLines(ctx, center, outerRadius, asc, theme);

        // No cardinal lines, no aspects — planets (including harmonic ASC/MC glyphs) only
        DrawPlanets.DrawPlanetConnectLines(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }

}
