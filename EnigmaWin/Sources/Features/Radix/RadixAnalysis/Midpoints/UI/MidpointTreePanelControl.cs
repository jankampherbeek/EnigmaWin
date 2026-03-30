// MidpointTreePanelControl.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints.UI;

/// <summary>
/// Lays out one <see cref="MidpointTreeControl"/> per matching factor,
/// wrapping onto new rows when the available width is exhausted.
/// Items are aligned to the top.
/// </summary>
public sealed class MidpointTreePanelControl : Panel
{
    private const double TreeW   = 72;
    private const double Spacing = 12;

    public static readonly StyledProperty<IReadOnlyList<MidpointMatch>> MatchesProperty =
        AvaloniaProperty.Register<MidpointTreePanelControl, IReadOnlyList<MidpointMatch>>(
            nameof(Matches), defaultValue: []);

    public IReadOnlyList<MidpointMatch> Matches
    {
        get => GetValue(MatchesProperty);
        set => SetValue(MatchesProperty, value);
    }

    static MidpointTreePanelControl()
    {
        MatchesProperty.Changed.AddClassHandler<MidpointTreePanelControl>(
            (c, _) => c.RebuildChildren());
    }

    private void RebuildChildren()
    {
        Children.Clear();
        foreach (var (factor, pairs) in GroupedByFactor(Matches))
        {
            Children.Add(new MidpointTreeControl
            {
                MatchingFactor = factor,
                Pairs = pairs
            });
        }
        InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var x = 0.0;
        var y = 0.0;
        var rowH = 0.0;

        foreach (Control child in Children)
        {
            child.Measure(availableSize);
            var w = child.DesiredSize.Width + Spacing;
            var h = child.DesiredSize.Height;

            if (x > 0 && x + w > availableSize.Width)
            {
                x = 0;
                y += rowH + Spacing;
                rowH = 0;
            }

            x += w;
            if (h > rowH) rowH = h;
        }

        return new Size(availableSize.Width, y + rowH);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var x = 0.0;
        var y = 0.0;
        var rowH = 0.0;

        // First pass: collect row heights
        var rows = new List<(Control child, double cx, double cy)>();
        var rowStart = 0;

        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            var w = child.DesiredSize.Width + Spacing;
            var h = child.DesiredSize.Height;

            if (x > 0 && x + w > finalSize.Width)
            {
                // Flush current row
                for (var j = rowStart; j < rows.Count; j++)
                    rows[j] = (rows[j].child, rows[j].cx, y);
                x = 0;
                y += rowH + Spacing;
                rowH = 0;
                rowStart = rows.Count;
            }

            rows.Add((child, x, y));
            x += w;
            if (h > rowH) rowH = h;
        }

        // Arrange all
        foreach (var (child, cx, cy) in rows)
        {
            child.Arrange(new Rect(cx, cy, child.DesiredSize.Width, child.DesiredSize.Height));
        }

        return finalSize;
    }

    /// <summary>
    /// Groups matches by MatchingFactor, preserving first-appearance order
    /// and deduplicating factor pairs within each group.
    /// </summary>
    private static List<(Factors Factor, List<(Factors F1, Factors F2)> Pairs)>
        GroupedByFactor(IReadOnlyList<MidpointMatch> matches)
    {
        var order  = new List<Factors>();
        var seen   = new Dictionary<Factors, HashSet<string>>();
        var result = new Dictionary<Factors, List<(Factors, Factors)>>();

        foreach (var m in matches)
        {
            if (!result.ContainsKey(m.MatchingFactor))
            {
                order.Add(m.MatchingFactor);
                result[m.MatchingFactor] = [];
                seen[m.MatchingFactor]   = [];
            }
            var key = $"{(int)m.Factor1}-{(int)m.Factor2}";
            if (seen[m.MatchingFactor].Add(key))
                result[m.MatchingFactor].Add((m.Factor1, m.Factor2));
        }

        var grouped = new List<(Factors, List<(Factors, Factors)>)>();
        foreach (var f in order)
            grouped.Add((f, result[f]));
        return grouped;
    }
}
