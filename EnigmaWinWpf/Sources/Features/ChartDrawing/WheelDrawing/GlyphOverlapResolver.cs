// GlyphOverlapResolver.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Adjusts plotAngles of WheelPlotItems so that no two glyphs overlap on the wheel.
/// </summary>
public static class GlyphOverlapResolver
{
    private const int MaxIterations = 10;

    public static WheelPlotItem[] Resolve(IReadOnlyList<WheelPlotItem> items)
    {
        if (items.Count == 0) return [];

        var angles = new double[items.Count];
        for (var i = 0; i < items.Count; i++)
            angles[i] = items[i].MundaneAngle;

        for (var iter = 0; iter < MaxIterations; iter++)
        {
            var changed = false;
            for (var i = 0; i < angles.Length; i++)
            {
                for (var j = i + 1; j < angles.Length; j++)
                {
                    var diff = AngularDifferenceSigned(angles[i], angles[j]);
                    var absDiff = Math.Abs(diff);
                    if (absDiff < WheelMetrics.MinGlyphDistance)
                    {
                        var push = (WheelMetrics.MinGlyphDistance - absDiff) / 2.0;
                        if (diff >= 0)
                        {
                            angles[i] = WheelGeometry.Normalise(angles[i] - push);
                            angles[j] = WheelGeometry.Normalise(angles[j] + push);
                        }
                        else
                        {
                            angles[i] = WheelGeometry.Normalise(angles[i] + push);
                            angles[j] = WheelGeometry.Normalise(angles[j] - push);
                        }
                        changed = true;
                    }
                }
            }
            if (!changed) break;
        }

        var result = new WheelPlotItem[items.Count];
        for (var i = 0; i < items.Count; i++)
            result[i] = items[i].WithPlotAngle(angles[i]);
        return result;
    }

    private static double AngularDifferenceSigned(double a, double b)
    {
        var diff = (b - a) % 360.0;
        if (diff > 180.0) diff -= 360.0;
        if (diff <= -180.0) diff += 360.0;
        return diff;
    }
}
