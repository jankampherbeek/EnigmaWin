// AspectGridControl.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects.UI;

/// <summary>
/// Draws an aspect grid (aspectentrap): a lower-triangle matrix where the diagonal
/// shows factor glyphs and cells show the aspect glyph (coloured) when an aspect exists.
/// </summary>
public partial class AspectGridControl : UserControl
{
    private const double CellSize      = 30;
    private const double GlyphFontSize = 16;
    private const double BorderOpacity = 0.35;

    public record AspectCell(int LoFactor, int HiFactor, string Glyph, Color Color);

    private List<Factors>    _factors = [];
    private List<AspectCell> _cells   = [];
    private FontFamily?  _glyphFont;

    public AspectGridControl()
    {
        InitializeComponent();
    }

    public void SetData(List<Factors> factors, List<AspectCell> cells)
    {
        _factors   = factors;
        _cells     = cells;
        _glyphFont ??= TryResolveGlyphFont();
        Rebuild();
    }

    private void Rebuild()
    {
        GridCanvas.Children.Clear();

        var n = _factors.Count;
        if (n == 0) return;

        GridCanvas.Width  = n * CellSize;
        GridCanvas.Height = n * CellSize;

        var lookup = new Dictionary<(int, int), AspectCell>();
        foreach (var cell in _cells)
            lookup[(cell.LoFactor, cell.HiFactor)] = cell;

        var borderBrush = new SolidColorBrush(Colors.Gray) { Opacity = BorderOpacity };

        for (var row = 0; row < n; row++)
        {
            for (var col = 0; col < row; col++)
            {
                var border = MakeCellBorder(borderBrush);
                Canvas.SetLeft(border, col * CellSize);
                Canvas.SetTop(border,  row * CellSize);

                var loRaw = Math.Min((int)_factors[row], (int)_factors[col]);
                var hiRaw = Math.Max((int)_factors[row], (int)_factors[col]);

                if (lookup.TryGetValue((loRaw, hiRaw), out var aspectCell))
                {
                    var glyph = MakeGlyphText(aspectCell.Glyph, GlyphFontSize - 2, new SolidColorBrush(aspectCell.Color));
                    border.Child = CenterIn(glyph);
                }

                GridCanvas.Children.Add(border);
            }

            var diagBorder = MakeCellBorder(borderBrush);
            Canvas.SetLeft(diagBorder, row * CellSize);
            Canvas.SetTop(diagBorder,  row * CellSize);

            var factorGlyph = MakeGlyphText(
                GlyphSelector.GetGlyphForFactor(_factors[row]),
                GlyphFontSize,
                null);
            diagBorder.Child = CenterIn(factorGlyph);
            GridCanvas.Children.Add(diagBorder);
        }
    }

    private Border MakeCellBorder(Brush borderBrush) => new()
    {
        Width           = CellSize,
        Height          = CellSize,
        BorderBrush     = borderBrush,
        BorderThickness = new Thickness(0.5),
    };

    private TextBlock MakeGlyphText(string text, double fontSize, Brush? foreground)
    {
        var tb = new TextBlock
        {
            Text       = text,
            FontSize   = fontSize,
            FontFamily = _glyphFont ?? new FontFamily("Segoe UI"),
        };
        if (foreground != null) tb.Foreground = foreground;
        return tb;
    }

    private static ContentControl CenterIn(UIElement child) => new()
    {
        Content                    = child,
        HorizontalContentAlignment = HorizontalAlignment.Center,
        VerticalContentAlignment   = VerticalAlignment.Center,
        Width                      = CellSize,
        Height                     = CellSize,
    };

    private static FontFamily TryResolveGlyphFont() =>
        new(new Uri("pack://application:,,,/"), "/Resources/Fonts/#EnigmaAstrology3");
}
