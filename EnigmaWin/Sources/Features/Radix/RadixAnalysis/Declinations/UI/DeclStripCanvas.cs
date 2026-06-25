// DeclStripCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public sealed class DeclStripCanvas : FrameworkElement
{
    private const double BarWidthFraction    = 0.12;
    private const double BarHeightFraction   = 0.94;
    private const double LabelAreaFraction   = 0.03;
    private const double GlyphMarginFraction = 0.06;

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(IReadOnlyList<DeclStripItem>),
            typeof(DeclStripCanvas),
            new FrameworkPropertyMetadata(
                (IReadOnlyList<DeclStripItem>)Array.Empty<DeclStripItem>(),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ObliquityProperty =
        DependencyProperty.Register(nameof(Obliquity), typeof(double), typeof(DeclStripCanvas),
            new FrameworkPropertyMetadata(23.45, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsBlackWhiteProperty =
        DependencyProperty.Register(nameof(IsBlackWhite), typeof(bool), typeof(DeclStripCanvas),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public IReadOnlyList<DeclStripItem> Items
    {
        get => (IReadOnlyList<DeclStripItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public double Obliquity
    {
        get => (double)GetValue(ObliquityProperty);
        set => SetValue(ObliquityProperty, value);
    }

    public bool IsBlackWhite
    {
        get => (bool)GetValue(IsBlackWhiteProperty);
        set => SetValue(IsBlackWhiteProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (availableSize.Width > 0 && !double.IsInfinity(availableSize.Width))
        {
            var h = availableSize.Width / 0.55;
            if (!double.IsInfinity(availableSize.Height))
                h = Math.Min(h, availableSize.Height);
            return new Size(availableSize.Width, h);
        }
        return new Size(160, 290);
    }

    protected override void OnRender(DrawingContext ctx)
    {
        var w = ActualWidth;
        var h = ActualHeight;
        if (w < 1 || h < 1) return;

        var bw = IsBlackWhite;

        var inBoundsColor = bw ? Color.FromRgb(0xF2, 0xF2, 0xF2) : Color.FromRgb(0xE1, 0xF5, 0xFB);
        var oobColor      = bw ? Color.FromRgb(0xD9, 0xD9, 0xD9) : Color.FromRgb(0xC2, 0xE3, 0xF5);
        var barColor      = bw ? Color.FromRgb(0xCC, 0xCC, 0xCC) : Color.FromRgb(0xF0, 0xE8, 0xB3);
        var lineColor     = bw ? Color.FromRgb(0x66, 0x66, 0x66) : Color.FromRgb(0x00, 0x8C, 0x8C);
        var glyphColor    = bw ? Colors.Black                     : Color.FromRgb(0x2E, 0x4F, 0x87);
        var labelColor    = bw ? Colors.Black                     : Color.FromRgb(0x2E, 0x4F, 0x87);
        var degLineColor  = bw ? Color.FromRgb(0x8C, 0x8C, 0x8C) : Color.FromRgb(0xB8, 0xAD, 0x70);
        var oblColor      = bw ? Colors.Black                     : Colors.OrangeRed;

        var items = Items;
        var maxAbsDecl = items.Count > 0 ? items.Max(i => Math.Abs(i.Declination)) : 0;
        var declRange  = 30;
        if (maxAbsDecl > declRange)
        {
            var extra = (int)(maxAbsDecl - declRange);
            declRange += (extra / 5 + 1) * 5;
        }
        var degreesCount = (double)declRange;

        var labelAreaH = h * LabelAreaFraction;
        var barH       = h * BarHeightFraction;
        var barW       = w * BarWidthFraction;
        var barX       = (w - barW) / 2.0;
        var barTopY    = h - barH - labelAreaH;
        var degreeH    = barH / degreesCount;
        var oblY       = barTopY + (degreesCount - Obliquity) * degreeH;

        ctx.DrawRectangle(new SolidColorBrush(oobColor),    null, new Rect(0,    0,    w, oblY));
        ctx.DrawRectangle(new SolidColorBrush(inBoundsColor), null, new Rect(0, oblY, w, h - oblY));
        ctx.DrawRectangle(new SolidColorBrush(barColor),    null, new Rect(barX, barTopY, barW, barH));

        var degLinePen  = new Pen(new SolidColorBrush(degLineColor), 0.75);
        var labelBrush  = new SolidColorBrush(labelColor);
        var labelSize   = Math.Max(8, Math.Min(14, w * 0.028));

        for (var i = 0; i <= declRange; i++)
        {
            var y      = barTopY + i * degreeH;
            var degVal = declRange - i;
            ctx.DrawLine(degLinePen, new Point(barX, y), new Point(barX + barW, y));

            if (degVal < declRange)
            {
                var ft = new FormattedText(
                    $"{degVal}", CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), labelSize, labelBrush, 1.0);
                ctx.DrawText(ft, new Point(barX + barW * 0.12, y - degreeH * 0.85));
            }
        }

        var oblPen = new Pen(new SolidColorBrush(oblColor), 1.0) { DashStyle = new DashStyle(new double[] { 4, 3 }, 0) };
        ctx.DrawLine(oblPen, new Point(0, oblY), new Point(w, oblY));

        var glyphSize  = Math.Max(10, Math.Min(24, w * 0.06));
        var glyphFont  = (Application.Current?.TryFindResource("GlyphFont") as FontFamily)
                         ?? new FontFamily(new Uri("pack://application:,,,/"), "/Resources/Fonts/#EnigmaAstrology3");
        var glyphTypeface = new Typeface(glyphFont, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var glyphBrush = new SolidColorBrush(glyphColor);
        var lineAlpha  = (byte)128;
        var lineBrush  = new SolidColorBrush(Color.FromArgb(lineAlpha, lineColor.R, lineColor.G, lineColor.B));
        var glyphLinePen = new Pen(lineBrush, 0.75);

        var margin     = w * 0.045;
        var northBaseX = barX - w * GlyphMarginFraction;
        var southBaseX = barX + barW + w * GlyphMarginFraction;

        var northItems = items.Where(i => i.Declination >= 0).OrderBy(i => i.Declination).ToList();
        var southItems = items.Where(i => i.Declination <  0).OrderBy(i => Math.Abs(i.Declination)).ToList();

        DrawGlyphs(ctx, northItems, northBaseX, true,  barX,         barTopY, degreeH, degreesCount,
                   margin, glyphTypeface, glyphSize, glyphBrush, glyphLinePen);
        DrawGlyphs(ctx, southItems, southBaseX, false, barX + barW,  barTopY, degreeH, degreesCount,
                   margin, glyphTypeface, glyphSize, glyphBrush, glyphLinePen);

        var dirSize  = Math.Max(9, Math.Min(13, w * 0.03));
        var dirY     = h - labelAreaH * 0.5;
        var dirTyp   = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
        var northFt  = new FormattedText("N", CultureInfo.InvariantCulture,
                           FlowDirection.LeftToRight, dirTyp, dirSize, labelBrush, 1.0);
        var southFt  = new FormattedText("S", CultureInfo.InvariantCulture,
                           FlowDirection.LeftToRight, dirTyp, dirSize, labelBrush, 1.0);
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
        Brush glyphBrush,
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

            var ft = new FormattedText(item.Glyph, CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, glyphFont, glyphSize, glyphBrush, 1.0);
            ctx.DrawText(ft, new Point(xPos - ft.Width / 2, y - ft.Height / 2));

            lastDecl = absDecl;
        }
    }
}
