// SpeedOrchestrator.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using System;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Speed;

/// <summary>Determines the SpeedType for a celestial factor based on its current speed.</summary>
public static class SpeedOrchestrator
{
    /// <summary>
    /// Determines the speed type for a factor.
    /// </summary>
    /// <param name="speed">The current daily speed in decimal degrees (negative = retrograde).</param>
    /// <param name="factor">The celestial factor.</param>
    /// <param name="config">The calculation configuration containing stationary and slow percentages.</param>
    /// <returns>The corresponding SpeedType.</returns>
    public static SpeedType Determine(double speed, Factors factor, CalculationConfig config)
    {
        bool isRetrograde = speed < 0.0;
        double absSpeed = Math.Abs(speed);

        double? average = AverageSpeed.AverageFor(factor);
        if (average is null)
            return isRetrograde ? SpeedType.Retrograde : SpeedType.Direct;

        double stationaryThreshold = average.Value * config.StationaryPercentage / 100.0;
        double slowThreshold = average.Value * config.SlowPercentage / 100.0;

        if (absSpeed <= stationaryThreshold)
            return isRetrograde ? SpeedType.StationaryRetrograde : SpeedType.StationaryDirect;

        if (absSpeed <= slowThreshold)
            return isRetrograde ? SpeedType.SlowRetrograde : SpeedType.Slow;

        return isRetrograde ? SpeedType.Retrograde : SpeedType.Direct;
    }
}
