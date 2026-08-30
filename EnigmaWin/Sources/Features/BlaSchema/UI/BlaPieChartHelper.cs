// BlaPieChartHelper.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ScottPlot.WPF;
using WpfColor = System.Windows.Media.Color;

namespace EnigmaWin.Sources.Features.BlaSchema.UI;

/// <summary>Renders a donut chart for a set of (label, value) counts, with a plain-WPF colored legend
/// underneath it (ScottPlot's own legend can't be verified visually in this environment, so the legend
/// is built from ordinary WPF elements instead). Shared by the Counts and Dispositors sections.
/// Zero-value entries are dropped so an empty slice never claims a color.</summary>
public static class BlaPieChartHelper
{
    private static readonly string[] PaletteHex =
    [
        "#4E79A7", "#F28E2B", "#E15759", "#76B7B2", "#59A14F", "#EDC948",
        "#B07AA1", "#FF9DA7", "#9C755F", "#BAB0AC", "#86BCB6", "#D4A6C8"
    ];

    public static void Render(WpfPlot plot, Panel legendPanel, IReadOnlyList<(string Label, double Value)> data)
    {
        plot.Plot.Clear();
        legendPanel.Children.Clear();

        var nonZero = data.Where(d => d.Value > 0).ToList();
        if (nonZero.Count > 0)
        {
            var slices = new List<ScottPlot.PieSlice>();
            for (var i = 0; i < nonZero.Count; i++)
            {
                slices.Add(new ScottPlot.PieSlice
                {
                    Value = nonZero[i].Value,
                    FillColor = ScottPlot.Color.FromHex(PaletteHex[i % PaletteHex.Length])
                    // No on-chart Label: labels are rendered as a WPF legend underneath instead.
                });
                legendPanel.Children.Add(BuildLegendItem(nonZero[i].Label, nonZero[i].Value, WpfColorAt(i)));
            }

            var pie = plot.Plot.Add.Pie(slices);
            pie.DonutFraction = 0.55;
        }

        plot.Plot.Axes.Frameless();
        plot.Plot.HideGrid();
        plot.Refresh();
    }

    private static WpfColor WpfColorAt(int index) =>
        (WpfColor)ColorConverter.ConvertFromString(PaletteHex[index % PaletteHex.Length]);

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
