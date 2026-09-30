// AltZodiacStartWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.AltZodiacStart.UI;

/// <summary>
/// Draws the radix wheel with the 12-sign ring re-anchored so that its first slice (traditionally Aries 0°)
/// starts at the ecliptic longitude of a user-chosen factor, instead of at the vernal equinox point.
/// House cusps, Ascendant/MC, planet positions and aspects are drawn exactly as in the normal radix wheel;
/// only the sign ring (sectors, separators, glyphs, degree ticks) is shifted, plus a marker showing where the
/// chosen factor, and therefore 0° of the new zodiac, lies.
/// </summary>
public sealed class AltZodiacStartWheelCanvas : FrameworkElement
{
    public static readonly DependencyProperty PlotDataProperty =
        DependencyProperty.Register(nameof(PlotData), typeof(WheelPlotData), typeof(AltZodiacStartWheelCanvas),
            new FrameworkPropertyMetadata(WheelPlotData.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ZodiacStartLongitudeProperty =
        DependencyProperty.Register(nameof(ZodiacStartLongitude), typeof(double), typeof(AltZodiacStartWheelCanvas),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(AltZodiacStartWheelCanvas),
            new FrameworkPropertyMetadata(WheelTheme.Color, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowAspectsProperty =
        DependencyProperty.Register(nameof(ShowAspects), typeof(bool), typeof(AltZodiacStartWheelCanvas),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public WheelPlotData PlotData
    {
        get => (WheelPlotData)GetValue(PlotDataProperty);
        set => SetValue(PlotDataProperty, value);
    }

    /// <summary>Real ecliptic longitude that becomes 0° of the alternative zodiac.</summary>
    public double ZodiacStartLongitude
    {
        get => (double)GetValue(ZodiacStartLongitudeProperty);
        set => SetValue(ZodiacStartLongitudeProperty, value);
    }

    public WheelTheme Theme
    {
        get => (WheelTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    public bool ShowAspects
    {
        get => (bool)GetValue(ShowAspectsProperty);
        set => SetValue(ShowAspectsProperty, value);
    }

    protected override void OnRender(DrawingContext ctx)
    {
        base.OnRender(ctx);

        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        var outerRadius = Math.Min(w, h) / 2.0;
        var center      = new Point(w / 2.0, h / 2.0);
        var data        = PlotData;
        var theme       = Theme;
        // Mundane angle of the new zodiac's 0°; sign i occupies [startAngle + i*30, startAngle + (i+1)*30).
        var startAngle  = WheelGeometry.MundaneAngle(ZodiacStartLongitude, data.AscendantLongitude);

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

        DrawCircles.Draw(ctx, center, outerRadius, theme);
        DrawShiftedSectors(ctx, center, outerRadius, startAngle, theme);
        DrawShiftedSeparators(ctx, center, outerRadius, startAngle, theme);
        DrawShiftedGlyphs(ctx, center, outerRadius, startAngle, theme);
        DrawShiftedDegreeLines(ctx, center, outerRadius, startAngle, theme);

        if (data.HasTime)
        {
            DrawCusps.DrawCuspLines(ctx, center, outerRadius, data, theme);
            DrawCusps.DrawCardinalLines(ctx, center, outerRadius, data, theme);
            DrawCusps.DrawCardinalLabels(ctx, center, outerRadius, data, theme);
            var zodiacStart = ZodiacStartLongitude;
            DrawCusps.DrawCuspTexts(ctx, center, outerRadius, data, theme, cusp =>
                DrawCusps.CuspPositionText(AltZodiacStartOrchestrator.ShiftedLongitude(cusp, zodiacStart)));
        }

        if (ShowAspects)
            DrawAspects.Draw(ctx, center, outerRadius, data, theme);

        DrawPlanets.DrawPlanetConnectLines(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetTexts(ctx, center, outerRadius, data, theme);

        DrawZodiacStartMarker(ctx, center, outerRadius, startAngle, theme);
    }

    private static void DrawShiftedSectors(DrawingContext ctx, Point center, double outerRadius,
                                           double startAngle, WheelTheme theme)
    {
        var innerR = outerRadius * WheelMetrics.OuterHouse;
        var outerR = outerRadius * WheelMetrics.OuterSign;
        for (var i = 0; i < 12; i++)
        {
            var color = theme.SignSectorColor((Signs)(i + 1));
            if (color == Colors.Transparent) continue;
            var from = startAngle + i * 30.0;
            DrawSigns.DrawAnnularSector(ctx, center, innerR, outerR, from, from + 30.0, new SolidColorBrush(color));
        }
    }

    private static void DrawShiftedSeparators(DrawingContext ctx, Point center, double outerRadius,
                                              double startAngle, WheelTheme theme)
    {
        var innerR = outerRadius * WheelMetrics.OuterHouse;
        var outerR = outerRadius * WheelMetrics.OuterSign;
        var pen    = new Pen(new SolidColorBrush(theme.SignSeparator),
                             WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius));
        for (var i = 0; i < 12; i++)
        {
            var angle = startAngle + i * 30.0;
            ctx.DrawLine(pen, WheelGeometry.PointOnCircle(angle, innerR, center),
                              WheelGeometry.PointOnCircle(angle, outerR, center));
        }
    }

    private static void DrawShiftedGlyphs(DrawingContext ctx, Point center, double outerRadius,
                                          double startAngle, WheelTheme theme)
    {
        var glyphRadius = outerRadius * WheelMetrics.SignGlyph;
        var fontSize    = WheelMetrics.FontSize(WheelMetrics.SignGlyphFontFraction, outerRadius);
        var brush       = new SolidColorBrush(theme.SignGlyph);
        for (var i = 0; i < 12; i++)
        {
            var pt    = WheelGeometry.PointOnCircle(startAngle + i * 30.0 + 15.0, glyphRadius, center);
            var glyph = GlyphSelector.GetGlyphForSign((Signs)(i + 1));
            var ft    = new FormattedText(glyph, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                          WheelMetrics.GlyphTypeface, fontSize, brush, 1.0);
            ctx.DrawText(ft, new Point(pt.X - ft.Width / 2, pt.Y - ft.Height / 2));
        }
    }

    private static void DrawShiftedDegreeLines(DrawingContext ctx, Point center, double outerRadius,
                                               double startAngle, WheelTheme theme)
    {
        var startR = outerRadius * WheelMetrics.OuterHouse;
        var shortR = outerRadius * WheelMetrics.Degrees;
        var longR  = outerRadius * WheelMetrics.Degrees5;
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), 1.0);
        for (var i = 0; i < 360; i++)
        {
            var angle = startAngle + i;
            var endR  = i % 5 == 0 ? longR : shortR;
            ctx.DrawLine(pen, WheelGeometry.PointOnCircle(angle, endR, center),
                              WheelGeometry.PointOnCircle(angle, startR, center));
        }
    }

    /// <summary>Radial line across the outer rings at the exact point that defines the new zodiac's 0°.</summary>
    private static void DrawZodiacStartMarker(DrawingContext ctx, Point center, double outerRadius,
                                              double startAngle, WheelTheme theme)
    {
        var pen = new Pen(new SolidColorBrush(theme.CardinalIndicator),
                          WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius) * 1.5);
        ctx.DrawLine(pen, WheelGeometry.PointOnCircle(startAngle, outerRadius * WheelMetrics.OuterHouse, center),
                          WheelGeometry.PointOnCircle(startAngle, outerRadius * WheelMetrics.OuterCircle, center));
    }
}
