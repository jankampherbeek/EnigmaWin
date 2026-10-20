// GlyphOverlapResolver.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Adjusts plotAngles of WheelPlotItems so that no two glyphs overlap on the wheel.
/// The mundaneAngle (exact position) is left untouched; a connect line links both.
/// </summary>
public static class GlyphOverlapResolver
{
    public static WheelPlotItem[] Resolve(IReadOnlyList<WheelPlotItem> items)
    {
        if (items.Count == 0) return [];

        var angles = ResolvedPlotAngles([.. items.Select(i => i.MundaneAngle)]);
        var result = new WheelPlotItem[items.Count];
        for (var i = 0; i < items.Count; i++)
            result[i] = items[i].WithPlotAngle(angles[i]);
        return result;
    }

    /// <summary>
    /// Spreads a set of angles (in degrees, any range) so that neighbours are at least
    /// <paramref name="minDistance"/> apart, while keeping every glyph as close as possible to its true angle.
    /// Isolated angles are left exactly where they are; only crowded clusters are fanned out, and each
    /// cluster stays centred on its original centre of mass. Handles the full circle (including wrap-around)
    /// and coincident angles. Returns the adjusted angles in the same order as the input.
    /// Use this directly to resolve overlap between glyphs of different kinds, such as factors and cusp numbers.
    /// </summary>
    public static double[] ResolvedPlotAngles(IReadOnlyList<double> angles,
                                              double minDistance = WheelMetrics.MinGlyphDistance)
    {
        var n = angles.Count;
        if (n == 0) return [];
        if (n == 1) return [angles[0]];

        // The separation can never exceed an even distribution around the whole circle.
        var gap = Math.Min(minDistance, 360.0 / n);

        var norm  = angles.Select(WheelGeometry.Normalise).ToArray();
        var order = Enumerable.Range(0, n).OrderBy(i => norm[i]).ToArray();

        // Put the "seam" at the largest gap between neighbours, so the circle can be treated as a
        // line: walking from the seam, positions only ever increase and never need to wrap.
        var seam   = 0;
        var maxGap = 360.0 - (norm[order[n - 1]] - norm[order[0]]);   // wrap-around gap
        for (var k = 1; k < n; k++)
        {
            var g = norm[order[k]] - norm[order[k - 1]];
            if (g > maxGap) { maxGap = g; seam = k; }
        }

        // Linearise from the seam into non-decreasing offsets in [0, 360).
        var start = norm[order[seam]];
        var lin   = new double[n];
        for (var k = 0; k < n; k++)
        {
            var a = norm[order[(seam + k) % n]] - start;
            if (a < 0) a += 360.0;
            lin[k] = a;
        }

        // Forward pass: enforce the minimum gap (only ever pushes elements to the right).
        var x = (double[])lin.Clone();
        for (var k = 1; k < n; k++)
            x[k] = Math.Max(x[k], x[k - 1] + gap);

        // Re-centre each tightly packed run on its original centre of mass, so isolated glyphs stay
        // put and clusters fan out symmetrically around their true position rather than drifting.
        var r = 0;
        while (r < n)
        {
            var j = r;
            while (j + 1 < n && Math.Abs(x[j + 1] - x[j] - gap) < 1e-6) j++;
            if (j > r)
            {
                var count      = j - r + 1;
                var origCentre = lin[r..(j + 1)].Sum() / count;
                var curCentre  = x[r..(j + 1)].Sum() / count;
                var shift      = origCentre - curCentre;                          // <= 0: runs only move left
                if (r > 0) shift = Math.Max(shift, x[r - 1] + gap - x[r]);        // never hit the left neighbour
                if (shift != 0)
                    for (var m = r; m <= j; m++) x[m] += shift;
            }
            r = j + 1;
        }

        // Map back to the input order, as absolute normalised angles.
        var result = new double[n];
        for (var k = 0; k < n; k++)
            result[order[(seam + k) % n]] = WheelGeometry.Normalise(start + x[k]);
        return result;
    }
}
