// DrawRingWheel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Drawing helpers for the Ring-style wheel.
/// Layout: one circle at RingR; cusps radiate from center to the ring;
/// planets sit on the ring; labels appear just outside; aspects inside.
/// </summary>
public static class DrawRingWheel
{
    // MARK: - Layout constants (fraction of outerRadius)
    private const double RingR           = 0.72;  // main circle
    private const double CuspLabelR      = 0.81;  // cusp degree+sign label
    private const double PlanetLabelR    = 0.86;  // planet position label
    private const double AspectEndpointR = 0.69;  // aspect line endpoints, inside ring

    private static readonly Color RingColor = Color.FromRgb(0xBF, 0xBF, 0xBF); // ~0.75 grey

    private static readonly HashSet<Aspects> MajorAspects =
    [
        Aspects.Opposition, Aspects.Square, Aspects.Trine,
        Aspects.Sextile,    Aspects.Inconjunct
    ];

    // MARK: - Circle

    /// <summary>Draws the single ring circle (grey stroke, no fill).</summary>
    public static void DrawCircle(DrawingContext ctx, Point center, double outerRadius,
                                   WheelTheme? theme = null)
    {
        var r      = outerRadius * RingR;
        var stroke = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius);
        var color  = (theme?.IsBlackWhite ?? false) ? Colors.Black : RingColor;
        var pen    = new Pen(new SolidColorBrush(color), stroke);
        ctx.DrawEllipse(null, pen, center, r, r);
    }

    // MARK: - Cusp lines (center → ring)

    /// <summary>Draws radial lines from the center to the ring for each house cusp.</summary>
    public static void DrawCuspLines(DrawingContext ctx, Point center, double outerRadius,
                                      WheelPlotData data, WheelTheme? theme = null)
    {
        var ringR   = outerRadius * RingR;
        var thin    = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction,       outerRadius);
        var thick   = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 2.0, outerRadius);
        var ascLong = data.AscendantLongitude;
        var color   = (theme?.IsBlackWhite ?? false) ? Colors.Black : RingColor;

        for (var i = 0; i < data.CuspLongitudes.Length; i++)
        {
            var angle = WheelGeometry.MundaneAngle(data.CuspLongitudes[i], ascLong);
            var lw    = (i % 3 == 0) ? thick : thin;
            var pen   = new Pen(new SolidColorBrush(color), lw);
            var p2    = WheelGeometry.PointOnCircle(angle, ringR, center);
            ctx.DrawLine(pen, center, p2);
        }
    }

    // MARK: - Cusp labels (deg°min' + sign glyph, outside ring)

    /// <summary>Draws "deg°min' + sign glyph" rotated along the ring for each cusp.</summary>
    public static void DrawCuspLabels(DrawingContext ctx, Point center, double outerRadius,
                                       WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * CuspLabelR;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, outerRadius);
        var ascLong  = data.AscendantLongitude;
        var brush    = new SolidColorBrush(theme.CuspText);

        foreach (var cuspLong in data.CuspLongitudes)
        {
            var angle = WheelGeometry.MundaneAngle(cuspLong, ascLong);
            var pt    = WheelGeometry.PointOnCircle(angle, r, center);
            DrawPositionLabel(ctx, cuspLong, pt, angle, fontSize, brush);
        }
    }

    // MARK: - Intercepted sign glyphs

    /// <summary>
    /// Draws a sign glyph at the midpoint of each sign that contains no cusp (intercepted signs).
    /// </summary>
    public static void DrawInterceptedSignGlyphs(DrawingContext ctx, Point center, double outerRadius,
                                                  WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * CuspLabelR;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction * 1.4, outerRadius);
        var ascLong  = data.AscendantLongitude;
        var brush    = new SolidColorBrush(theme.CuspText);
        var typeface = new Typeface("EnigmaAstrology2");

        var occupiedSigns = new HashSet<int>();
        foreach (var cl in data.CuspLongitudes)
            occupiedSigns.Add((int)(cl / 30.0) % 12);

        for (var signIdx = 0; signIdx < 12; signIdx++)
        {
            if (occupiedSigns.Contains(signIdx)) continue;
            var midLong = signIdx * 30.0 + 15.0;
            var angle   = WheelGeometry.MundaneAngle(midLong, ascLong);
            var pt      = WheelGeometry.PointOnCircle(angle, r, center);
            var glyph   = GlyphSelector.GetGlyphForSign((Signs)(signIdx + 1));
            DrawTextCentered(ctx, glyph, pt, fontSize, typeface, brush);
        }
    }

    // MARK: - Planet glyphs (on the ring)

    /// <summary>Draws the astrological glyph for each planet directly on the ring circle.</summary>
    public static void DrawPlanetGlyphs(DrawingContext ctx, Point center, double outerRadius,
                                         WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * RingR;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PlanetGlyphFontFraction * 1.5, outerRadius);
        var typeface = new Typeface("EnigmaAstrology2");
        var brush    = new SolidColorBrush(theme.PlanetGlyph);

        foreach (var item in data.PlanetItems)
        {
            var pt = WheelGeometry.PointOnCircle(item.PlotAngle, r, center);
            DrawTextCentered(ctx, item.Glyph, pt, fontSize, typeface, brush);
        }
    }

    // MARK: - Planet texts (deg°min' + sign glyph, outside ring)

    /// <summary>Draws "deg°min' + sign glyph" rotated for each planet, outside the ring.</summary>
    public static void DrawPlanetTexts(DrawingContext ctx, Point center, double outerRadius,
                                        WheelPlotData data, WheelTheme? theme = null)
    {
        theme ??= WheelTheme.Color;
        var r        = outerRadius * PlanetLabelR;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, outerRadius);
        var brush    = new SolidColorBrush(theme.PlanetText);

        foreach (var item in data.PlanetItems)
        {
            var pa = item.PlotAngle;
            var pt = WheelGeometry.PointOnCircle(pa, r, center);
            DrawPositionLabel(ctx, item.EclipticLongitude, pt, pa, fontSize, brush);
        }
    }

    // MARK: - Aspect lines

    /// <summary>
    /// Draws aspect lines inside the ring.
    /// Major aspects (opposition, square, trine, sextile, inconjunct) are drawn solid and thicker;
    /// minor aspects are drawn dashed and thin.
    /// </summary>
    public static void DrawAspectLines(DrawingContext ctx, Point center, double outerRadius,
                                        WheelPlotData data, WheelTheme? theme = null)
    {
        if (data.AspectItems.Length == 0) return;
        theme ??= WheelTheme.Color;

        var r          = outerRadius * AspectEndpointR;
        var majorWidth = WheelMetrics.StrokeWidth(WheelMetrics.AspectLineFraction * 2.0, outerRadius);
        var minorWidth = WheelMetrics.StrokeWidth(WheelMetrics.ConnectLineFraction,      outerRadius);
        var dashLen    = outerRadius * 0.04;

        foreach (var item in data.AspectItems)
        {
            var p1    = WheelGeometry.PointOnCircle(item.Angle1, r, center);
            var p2    = WheelGeometry.PointOnCircle(item.Angle2, r, center);
            var ac    = theme.AspectLineColor(item.Color);
            var color = Color.FromArgb((byte)(WheelMetrics.AspectOpacity * 255), ac.R, ac.G, ac.B);

            Pen pen;
            if (MajorAspects.Contains(item.Aspect))
            {
                pen = new Pen(new SolidColorBrush(color), majorWidth);
            }
            else
            {
                pen = new Pen(new SolidColorBrush(color), minorWidth,
                    new DashStyle([dashLen, dashLen], 0));
            }
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // MARK: - Helpers

    /// <summary>
    /// Draws "deg°min'" in the system font followed by the sign glyph in EnigmaAstrology2,
    /// rotated to read radially outward at the given wheel angle.
    /// </summary>
    private static void DrawPositionLabel(DrawingContext ctx, double longitude, Point pt,
                                           double wheelAngle, double fontSize, IBrush brush)
    {
        var posText  = CuspPositionText(longitude);
        var signIdx  = (int)(longitude / 30.0) % 12;
        var glyph    = GlyphSelector.GetGlyphForSign((Signs)(signIdx + 1));

        var glyphSize   = fontSize * 1.4;
        var textTyp     = Typeface.Default;
        var glyphTyp    = new Typeface("EnigmaAstrology2");

        var ftPos   = MakeFormattedText(posText, fontSize,  textTyp,  brush);
        var ftGlyph = MakeFormattedText(glyph,   glyphSize, glyphTyp, brush);

        // Total width of the combined label
        var totalW = ftPos.Width + ftGlyph.Width;
        var height = Math.Max(ftPos.Height, ftGlyph.Height);

        // Rotation: text reads outward
        double rotDeg = wheelAngle < 180.0 ? 90.0 - wheelAngle : 270.0 - wheelAngle;

        using var _ = ctx.PushTransform(
            Matrix.CreateRotation(rotDeg * Math.PI / 180.0) *
            Matrix.CreateTranslation(pt.X, pt.Y));

        // Centre the combined label on pt
        var startX = -totalW / 2.0;
        var y      = -height / 2.0;
        ctx.DrawText(ftPos,   new Point(startX,                  y));
        ctx.DrawText(ftGlyph, new Point(startX + ftPos.Width,    y + (height - ftGlyph.Height) / 2.0));
    }

    private static void DrawTextCentered(DrawingContext ctx, string text, Point pt,
                                          double fontSize, Typeface typeface, IBrush brush)
    {
        var ft = MakeFormattedText(text, fontSize, typeface, brush);
        ctx.DrawText(ft, new Point(pt.X - ft.Width / 2, pt.Y - ft.Height / 2));
    }

    private static FormattedText MakeFormattedText(string text, double fontSize,
                                                    Typeface typeface, IBrush brush) =>
        new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            typeface, fontSize, brush);

    /// <summary>Formats a longitude as "deg°min'" within the sign (e.g. "15°23'").</summary>
    private static string CuspPositionText(double longitude)
    {
        var inSign   = longitude % 30.0;
        var totalMin = (int)(Math.Abs(inSign) * 60);
        return $"{totalMin / 60}°{totalMin % 60:D2}'";
    }
}
