// ApogeeDuvalCalc.cs
// EnigmaWin
// Created by porting from ApogeeDuvalCalc.swift on 27-01-2026

using System;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWin.Sources.Features.AstronCalc;

/// <summary>
/// Calculate corrected apogee using Duval's formula.
/// </summary>
public class ApogeeDuvalCalc
{
    /// <summary>
    /// Calculate corrected apogee using Duval's formula.
    /// </summary>
    /// <param name="julianDay">Julian day for UT.</param>
    /// <param name="seWrapper">SEWrapper instance for calculations (not used directly, but kept for API compatibility).</param>
    /// <returns>Longitude in degrees (0-360).</returns>
    public double CalcApogeeDuval(double julianDay, SEWrapper seWrapper)
    {
        const int flagsEcl = 2 + 256;  // use SE + speed
        
        var sunPosition = SEWrapper.CalculateFactorPosition(
            julianDay,
            Factors.Sun.SeId(),
            flagsEcl
        );
        
        if (sunPosition == null)
        {
            return 0.0;
        }
        
        var longSun = sunPosition.MainPos;

        var apogeeMeanPosition = SEWrapper.CalculateFactorPosition(
            julianDay,
            Factors.ApogeeMean.SeId(),
            flagsEcl
        );
        
        if (apogeeMeanPosition == null)
        {
            return 0.0;
        }
        
        var longApogeeMean = apogeeMeanPosition.MainPos;
        var diff = RangeUtil.ValueToRange(longSun - longApogeeMean, -180.0, 180.0);

        // Calculate correction factor
        const double factor1 = 12.37;
        var sin2Diff = Math.Sin(MathExtra.DegToRad(2 * diff));
        var factor2 = Math.Sin(MathExtra.DegToRad(2 * (diff - 11.726 * sin2Diff)));
        var sin6Diff = Math.Sin(MathExtra.DegToRad(6 * diff));
        var factor3 = (8.8 / 60.0) * sin6Diff;
        var corrFactor = factor1 * factor2 + factor3;
        return RangeUtil.ValueToRange(longApogeeMean + corrFactor, 0.0, 360.0);
    }
}
