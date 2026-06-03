// LogTimeScaleOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Features.Progressive.LogTimeScale;

/// <summary>Implements Tad Mann's Logarithmic Time Scale calculations.</summary>
public sealed class LogTimeScaleOrchestrator
{
    private const double Z = 0.0766835;   // lunar-month unit: converts between lunar months and years

    /// <summary>Converts a horoscope position in degrees to age in years via Mann's algorithm.</summary>
    public double MannAgeFromPosition(double position, double ascendant)
    {
        var diff = position - ascendant;
        if (diff < 0) diff += 360.0;

        var withGestation = diff + 120.0;
        var divided       = withGestation / 120.0;
        var antilog       = Math.Pow(10.0, divided);
        var shifted       = antilog - 10.0;
        return shifted * Z;
    }

    /// <summary>Converts age in years to horoscope position in degrees (reverse of Mann's algorithm).</summary>
    public double MannPositionFromAge(double age, double ascendant)
    {
        var antilog       = age / Z + 10.0;
        var divided       = Math.Log10(antilog);
        var withGestation = divided * 120.0;
        var diff          = withGestation - 120.0;

        var position = ascendant + diff;
        while (position >= 360.0) position -= 360.0;
        while (position < 0.0)    position += 360.0;
        return position;
    }
}
