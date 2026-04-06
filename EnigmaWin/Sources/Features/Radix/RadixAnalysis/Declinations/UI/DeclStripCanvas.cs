// DeclStripCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

/// <summary>
/// Custom Avalonia control that draws the vertical declination strip diagram.
///
/// Layout (top = north pole, bottom = south pole):
/// - Background: out-of-bounds region (blue tint above obliquity), in-bounds region (cyan tint)
/// - Central vertical bar with horizontal degree lines
/// - Factor glyphs on the left side (north, positive declination) with a line to the bar
/// - Factor glyphs on the right side (south, negative declination) with a line to the bar
/// - Degree labels inside the bar
/// - "N" / "S" direction labels at the bottom
/// </summary>
public sealed class DeclStripCanvas : Control
{
    // ── Styled properties ─────────────────────────────────────────────────

    public static readonly StyledProperty<IReadOnlyList<DeclStripItem>> ItemsProperty =
        AvaloniaProperty.Register<DeclStripCanvas, IReadOnlyList<DeclStripItem>>(nameof(Items), []);

    public static readonly StyledProperty<double> ObliquityProperty =
        AvaloniaProperty.Register<DeclStripCanvas, double>(nameof(Obliquity), 23.45);

    public static readonly StyledProperty<bool> IsBlackWhiteProperty =
        AvaloniaProperty.Register<DeclStripCanvas, bool>(nameof(IsBlackWhite), false);

    public IReadOnlyList<DeclStripItem> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public double Obliquity
    {
        get => GetValue(ObliquityProperty);
        set => SetValue(ObliquityProperty, value);
    }

    public bool IsBlackWhite
    {
        get => GetValue(IsBlackWhiteProperty);
        set => SetValue(IsBlackWhiteProperty, value);
    }

    static DeclStripCanvas()
    {
        AffectsRender<DeclStripCanvas>(ItemsProperty, ObliquityProperty, IsBlackWhiteProperty);
    }

    // ── Layout fractions (mirror the Apple reference) ─────────────────────

    private const double BarWidthFraction    = 0.12;
    private const double BarHeightFraction   = 0.94;
    private const double LabelAreaFraction   = 0.03;
    private const double GlyphMarginFraction = 0.06;

    // ── Rendering ─────────────────────────────────────────────────────────

