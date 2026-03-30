// MidpointTreeControl.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints.UI;

/// <summary>
/// Draws a single midpoint tree: a planet glyph at top, a vertical stem,
/// and horizontal crossbars with factor glyphs left and right.
/// </summary>
public sealed class MidpointTreeControl : Control
{
    // Layout constants (matching the Apple reference)
    private const double GlyphSize = 16;
    private const double TreeW     = 72;
    private const double BarStep   = 26;
    private const double TopPad    = 28;
    private const double BarHalf   = 18;
    private const double GlyphW    = 22;

    public static readonly StyledProperty<Factors> MatchingFactorProperty =
        AvaloniaProperty.Register<MidpointTreeControl, Factors>(nameof(MatchingFactor));

    public static readonly StyledProperty<IReadOnlyList<(Factors F1, Factors F2)>> PairsProperty =
        AvaloniaProperty.Register<MidpointTreeControl, IReadOnlyList<(Factors, Factors)>>(
            nameof(Pairs), defaultValue: []);

    public Factors MatchingFactor
    {
        get => GetValue(MatchingFactorProperty);
        set => SetValue(MatchingFactorProperty, value);
    }

    public IReadOnlyList<(Factors F1, Factors F2)> Pairs
    {
        get => GetValue(PairsProperty);
        set => SetValue(PairsProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var h = TopPad + Pairs.Count * BarStep + BarStep / 2 + 8;
        return new Size(TreeW, h);
    }

    public override void Render(DrawingContext context)
    {
        var pairs = Pairs;
        var stemX = TreeW / 2;

        var glyphTypeface = GetGlyphTypeface();
        var pen = new Pen(Foreground ?? Brushes.Black, 1);

        // Planet glyph centred on stem
        DrawGlyph(context, glyphTypeface,
            GlyphSelector.GetGlyphForFactor(MatchingFactor),
            stemX, TopPad / 2);

        // Vertical stem
        var stemTop    = TopPad;
        var stemBottom = TopPad + pairs.Count * BarStep;
        context.DrawLine(pen,
            new Point(stemX, stemTop),
            new Point(stemX, stemBottom));

        // Crossbars with factor glyphs
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

    private IBrush? Foreground =>
        this.FindResource("SystemControlForegroundBaseHighBrush") as IBrush
        ?? Brushes.Black;

    private static Typeface GetGlyphTypeface()
    {
        if (Application.Current?.TryFindResource("GlyphFont", out var raw) == true
            && raw is FontFamily ff)
            return new Typeface(ff);
        return Typeface.Default;
    }

    private static void DrawGlyph(DrawingContext ctx, Typeface typeface, string glyph, double cx, double cy)
    {
        var ft = new FormattedText(
            glyph,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            GlyphSize,
            Brushes.Black);

        // centre the glyph on (cx, cy)
        ctx.DrawText(ft, new Point(cx - ft.Width / 2, cy - ft.Height / 2));
    }
}
