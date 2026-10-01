// SignColorSelector.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Selects the background colors for zodiac signs in chart wheels.</summary>
public static class SignColorSelector
{
    private static Dictionary<Signs, Color> _overrides = [];

    /// <summary>Applies the user-configured sign colors. Call this whenever the active config changes.</summary>
    public static void Configure(DisplayConfig display)
    {
        _overrides = [];
        foreach (var sc in display.SignColors)
            _overrides[sc.Sign] = ToColor(sc.Color);
    }

    /// <summary>Returns the background color for a sign, preferring the user-configured choice.</summary>
    public static Color GetColorForSign(Signs sign) =>
        _overrides.TryGetValue(sign, out var c) ? c : ToColor(ColorConfig.DefaultSignColor(sign));

    private static Color ToColor(ColorConfig c) => Color.FromArgb(
        ToByte(c.Opacity), ToByte(c.Red), ToByte(c.Green), ToByte(c.Blue));

    private static byte ToByte(double value) => (byte)Math.Clamp((int)Math.Round(value * 255), 0, 255);
}
