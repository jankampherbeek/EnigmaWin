// DrawRingWheel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
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
    private const double RingR           = 0.72;
    private const double CuspLabelR      = 0.81;
    private const double PlanetLabelR    = 0.86;
    private const double AspectEndpointR = 0.69;

    private static readonly Color RingColor = Color.FromRgb(0xBF, 0xBF, 0xBF);

    private static readonly HashSet<Aspects> MajorAspects =
    [
        Aspects.Opposition, Aspects.Square, Aspects.Trine,
        Aspects.Sextile,    Aspects.Inconjunct
    ];

    public static void DrawCircle(DrawingContext ctx, Point center, double outerRadius,
                                   WheelTheme? theme = null)
    {
        var r      = outerRadius * RingR;
        var stroke = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerRadius);
        var color  = (theme?.IsBlackWhite ?? false) ? Colors.Black : RingColor;
        var pen    = new Pen(new SolidColorBrush(color), stroke);
        ctx.DrawEllipse(null, pen, center, r, r);
    }

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
                pen = new Pen(new SolidColorBrush(color), minorWidth);
                pen.DashStyle = new DashStyle(new double[] { dashLen, dashLen }, 0);
            }
            ctx.DrawLine(pen, p1, p2);
        }
    }

    private static void DrawPositionLabel(DrawingContext ctx, double longitude, Point pt,
                                           double wheelAngle, double fontSize, Brush brush)
    {
        var posText  = CuspPositionText(longitude);
        var signIdx  = (int)(longitude / 30.0) % 12;
        var glyph    = GlyphSelector.GetGlyphForSign((Signs)(signIdx + 1));

        var glyphSize = fontSize * 1.4;
        var textTyp   = new Typeface("Segoe UI");
        var glyphTyp  = new Typeface("EnigmaAstrology2");

        var ftPos   = MakeFormattedText(posText, fontSize,  textTyp,  brush);
        var ftGlyph = MakeFormattedText(glyph,   glyphSize, glyphTyp, brush);

        var totalW = ftPos.Width + ftGlyph.Width;
        var height = Math.Max(ftPos.Height, ftGlyph.Height);

        double rotDeg = wheelAngle < 180.0 ? 90.0 - wheelAngle : 270.0 - wheelAngle;

        var rad = rotDeg * Math.PI / 180.0;
        var cos = Math.Cos(rad);
        var sin = Math.Sin(rad);
        var transform = new MatrixTransform(new Matrix(cos, sin, -sin, cos, pt.X, pt.Y));
        ctx.PushTransform(transform);

        var startX = -totalW / 2.0;
        var y      = -height / 2.0;
        ctx.DrawText(ftPos,   new Point(startX,                  y));
        ctx.DrawText(ftGlyph, new Point(startX + ftPos.Width,    y + (height - ftGlyph.Height) / 2.0));

        ctx.Pop();
    }

    private static void DrawTextCentered(DrawingContext ctx, string text, Point pt,
                                          double fontSize, Typeface typeface, Brush brush)
    {
        var ft = MakeFormattedText(text, fontSize, typeface, brush);
        ctx.DrawText(ft, new Point(pt.X - ft.Width / 2, pt.Y - ft.Height / 2));
    }

    private static FormattedText MakeFormattedText(string text, double fontSize,
                                                    Typeface typeface, Brush brush) =>
        new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            typeface, fontSize, brush, 1.0);

    private static string CuspPositionText(double longitude)
    {
        var inSign   = longitude % 30.0;
        var totalMin = (int)(Math.Abs(inSign) * 60);
        return $"{totalMin / 60}°{totalMin % 60:D2}'";
    }
}
