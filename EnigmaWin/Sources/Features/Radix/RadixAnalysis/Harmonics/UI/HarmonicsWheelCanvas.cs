// HarmonicsWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

/// <summary>
/// Renders a zodiac-based wheel for harmonics. The harmonic ASC is at the left with equal cusps from it;
/// without a harmonic ASC, 0° Aries is at the left and no cusps are drawn. ASC and MC lines are drawn at
/// their harmonic longitudes.
/// Aspect lines between the harmonic positions are drawn when ShowAspects is true.
/// </summary>
public sealed class HarmonicsWheelCanvas : FrameworkElement
{
    public static readonly DependencyProperty PlotDataProperty =
        DependencyProperty.Register(nameof(PlotData), typeof(WheelPlotData), typeof(HarmonicsWheelCanvas),
            new FrameworkPropertyMetadata(WheelPlotData.Empty,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(HarmonicsWheelCanvas),
            new FrameworkPropertyMetadata(WheelTheme.Color,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowAspectsProperty =
        DependencyProperty.Register(nameof(ShowAspects), typeof(bool), typeof(HarmonicsWheelCanvas),
            new FrameworkPropertyMetadata(true,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public bool ShowAspects
    {
        get => (bool)GetValue(ShowAspectsProperty);
        set => SetValue(ShowAspectsProperty, value);
    }

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

    protected override void OnRender(DrawingContext ctx)
    {
        base.OnRender(ctx);

        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        var outerRadius = Math.Min(w, h) / 2.0;

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));
        Render(ctx, new Point(w / 2.0, h / 2.0), outerRadius, PlotData, Theme, ShowAspects);
    }

    /// <summary>Draws a harmonic wheel with the given center and outer radius.</summary>
    public static void Render(DrawingContext ctx, Point center, double outerRadius,
                              WheelPlotData data, WheelTheme theme, bool showAspects)
    {
        // Harmonic ASC at the left, or 0° Aries if there is no harmonic ASC.
        var asc = data.AscendantLongitude;

        // Harmonic longitudes of ASC and MC (null if not in config or hidden).
        var harmonicAscLong = data.PlanetItems.FirstOrDefault(p => p.Factor == Factors.Ascendant)?.EclipticLongitude;
        var harmonicMcLong  = data.PlanetItems.FirstOrDefault(p => p.Factor == Factors.Mc)?.EclipticLongitude;

        DrawCircles.Draw(ctx, center, outerRadius, theme);
        DrawSigns.DrawElementSectors(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawSignSeparators(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawSignGlyphs(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawDegreeLines(ctx, center, outerRadius, asc, theme);

        if (data.HasTime)
        {
            DrawCusps.DrawCuspLines(ctx, center, outerRadius, data, theme);
            DrawCusps.DrawCuspTexts(ctx, center, outerRadius, data, theme);
        }

        DrawAscMc.DrawAscMcLines(ctx, center, outerRadius, harmonicAscLong, harmonicMcLong, asc, theme);
        DrawAscMc.DrawAscMcLabels(ctx, center, outerRadius, harmonicAscLong, harmonicMcLong, asc, theme);

        if (showAspects)
            DrawAspects.Draw(ctx, center, outerRadius, data, theme);

        DrawPlanets.DrawPlanetConnectLines(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }
}
