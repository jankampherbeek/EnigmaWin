// CountingsColors.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows.Media;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Countings.UI;

/// <summary>Fixed, non-cycled colors per group — traditional astrological element/cross colors,
/// matching the Apple app's system-color choices.</summary>
public static class CountingsColors
{
    public static Color ForGroup(CountingsGroup group) => group switch
    {
        CountingsGroup.Cardinal => (Color)ColorConverter.ConvertFromString("#FF9500"),
        CountingsGroup.Fixed => (Color)ColorConverter.ConvertFromString("#5856D6"),
        CountingsGroup.Mutable => (Color)ColorConverter.ConvertFromString("#30B0C7"),
        CountingsGroup.Fire => (Color)ColorConverter.ConvertFromString("#FF3B30"),
        CountingsGroup.Earth => (Color)ColorConverter.ConvertFromString("#A2845E"),
        CountingsGroup.Air => (Color)ColorConverter.ConvertFromString("#FFCC00"),
        CountingsGroup.Water => (Color)ColorConverter.ConvertFromString("#007AFF"),
        _ => Colors.Gray
    };
}