    public override void Render(DrawingContext ctx)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w < 1 || h < 1) return;

        var bw = IsBlackWhite;

        // Colours
        var inBoundsColor = bw ? Color.FromRgb(0xF2, 0xF2, 0xF2) : Color.FromRgb(0xE1, 0xF5, 0xFB);
        var oobColor      = bw ? Color.FromRgb(0xD9, 0xD9, 0xD9) : Color.FromRgb(0xC2, 0xE3, 0xF5);
        var barColor      = bw ? Color.FromRgb(0xCC, 0xCC, 0xCC) : Color.FromRgb(0xF0, 0xE8, 0xB3);
        var lineColor     = bw ? Color.FromRgb(0x66, 0x66, 0x66) : Color.FromRgb(0x00, 0x8C, 0x8C);
        var glyphColor    = bw ? Colors.Black                     : Color.FromRgb(0x2E, 0x4F, 0x87);
        var labelColor    = bw ? Colors.Black                     : Color.FromRgb(0x2E, 0x4F, 0x87);
        var degLineColor  = bw ? Color.FromRgb(0x8C, 0x8C, 0x8C) : Color.FromRgb(0xB8, 0xAD, 0x70);
        var oblColor      = bw ? Colors.Black                     : Colors.OrangeRed;

        // Determine degree range: at least 30°, expanded in 5° steps
        var maxAbsDecl = Items.Count > 0 ? Items.Max(i => Math.Abs(i.Declination)) : 0;
        var declRange = 30;
        if (maxAbsDecl > declRange)
        {
            var extra = (int)(maxAbsDecl - declRange);
            var steps = extra / 5 + 1;
            declRange += steps * 5;
        }
        var degreesCount = (double)declRange;

        // Geometry
        var labelAreaH = h * LabelAreaFraction;
        var barH       = h * BarHeightFraction;
        var barW       = w * BarWidthFraction;
        var barX       = (w - barW) / 2.0;
        var barTopY    = h - barH - labelAreaH;
        var degreeH    = barH / degreesCount;

        // Obliquity line y
        var oblY = barTopY + (degreesCount - Obliquity) * degreeH;

        // ── Background ───────────────────────────────────────────────────
        ctx.FillRectangle(new SolidColorBrush(oobColor),    new Rect(0,    0,    w, oblY));
        ctx.FillRectangle(new SolidColorBrush(inBoundsColor), new Rect(0, oblY, w, h - oblY));
        ctx.FillRectangle(new SolidColorBrush(barColor),    new Rect(barX, barTopY, barW, barH));

        // ── Degree lines + labels inside bar ─────────────────────────────
        var degLinePen  = new Pen(new SolidColorBrush(degLineColor), 0.75);
        var labelBrush  = new SolidColorBrush(labelColor);
        var labelSize   = Math.Max(8, Math.Min(14, w * 0.028));

        for (var i = 0; i <= declRange; i++)
        {
            var y       = barTopY + i * degreeH;
            var degVal  = declRange - i;

            ctx.DrawLine(degLinePen, new Point(barX, y), new Point(barX + barW, y));

            if (degVal < declRange)
            {
                var ft = new FormattedText(
                    $"{degVal}",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    labelSize,
                    labelBrush);
                ctx.DrawText(ft, new Point(barX + barW * 0.12, y - degreeH * 0.85));
            }
        }

        // ── Obliquity indicator (dashed) ─────────────────────────────────
        var oblPen = new Pen(new SolidColorBrush(oblColor), 1.0, new DashStyle([4, 3], 0));
        ctx.DrawLine(oblPen, new Point(0, oblY), new Point(w, oblY));

        // ── Factor glyphs + position lines ───────────────────────────────
        var glyphSize   = Math.Max(10, Math.Min(24, w * 0.06));
        var glyphFont   = new Typeface("avares://EnigmaWin/Resources/Fonts#EnigmaAstrology2");
        var glyphBrush  = new SolidColorBrush(glyphColor);
        var lineBrush   = new SolidColorBrush(Color.FromArgb(128, lineColor.R, lineColor.G, lineColor.B));
        var glyphLinePen = new Pen(lineBrush, 0.75);

        var margin       = w * 0.045;
        var northBaseX   = barX - w * GlyphMarginFraction;
        var southBaseX   = barX + barW + w * GlyphMarginFraction;

        var northItems = Items.Where(i => i.Declination >= 0).OrderBy(i => i.Declination).ToList();
        var southItems = Items.Where(i => i.Declination < 0).OrderBy(i => Math.Abs(i.Declination)).ToList();

        DrawGlyphs(ctx, northItems, northBaseX, isNorth: true,
                   barEdgeX: barX, barTopY: barTopY, degreeH: degreeH, degreesCount: degreesCount,
                   margin: margin, glyphFont: glyphFont, glyphSize: glyphSize,
                   glyphBrush: glyphBrush, linePen: glyphLinePen);

        DrawGlyphs(ctx, southItems, southBaseX, isNorth: false,
                   barEdgeX: barX + barW, barTopY: barTopY, degreeH: degreeH, degreesCount: degreesCount,
                   margin: margin, glyphFont: glyphFont, glyphSize: glyphSize,
                   glyphBrush: glyphBrush, linePen: glyphLinePen);

        // ── N / S direction labels ────────────────────────────────────────
        var dirSize  = Math.Max(9, Math.Min(13, w * 0.03));
        var dirY     = h - labelAreaH * 0.5;
        var northFt = new FormattedText("N", System.Globalization.CultureInfo.InvariantCulture,
                           FlowDirection.LeftToRight, new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.SemiBold),
                           dirSize, labelBrush);
        var southFt = new FormattedText("S", System.Globalization.CultureInfo.InvariantCulture,
                           FlowDirection.LeftToRight, new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.SemiBold),
                           dirSize, labelBrush);
        ctx.DrawText(northFt, new Point(barX - w * 0.10, dirY - northFt.Height / 2));
        ctx.DrawText(southFt, new Point(barX + barW + w * 0.04, dirY - southFt.Height / 2));
    }

    private static void DrawGlyphs(
        DrawingContext ctx,
        List<DeclStripItem> items,
        double baseX,
        bool isNorth,
        double barEdgeX,
        double barTopY,
        double degreeH,
        double degreesCount,
        double margin,
        Typeface glyphFont,
        double glyphSize,
        IBrush glyphBrush,
        Pen linePen)
    {
        double lastDecl     = -1000;
        int    marginFactor = 0;

        foreach (var item in items)
        {
            var absDecl = Math.Abs(item.Declination);
            var y       = barTopY + (degreesCount - absDecl) * degreeH;

            if ((absDecl - lastDecl) < 0.5) marginFactor++;
            else                             marginFactor = 0;

            var xPos = isNorth
                ? baseX - marginFactor * margin
                : baseX + marginFactor * margin;

            ctx.DrawLine(linePen, new Point(xPos, y), new Point(barEdgeX, y));

            var ft = new FormattedText(
                item.Glyph,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                glyphFont,
                glyphSize,
                glyphBrush);
            ctx.DrawText(ft, new Point(xPos - ft.Width / 2, y - ft.Height / 2));

            lastDecl = absDecl;
        }
    }

    // ── Desired size ──────────────────────────────────────────────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        // Portrait strip aspect ratio ~0.55 (width : height)
        if (availableSize.Width > 0 && !double.IsInfinity(availableSize.Width))
        {
            var h = availableSize.Width / 0.55;
            if (!double.IsInfinity(availableSize.Height))
                h = Math.Min(h, availableSize.Height);
            return new Size(availableSize.Width, h);
        }
        return new Size(160, 290);
    }
}
