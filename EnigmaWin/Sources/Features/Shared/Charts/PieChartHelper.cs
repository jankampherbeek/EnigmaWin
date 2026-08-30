// PieChartHelper.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ScottPlot.WPF;
using WpfColor = System.Windows.Media.Color;

namespace EnigmaWin.Sources.Features.Shared.Charts;

/// <summary>Renders a donut chart for a set of (label, value) counts, with a plain-WPF colored legend
/// underneath it (ScottPlot's own legend can't be verified visually in this environment, so the legend
/// is built from ordinary WPF elements instead). Shared by every screen that shows count breakdowns
/// (BLA schema, Countings, ...). Zero-value entries are dropped so an empty slice never claims a color.</summary>
public static class PieChartHelper
{
    private static readonly string[] DefaultPaletteHex =
    [
        "#4E79A7", "#F28E2B", "#E15759", "#76B7B2", "#59A14F", "#EDC948",
        "#B07AA1", "#FF9DA7", "#9C755F", "#BAB0AC", "#86BCB6", "#D4A6C8"
    ];

    /// <summary>Renders slices colored from a generic categorical palette, assigned by position.</summary>
    public static void Render(WpfPlot plot, Panel legendPanel, IReadOnlyList<(string Label, double Value)> data)
    {
        var withColors = data
            .Select((d, i) => (d.Label, d.Value, DefaultWpfColorAt(i)))
            .ToList();
        RenderWithColors(plot, legendPanel, withColors);
    }

    /// <summary>Renders slices using the given explicit color per entry (e.g. fixed, meaningful colors
    /// such as the traditional element/modality colors).</summary>
    public static void RenderWithColors(WpfPlot plot, Panel legendPanel, IReadOnlyList<(string Label, double Value, WpfColor Color)> data)
    {
        plot.Plot.Clear();
        legendPanel.Children.Clear();

        var nonZero = data.Where(d => d.Value > 0).ToList();
        if (nonZero.Count > 0)
        {
            var slices = new List<ScottPlot.PieSlice>();
            foreach (var entry in nonZero)
            {
                slices.Add(new ScottPlot.PieSlice
                {
                    Value = entry.Value,
                    FillColor = ScottPlot.Color.FromHex(ToHex(entry.Color))
                    // No on-chart Label: labels are rendered as a WPF legend underneath instead.
                });
                legendPanel.Children.Add(BuildLegendItem(entry.Label, entry.Value, entry.Color));
            }

            var pie = plot.Plot.Add.Pie(slices);
            pie.DonutFraction = 0.55;

            // ScottPlot's default auto-scaled axis limits leave a large, mostly-empty margin around
            // the pie (it doesn't scale to fill the plot area on its own). The pie's slices span a
            // unit circle (radius 1), so tight limits just outside that make it fill the control.
            plot.Plot.Axes.SetLimits(-1.1, 1.1, -1.1, 1.1);
        }

        plot.Plot.Axes.Frameless();
        plot.Plot.HideGrid();
        plot.Refresh();
    }

    private static WpfColor DefaultWpfColorAt(int index) =>
        (WpfColor)ColorConverter.ConvertFromString(DefaultPaletteHex[index % DefaultPaletteHex.Length]);

    private static string ToHex(WpfColor color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    private static UIElement BuildLegendItem(string label, double value, WpfColor color)
    {
        var swatch = new Border
        {
            Width = 12,
            Height = 12,
            Background = new SolidColorBrush(color),
            Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        var text = new TextBlock
        {
            Text = $"{label} ({value:0.#})",
            VerticalAlignment = VerticalAlignment.Center
        };
        var row = new StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            Margin = new Thickness(0, 0, 12, 4)
        };
        row.Children.Add(swatch);
        row.Children.Add(text);
        return row;
    }
}
