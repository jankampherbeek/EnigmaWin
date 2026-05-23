// MidpointTreeControl.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints.UI;

/// <summary>
/// Draws a single midpoint tree: a planet glyph at top, a vertical stem,
/// and horizontal crossbars with factor glyphs left and right.
/// </summary>
public sealed class MidpointTreeControl : FrameworkElement
{
    private const double GlyphSize = 16;
    private const double TreeW     = 72;
    private const double BarStep   = 26;
    private const double TopPad    = 28;
    private const double BarHalf   = 18;
    private const double GlyphW    = 22;

    public static readonly DependencyProperty MatchingFactorProperty =
        DependencyProperty.Register(nameof(MatchingFactor), typeof(Factors), typeof(MidpointTreeControl),
            new PropertyMetadata(default(Factors), (d, _) => ((MidpointTreeControl)d).InvalidateVisual()));

    public static readonly DependencyProperty PairsProperty =
        DependencyProperty.Register(nameof(Pairs), typeof(IReadOnlyList<(Factors F1, Factors F2)>),
            typeof(MidpointTreeControl),
            new PropertyMetadata(new List<(Factors, Factors)>(), (d, _) =>
            {
                var c = (MidpointTreeControl)d;
                c.InvalidateMeasure();
                c.InvalidateVisual();
            }));

    public Factors MatchingFactor
    {
        get => (Factors)GetValue(MatchingFactorProperty);
        set => SetValue(MatchingFactorProperty, value);
    }

    public IReadOnlyList<(Factors F1, Factors F2)> Pairs
    {
        get => (IReadOnlyList<(Factors, Factors)>)GetValue(PairsProperty);
        set => SetValue(PairsProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var h = TopPad + Pairs.Count * BarStep + BarStep / 2 + 8;
        return new Size(TreeW, h);
    }

    protected override void OnRender(DrawingContext context)
    {
        var pairs = Pairs;
        var stemX = TreeW / 2;

        var glyphTypeface = GetGlyphTypeface();
        var pen = new Pen(Brushes.Black, 1);

        DrawGlyph(context, glyphTypeface,
            GlyphSelector.GetGlyphForFactor(MatchingFactor),
            stemX, TopPad / 2);

        var stemTop    = TopPad;
        var stemBottom = TopPad + pairs.Count * BarStep;
        context.DrawLine(pen,
            new Point(stemX, stemTop),
            new Point(stemX, stemBottom));

        for (var i = 0; i < pairs.Count; i++)
        {
            var barY = TopPad + i * BarStep + BarStep / 2;
            context.DrawLine(pen,
                new Point(stemX - BarHalf, barY),
                new Point(stemX + BarHalf, barY));

            DrawGlyph(context, glyphTypeface,
                GlyphSelector.GetGlyphForFactor(pairs[i].F1),
                stemX - BarHalf - GlyphW / 2, barY);
            DrawGlyph(context, glyphTypeface,
                GlyphSelector.GetGlyphForFactor(pairs[i].F2),
                stemX + BarHalf + GlyphW / 2, barY);
        }
    }

    private static Typeface GetGlyphTypeface()
    {
        if (Application.Current?.TryFindResource("GlyphFont") is FontFamily ff)
            return new Typeface(ff, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        return new Typeface("Segoe UI");
    }

    private static void DrawGlyph(DrawingContext ctx, Typeface typeface, string glyph, double cx, double cy)
    {
        var ft = new FormattedText(
            glyph,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            GlyphSize,
            Brushes.Black,
            1.0);

        ctx.DrawText(ft, new Point(cx - ft.Width / 2, cy - ft.Height / 2));
    }
}
